using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Cosmos;
using VendorMdm.Api.Data;
using VendorMdm.Api.Models; // DTOs and Cosmos entities
using VendorMdm.Core.Framework.Primitives;
using VendorMdm.Shared.Models; // SQL entities
using VendorMdm.Shared.Models.Sanctions;
using CosmosModels = VendorMdm.Shared.Models; // Alias for disambiguation
using VendorMdm.Core.Framework.Logging; // Added for IStructuredLogger
// State machine alias to avoid conflict with VendorMdm.Shared.Models.InvitationStatus
using InvitationStatusSM = VendorMdm.Shared.Constants.InvitationStatus;

namespace VendorMdm.Api.Services;

/// <summary>
/// Invitation Service Interface - Brain v1.2.0 Compliant
/// Pattern 4: Result Pattern - Returns Result instead of throwing exceptions for business logic.
/// </summary>
public interface IInvitationService
{
    Task<Result<CreateInvitationResponse>> CreateInvitationAsync(CreateInvitationRequest request, Guid invitedBy, string invitedByName);
    Task<ValidateInvitationResponse> ValidateInvitationAsync(string token);
    Task<VendorInvitation?> GetInvitationByTokenAsync(string token);
    Task<InvitationListResponse> GetInvitationsAsync(int page = 1, int pageSize = 20, string? status = null);
    Task<bool> CompleteInvitationAsync(string token, Guid vendorApplicationId);
    Task<(bool Success, string? Link, bool EmailSent, string? EmailError)> ResendInvitationAsync(Guid invitationId, Guid requestedBy);
    Task<bool> TriggerMfaAsync(string token);
    Task<VerifyMfaResponse> VerifyMfaCodeAsync(string token, string code);
    Task<bool> SubmitInitialInfoAsync(string token, Dictionary<string, object> initialInfo);
    Task<bool> SubmitEnrichmentAsync(string token, Dictionary<string, object> enrichmentData);
    Task<bool> CancelInvitationAsync(Guid invitationId, Guid requestedBy);
    Task ExpireOldInvitationsAsync(); // Background task to expire old invitations
}

public class InvitationService : IInvitationService
{
    private readonly SqlDbContext _context;
    private readonly IStructuredLogger _logger; // Changed from ILogger
    private readonly IServiceBusService _serviceBusService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly Container _cosmosArtifactsContainer;
    private readonly Container _cosmosEventsContainer;
    private readonly IAuditLogService _auditLog;
    private readonly ISanctionsScreeningService _sanctionsService;

    public InvitationService(
        SqlDbContext context, 
        IStructuredLogger logger, // Changed from ILogger
        IServiceBusService serviceBusService,
        IEmailService emailService,
        IConfiguration configuration,
        ISanctionsScreeningService sanctionsService,
        CosmosClient cosmosClient,
        IAuditLogService auditLog)
    {
        _context = context;
        _logger = logger;
        _serviceBusService = serviceBusService;
        _emailService = emailService;
        _configuration = configuration;
        _sanctionsService = sanctionsService;
        _cosmosArtifactsContainer = cosmosClient.GetContainer("VendorMdm", "InvitationArtifacts");
        _cosmosEventsContainer = cosmosClient.GetContainer("VendorMdm", "DomainEvents");
        _auditLog = auditLog;
    }

    public async Task<Result<CreateInvitationResponse>> CreateInvitationAsync(
        CreateInvitationRequest request,
        Guid invitedBy,
        string invitedByName)
    {
        // Validate input
        if (request == null)
            return Result.Fail<CreateInvitationResponse>("Request is required");

        if (string.IsNullOrEmpty(request.PrimaryContactEmail))
            return Result.Fail<CreateInvitationResponse>("Primary contact email is required");

        if (string.IsNullOrEmpty(request.VendorLegalName))
            return Result.Fail<CreateInvitationResponse>("Vendor legal name is required");

        // Check for existing pending invitation with same email
        var existingInvitation = await _context.VendorInvitations
            .Where(i => i.PrimaryContactEmail == request.PrimaryContactEmail
                     && (i.Status == InvitationStatus.Pending || i.Status == InvitationStatus.Accepted))
            .FirstOrDefaultAsync();

        if (existingInvitation != null)
        {
            return Result.Fail<CreateInvitationResponse>(
                $"An active invitation already exists for {request.PrimaryContactEmail}");
        }

        // Check for existing application with same email
        var existingApplication = await _context.VendorApplications
            .Where(a => a.ContactEmail == request.PrimaryContactEmail)
            .FirstOrDefaultAsync();

        if (existingApplication != null)
        {
            return Result.Fail<CreateInvitationResponse>(
                $"A vendor application already exists for {request.PrimaryContactEmail}");
        }

        // 3. Pre-Invitation Sanctions Screening
        try
        {
            var screeningRequest = new VendorMdm.Shared.Models.Sanctions.ScreeningRequest
            {
                EntityName = request.VendorLegalName,
                EntityType = request.VendorType == "Individual" ? "Person" : "Organization",
                Address = new VendorMdm.Shared.Models.Sanctions.AddressInfo { Country = "US" } // Default for initial check
            };

            var screeningResult = await _sanctionsService.ScreenEntityAsync(screeningRequest);
            
            // Allow creation even if potential match, but log it
            if (screeningResult.OverallRisk == RiskLevel.High || screeningResult.OverallRisk == RiskLevel.Critical)
            {
                _logger.LogWarning("Pre-invitation sanctions match found", 
                    ("Email", request.PrimaryContactEmail), 
                    ("RiskLevel", screeningResult.OverallRisk), 
                    ("ScreeningId", screeningResult.ScreeningId));
             }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pre-invitation screening failed - proceeding with invitation", ("Email", request.PrimaryContactEmail));
            // Fail-open for now on creation, we screen again later
        }

        // Create Invitation
        // Build attributes including internal data fields (used by MDU for pre-population)
        var attributesObj = new Dictionary<string, object?>
        {
            ["RequestNotes"] = request.Notes,
            ["InitialMarket"] = "US", // Default
            ["IsVip"] = false
        };

        // Add internal data fields if provided
        if (!string.IsNullOrEmpty(request.Currency))
            attributesObj["Currency"] = request.Currency;

        if (!string.IsNullOrEmpty(request.SapLanguage))
            attributesObj["SapLanguage"] = request.SapLanguage;

        if (!string.IsNullOrEmpty(request.TaxCode1))
            attributesObj["TaxCode1"] = request.TaxCode1;

        if (!string.IsNullOrEmpty(request.TaxCode2))
            attributesObj["TaxCode2"] = request.TaxCode2;

        if (!string.IsNullOrEmpty(request.PermittedPayee))
            attributesObj["PermittedPayee"] = request.PermittedPayee;

        var invitation = new VendorInvitation
        {
            Id = Guid.NewGuid(),
            InvitationToken = GenerateSecureToken(),
            VendorLegalName = request.VendorLegalName,
            PrimaryContactEmail = request.PrimaryContactEmail,
            InvitedBy = invitedBy,
            InvitedByName = invitedByName,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(request.ExpirationDays > 0 ? request.ExpirationDays : 14), // Default 14 days
            Status = InvitationStatus.Pending,
            VendorType = request.VendorType ?? "Company",
            AccountGroup = request.AccountGroup ?? "Z001",
            CurrentStage = InvitationStage.InvitationSent,
            Attributes = JsonSerializer.Serialize(attributesObj)
        };

        _context.VendorInvitations.Add(invitation);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Invitation created", ("InvitationId", invitation.Id), ("Email", invitation.PrimaryContactEmail));

        // AUDIT LOG
        await _auditLog.LogAsync(
            entityType: "VendorInvitation",
            entityId: invitation.Id,
            action: "Created",
            oldValues: null,
            newValues: request,
            reason: "Initial user request");

        // SERVICE BUS: Queue Email
        try
        {
            await _serviceBusService.PublishEventAsync("InvitationCreated", new 
            { 
                InvitationId = invitation.Id,
                Email = invitation.PrimaryContactEmail,
                Token = invitation.InvitationToken
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish InvitationCreated event", ("InvitationId", invitation.Id));
            // Don't fail the request, email might be sent via fallback or retry
        }

        // SEND EMAIL DIRECTLY (Fallback/Immediate)
        var emailData = new InvitationEmailData
        {
            InvitationId = invitation.Id.ToString(),
            VendorName = invitation.VendorLegalName,
            Email = invitation.PrimaryContactEmail,
            Token = invitation.InvitationToken,
            ExpiresAt = invitation.ExpiresAt,
            InvitedByName = invitation.InvitedByName,
            CompanyName = "UNESCO Vendor Portal", // Configurable
            Notes = request.Notes,
            // BaseUrl will be resolved by EmailService from config
        };

        var (emailSuccess, emailError) = await _emailService.SendInvitationEmailAsync(emailData);

        if (!emailSuccess)
        {
            _logger.LogWarning("Failed to send invitation email immediately", 
                ("InvitationId", invitation.Id), 
                ("Error", emailError ?? "Unknown"));
        }

        var baseUrl = _configuration["App:BaseUrl"] ?? "http://localhost:3000";
        var invitationLink = $"{baseUrl}/invitation/register/{invitation.InvitationToken}";

        return Result.Ok(new CreateInvitationResponse
        {
            InvitationId = invitation.Id,
            InvitationToken = invitation.InvitationToken,
            InvitationLink = invitationLink,
            ExpiresAt = invitation.ExpiresAt,
            EmailSent = emailSuccess,
            EmailError = emailSuccess ? null : emailError
        });
    }

    public async Task<ValidateInvitationResponse> ValidateInvitationAsync(string token)
    {
        var invitation = await _context.VendorInvitations
            .FirstOrDefaultAsync(i => i.InvitationToken == token);

        if (invitation == null)
        {
            _logger.LogWarning("Invalid invitation token attempt", ("TokenPrefix", token.Substring(0, Math.Min(5, token.Length))));
            return new ValidateInvitationResponse { IsValid = false, ErrorMessage = "Invalid token" };
        }

        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            // Update status to Expired if not already
            if (invitation.Status != InvitationStatus.Expired &&
                InvitationStatusSM.IsValidTransition(invitation.Status, InvitationStatus.Expired))
            {
                invitation.Status = InvitationStatus.Expired;
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Expired invitation token attempt", ("InvitationId", invitation.Id));
            return new ValidateInvitationResponse { IsValid = false, ErrorMessage = "Invitation expired" };
        }

        if (invitation.Status != InvitationStatus.Pending && invitation.Status != InvitationStatus.Accepted)
        {
            _logger.LogInformation("Invitation already completed or cancelled",
                ("InvitationId", invitation.Id),
                ("Status", invitation.Status));
            return new ValidateInvitationResponse
            {
                IsValid = false,
                ErrorMessage = $"Invitation is {invitation.Status}"
            };
        }

        return new ValidateInvitationResponse
        {
            IsValid = true,
            InvitationId = invitation.Id,
            VendorLegalName = invitation.VendorLegalName,
            PrimaryContactEmail = invitation.PrimaryContactEmail,
            Status = invitation.Status,
            CurrentStage = invitation.CurrentStage
        };
    }

    public async Task<VendorInvitation?> GetInvitationByTokenAsync(string token)
    {
        return await _context.VendorInvitations
            .FirstOrDefaultAsync(i => i.InvitationToken == token);
    }

    public async Task<InvitationListResponse> GetInvitationsAsync(int page = 1, int pageSize = 20, string? status = null)
    {
        var query = _context.VendorInvitations.AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(i => i.Status == status);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var invitations = items.Select(i => 
        {
            var emailSentIndex = true;
            if (!string.IsNullOrEmpty(i.Attributes) && i.Attributes.Contains("\"emailSent\":false"))
            {
                emailSentIndex = false;
            }

            return new InvitationListItem
            {
                Id = i.Id,
                VendorLegalName = i.VendorLegalName,
                PrimaryContactEmail = i.PrimaryContactEmail,
                Status = i.Status,
                CurrentStage = i.CurrentStage,
                InvitedByName = i.InvitedByName,
                CreatedAt = i.CreatedAt,
                ExpiresAt = i.ExpiresAt,
                VendorApplicationId = i.VendorApplicationId,
                EmailSent = emailSentIndex
            };
        }).ToList();

        return new InvitationListResponse
        {
            Invitations = invitations,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> TriggerMfaAsync(string token)
    {
        var invitation = await _context.VendorInvitations.FirstOrDefaultAsync(i => i.InvitationToken == token);
        if (invitation == null) return false;

        var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        
        // Update Attributes with MFA code
        var attributes = string.IsNullOrEmpty(invitation.Attributes) || invitation.Attributes == "{}" 
            ? new Dictionary<string, object>() 
            : JsonSerializer.Deserialize<Dictionary<string, object>>(invitation.Attributes) ?? new Dictionary<string, object>();
            
        attributes["mfaCode"] = code;
        attributes["mfaExpires"] = DateTime.UtcNow.AddMinutes(15);
        
        invitation.Attributes = JsonSerializer.Serialize(attributes);
        await _context.SaveChangesAsync();

        // Send Email
        await _emailService.SendMfaCodeEmailAsync(invitation.PrimaryContactEmail, invitation.VendorLegalName, code);
        
        _logger.LogInformation("MFA code sent", ("InvitationId", invitation.Id));
        return true;
    }

    public async Task<VerifyMfaResponse> VerifyMfaCodeAsync(string token, string code)
    {
        var invitation = await _context.VendorInvitations.FirstOrDefaultAsync(i => i.InvitationToken == token);
        if (invitation == null) return new VerifyMfaResponse { Success = false, Message = "Invalid token" };

        var attributes = string.IsNullOrEmpty(invitation.Attributes) || invitation.Attributes == "{}" 
             ? new Dictionary<string, object>() 
             : JsonSerializer.Deserialize<Dictionary<string, object>>(invitation.Attributes) ?? new Dictionary<string, object>();

        if (!attributes.TryGetValue("mfaCode", out var storedCodeObj) || 
            !attributes.TryGetValue("mfaExpires", out var expiresObj))
        {
             return new VerifyMfaResponse { Success = false, Message = "No MFA code active" };
        }

        var storedCode = storedCodeObj.ToString();
        var expires = DateTime.Parse(expiresObj.ToString()!);
        
        if (DateTime.UtcNow > expires)
        {
            return new VerifyMfaResponse { Success = false, Message = "Code expired" };
        }

        if (storedCode != code)
        {
            _logger.LogWarning("Invalid MFA code attempt", ("InvitationId", invitation.Id));
            return new VerifyMfaResponse { Success = false, Message = "Invalid code" };
        }

        // Success
        invitation.CurrentStage = InvitationStage.MfaVerified;
        // Clean up code
        attributes.Remove("mfaCode");
        attributes.Remove("mfaExpires");
        invitation.Attributes = JsonSerializer.Serialize(attributes);
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("MFA verified", ("InvitationId", invitation.Id));
        return new VerifyMfaResponse { Success = true };
    }

    public async Task<bool> SubmitInitialInfoAsync(string token, Dictionary<string, object> initialInfo)
    {
        var invitation = await _context.VendorInvitations.FirstOrDefaultAsync(i => i.InvitationToken == token);
        if (invitation == null) return false;

        // Merge info into attributes
        var attributes = string.IsNullOrEmpty(invitation.Attributes) || invitation.Attributes == "{}" 
             ? new Dictionary<string, object>() 
             : JsonSerializer.Deserialize<Dictionary<string, object>>(invitation.Attributes) ?? new Dictionary<string, object>();

        foreach (var kvp in initialInfo)
        {
            attributes[kvp.Key] = kvp.Value;
        }

        invitation.Attributes = JsonSerializer.Serialize(attributes);
        invitation.CurrentStage = InvitationStage.InitialInfoCompleted;
        // Optionally update strict columns if initialInfo contains them (e.g. LegalName correction)

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Initial info submitted", ("InvitationId", invitation.Id));
        return true;
    }

    public async Task<bool> SubmitEnrichmentAsync(string token, Dictionary<string, object> enrichmentData)
    {
        var invitation = await _context.VendorInvitations.FirstOrDefaultAsync(i => i.InvitationToken == token);
        if (invitation == null) return false;

         // Merge info into attributes
        var attributes = string.IsNullOrEmpty(invitation.Attributes) || invitation.Attributes == "{}" 
             ? new Dictionary<string, object>() 
             : JsonSerializer.Deserialize<Dictionary<string, object>>(invitation.Attributes) ?? new Dictionary<string, object>();

        foreach (var kvp in enrichmentData)
        {
            attributes[kvp.Key] = kvp.Value;
        }

        invitation.Attributes = JsonSerializer.Serialize(attributes);
        invitation.CurrentStage = InvitationStage.Enriched;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Enrichment data submitted", ("InvitationId", invitation.Id));
        return true;
    }

    public async Task<bool> CancelInvitationAsync(Guid invitationId, Guid requestedBy)
    {
        var invitation = await _context.VendorInvitations.FindAsync(invitationId);
        if (invitation == null) return false;

        // Validate state transition using state machine
        if (!InvitationStatusSM.IsValidTransition(invitation.Status, InvitationStatus.Cancelled))
        {
            _logger.LogWarning("Invalid state transition for cancellation",
                ("InvitationId", invitation.Id),
                ("CurrentStatus", invitation.Status),
                ("AllowedTransitions", string.Join(", ", InvitationStatusSM.GetAllowedTransitions(invitation.Status))));
            return false;
        }

        var previousStatus = invitation.Status;
        invitation.Status = InvitationStatus.Cancelled;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Invitation cancelled",
            ("InvitationId", invitation.Id),
            ("PreviousStatus", previousStatus),
            ("RequestedBy", requestedBy));
        return true;
    }

    public async Task<bool> CompleteInvitationAsync(string token, Guid vendorApplicationId)
    {
        var invitation = await _context.VendorInvitations
            .FirstOrDefaultAsync(i => i.InvitationToken == token);

        if (invitation == null) return false;

        // Validate state transition using state machine
        if (!InvitationStatusSM.IsValidTransition(invitation.Status, InvitationStatus.Completed))
        {
            _logger.LogWarning("Invalid state transition for completion",
                ("InvitationId", invitation.Id),
                ("CurrentStatus", invitation.Status),
                ("AllowedTransitions", string.Join(", ", InvitationStatusSM.GetAllowedTransitions(invitation.Status))));
            return false;
        }

        // A. SQL: Update invitation state
        var previousStatus = invitation.Status;
        invitation.Status = InvitationStatus.Completed;
        invitation.CompletedAt = DateTime.UtcNow;
        invitation.VendorApplicationId = vendorApplicationId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Invitation completed", 
            ("InvitationId", invitation.Id), 
            ("PreviousStatus", previousStatus), 
            ("ApplicationId", vendorApplicationId));

        // ✅ AUDIT LOG: Log invitation completion
        await _auditLog.LogAsync(
            entityType: "VendorInvitation",
            entityId: invitation.Id,
            action: "Completed",
            oldValues: new { Status = previousStatus },
            newValues: new { 
                Status = "Completed", 
                VendorApplicationId = vendorApplicationId,
                CompletedAt = DateTime.UtcNow
            },
            reason: "Vendor completed registration process");

        // B. COSMOS: Store completion artifact
        try
        {
            var completionArtifact = new InvitationCompletionArtifact
            {
                Id = Guid.NewGuid().ToString(),
                InvitationId = invitation.Id.ToString(),
                VendorApplicationId = vendorApplicationId.ToString(),
                CompletedAt = DateTime.UtcNow
            };

            await _cosmosArtifactsContainer.UpsertItemAsync(
                completionArtifact,
                new PartitionKey(invitation.Id.ToString()));

            _logger.LogInformation("Invitation completion artifact stored", ("InvitationId", invitation.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store completion artifact", ("InvitationId", invitation.Id));
        }

        // C. COSMOS: Emit domain event
        try
        {
            await EmitDomainEventAsync("InvitationCompleted", invitation.Id.ToString(), new
            {
                InvitationId = invitation.Id,
                VendorApplicationId = vendorApplicationId,
                CompletedAt = DateTime.UtcNow,
                VendorName = invitation.VendorLegalName,
                Email = invitation.PrimaryContactEmail
            });

            _logger.LogInformation("Domain event InvitationCompleted emitted", ("InvitationId", invitation.Id));
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Failed to emit completion event", ("InvitationId", invitation.Id));
        }

        return true;
    }

    public async Task<(bool Success, string? Link, bool EmailSent, string? EmailError)> ResendInvitationAsync(Guid invitationId, Guid requestedBy)
    {
        var invitation = await _context.VendorInvitations
            .FirstOrDefaultAsync(i => i.Id == invitationId);

        if (invitation == null)
        {
            return (false, null, false, "Invitation not found");
        }

        // Validate state transition - check if we can transition to Resent first
        var targetStatus = InvitationStatusSM.Resent;
        if (!InvitationStatusSM.IsValidTransition(invitation.Status, targetStatus))
        {
            // If can't transition to Resent, try Pending (for expired invitations)
            targetStatus = InvitationStatus.Pending;
            if (!InvitationStatusSM.IsValidTransition(invitation.Status, targetStatus))
            {
                return (false, null, false, $"Cannot resend invitation in status '{invitation.Status}'");
            }
        }

        // Generate new token and extend expiration
        invitation.InvitationToken = GenerateSecureToken();
        invitation.ExpiresAt = DateTime.UtcNow.AddDays(14);
        invitation.Status = targetStatus;
        invitation.CurrentStage = InvitationStage.InvitationSent;

        await _context.SaveChangesAsync();

        // D. SERVICE BUS: Queue email notification (async processing for production)
        try
        {
            await _serviceBusService.PublishEventAsync("InvitationResent", new 
            { 
                InvitationId = invitation.Id,
                Email = invitation.PrimaryContactEmail,
                NewToken = invitation.InvitationToken,
                ResentBy = requestedBy
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish InvitationResent event", ("InvitationId", invitation.Id));
        }

        // E. DIRECT EMAIL (Sync fallback/priority)
        var emailData = new InvitationEmailData
        {
            InvitationId = invitation.Id.ToString(),
            VendorName = invitation.VendorLegalName,
            Email = invitation.PrimaryContactEmail,
            Token = invitation.InvitationToken,
            ExpiresAt = invitation.ExpiresAt,
            InvitedByName = invitation.InvitedByName,
            CompanyName = "UNESCO Vendor Portal",
            Notes = "This is a resent invitation."
        };

        var (emailSuccess, emailError) = await _emailService.SendInvitationEmailAsync(emailData);

        var link = $"{_configuration["App:BaseUrl"]}/invitation/register/{invitation.InvitationToken}";

        _logger.LogInformation("Invitation resent", 
            ("InvitationId", invitation.Id), 
            ("Email", invitation.PrimaryContactEmail),
            ("ResentBy", requestedBy));

        return (true, link, emailSuccess, emailError);
    }

    public async Task ExpireOldInvitationsAsync()
    {
        var expiredInvitations = await _context.VendorInvitations
            .Where(i => i.Status == InvitationStatus.Pending && i.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

        var expiredCount = 0;
        foreach (var invitation in expiredInvitations)
        {
            // Validate state transition using state machine
            if (InvitationStatusSM.IsValidTransition(invitation.Status, InvitationStatus.Expired))
            {
                invitation.Status = InvitationStatus.Expired;
                expiredCount++;
                _logger.LogInformation("Invitation expired", ("InvitationId", invitation.Id));
            }
            else
            {
                _logger.LogWarning("Could not expire invitation - invalid state transition",
                    ("InvitationId", invitation.Id),
                    ("CurrentStatus", invitation.Status));
            }
        }

        if (expiredCount > 0)
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Expired overdue invitations", ("Count", expiredCount));
        }
    }

    // Helper methods
    private string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-").Replace("/", "_").Replace("=", ""); // URL safe
    }

    private async Task EmitDomainEventAsync(string eventName, string entityId, object payload)
    {
        var domainEvent = new DomainEvent
        {
            Id = Guid.NewGuid().ToString(),
            EventType = eventName,
            EntityId = entityId,
            Data = payload,
            Timestamp = DateTime.UtcNow,
            Source = "VendorMdm.Api"
        };

        await _cosmosEventsContainer.CreateItemAsync(domainEvent, new PartitionKey(entityId));
    }
}
