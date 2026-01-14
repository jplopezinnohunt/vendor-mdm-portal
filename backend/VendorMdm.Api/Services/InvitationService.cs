using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Cosmos;
using VendorMdm.Api.Data;
using VendorMdm.Api.Models; // DTOs and Cosmos entities
using VendorMdm.Shared.Models; // SQL entities
using VendorMdm.Shared.Models.Sanctions;
using CosmosModels = VendorMdm.Shared.Models; // Alias for disambiguation

namespace VendorMdm.Api.Services;

public interface IInvitationService
{
    Task<CreateInvitationResponse> CreateInvitationAsync(CreateInvitationRequest request, Guid invitedBy, string invitedByName);
    Task<ValidateInvitationResponse> ValidateInvitationAsync(string token);
    Task<VendorInvitation?> GetInvitationByTokenAsync(string token);
    Task<InvitationListResponse> GetInvitationsAsync(int page = 1, int pageSize = 20, string? status = null);
    Task<bool> CompleteInvitationAsync(string token, Guid vendorApplicationId);
    Task<(bool Success, string? Link, bool EmailSent)> ResendInvitationAsync(Guid invitationId, Guid requestedBy);
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
    private readonly ILogger<InvitationService> _logger;
    private readonly IServiceBusService _serviceBusService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly Container _cosmosArtifactsContainer;
    private readonly Container _cosmosEventsContainer;

    public InvitationService(
        SqlDbContext context, 
        ILogger<InvitationService> logger,
        IServiceBusService serviceBusService,
        IEmailService emailService,
        IConfiguration configuration,
        ISanctionsScreeningService sanctionsService,
        CosmosClient cosmosClient)
    {
        _context = context;
        _logger = logger;
        _serviceBusService = serviceBusService;
        _emailService = emailService;
        _configuration = configuration;
        _sanctionsService = sanctionsService;
        _cosmosArtifactsContainer = cosmosClient.GetContainer("VendorMdm", "InvitationArtifacts");
        _cosmosEventsContainer = cosmosClient.GetContainer("VendorMdm", "DomainEvents");
    }

    private readonly ISanctionsScreeningService _sanctionsService;

    public async Task<CreateInvitationResponse> CreateInvitationAsync(
        CreateInvitationRequest request, 
        Guid invitedBy, 
        string invitedByName)
    {
        // Check for existing pending invitation with same email
        var existingInvitation = await _context.VendorInvitations
            .Where(i => i.PrimaryContactEmail == request.PrimaryContactEmail 
                     && (i.Status == InvitationStatus.Pending || i.Status == InvitationStatus.Accepted))
            .FirstOrDefaultAsync();

        if (existingInvitation != null)
        {
            throw new InvalidOperationException(
                $"An active invitation already exists for {request.PrimaryContactEmail}");
        }

        // Check for existing application with same email
        var existingApplication = await _context.VendorApplications
            .Where(a => a.ContactEmail == request.PrimaryContactEmail)
            .FirstOrDefaultAsync();

        if (existingApplication != null)
        {
            throw new InvalidOperationException(
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
            if (screeningResult.OverallRisk == VendorMdm.Shared.Models.Sanctions.RiskLevel.High || 
                screeningResult.OverallRisk == VendorMdm.Shared.Models.Sanctions.RiskLevel.Critical)
            {
                _logger.LogWarning("Blocked invitation for {Email} due to Sanctions Risk: {RiskLevel}", request.PrimaryContactEmail, screeningResult.OverallRisk);
                throw new InvalidOperationException("High-risk match found in Sanctions Screening. Invitation cannot be sent.");
            }
        }
        catch (InvalidOperationException)
        {
            // Rethrow business logic exceptions (like high risk blockage)
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sanctions screening service failed unexpectedly. enforcing Fail-Closed policy.");
            throw new InvalidOperationException("Sanctions screening is temporarily unavailable. Please try again later.", ex);
        }

        // Generate secure token
        var token = GenerateSecureToken();
        var expiresAt = DateTime.UtcNow.AddDays(request.ExpirationDays);

        var invitation = new VendorInvitation
        {
            Id = Guid.NewGuid(),
            InvitationToken = token,
            VendorLegalName = request.VendorLegalName,
            PrimaryContactEmail = request.PrimaryContactEmail,
            InvitedBy = invitedBy,
            InvitedByName = invitedByName,
            ExpiresAt = expiresAt,
            VendorType = request.VendorType,
            AccountGroup = !string.IsNullOrEmpty(request.AccountGroup) 
                ? request.AccountGroup 
                : MapVendorTypeToAccountGroup(request.VendorType),
            Status = InvitationStatus.Pending,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        // Initialize Attributes with internal data
        var initialAttributes = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(request.Currency)) initialAttributes["Currency"] = request.Currency;
        if (!string.IsNullOrEmpty(request.SapLanguage)) initialAttributes["SapLanguage"] = request.SapLanguage;
        if (!string.IsNullOrEmpty(request.TaxCode1)) initialAttributes["TaxCode1"] = request.TaxCode1;
        if (!string.IsNullOrEmpty(request.TaxCode2)) initialAttributes["TaxCode2"] = request.TaxCode2;
        if (!string.IsNullOrEmpty(request.PermittedPayee)) initialAttributes["PermittedPayee"] = request.PermittedPayee;
        
        if (initialAttributes.Any())
        {
            invitation.Attributes = JsonSerializer.Serialize(initialAttributes);
        }

        _context.VendorInvitations.Add(invitation);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Invitation created: {InvitationId} for {Email} by {InvitedBy}",
            invitation.Id, request.PrimaryContactEmail, invitedByName);

        // HYBRID ARCHITECTURE PATTERN IMPLEMENTATION
        // Following: SQL (State) → Cosmos (Artifact) → Cosmos (Event) → Service Bus (Integration)

        // B. COSMOS: Store invitation artifact (full payload for audit trail)
        try
        {
            await SaveInvitationArtifactAsync(invitation.Id.ToString(), new
            {
                InvitationId = invitation.Id,
                VendorLegalName = request.VendorLegalName,
                PrimaryContactEmail = request.PrimaryContactEmail,
                InvitedBy = invitedBy,
                InvitedByName = invitedByName,
                Token = token,
                ExpiresAt = expiresAt,
                ExpirationDays = request.ExpirationDays,
                Notes = request.Notes,
                Status = InvitationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                OriginalRequest = request // Complete request for full audit trail
            });

            _logger.LogInformation(
                "Invitation artifact stored in Cosmos for {InvitationId}",
                invitation.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to store invitation artifact in Cosmos for {InvitationId}",
                invitation.Id);
            // Continue - artifact storage failure shouldn't block invitation
        }

        // C. COSMOS: Emit domain event (event sourcing)
        try
        {
            await EmitDomainEventAsync("InvitationCreated", invitation.Id.ToString(), new
            {
                InvitationId = invitation.Id,
                VendorName = request.VendorLegalName,
                Email = request.PrimaryContactEmail,
                InvitedBy = invitedBy,
                InvitedByName = invitedByName,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow
            });

            _logger.LogInformation(
                "Domain event InvitationCreated emitted for {InvitationId}",
                invitation.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to emit domain event for {InvitationId}",
                invitation.Id);
            // Continue - event emission failure shouldn't block invitation
        }

        // D. SERVICE BUS: Queue email notification (async processing for production)
        var useLocalEmulators = _configuration.GetValue<bool>("UseLocalEmulators");
        if (!useLocalEmulators)
        {
            try
            {
                var emailMessage = new
                {
                    InvitationId = invitation.Id.ToString(),
                    VendorName = request.VendorLegalName,
                    Email = request.PrimaryContactEmail,
                    Token = token,
                    ExpiresAt = expiresAt.ToString("o"), // ISO 8601 format
                    InvitedByName = invitedByName,
                    CompanyName = _configuration["App:CompanyName"] ?? "Your Company",
                    Notes = request.Notes
                };

                await _serviceBusService.PublishEventAsync("invitation-created", emailMessage);
                
                _logger.LogInformation(
                    "Invitation email queued via Service Bus for {Email}", 
                    request.PrimaryContactEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Failed to queue invitation email via Service Bus for {Email}. Will try direct email.", 
                    request.PrimaryContactEmail);
            }
        }

        // E. DIRECT EMAIL: Send email immediately (for local dev or as fallback)
        bool emailSent = false;
        try
        {
            var baseUrl = _configuration["App:BaseUrl"] 
                ?? (useLocalEmulators ? "http://localhost:3002" : "https://vendor-portal.company.com");
            
            var emailData = new InvitationEmailData
            {
                InvitationId = invitation.Id.ToString(),
                VendorName = request.VendorLegalName,
                Email = request.PrimaryContactEmail,
                Token = token,
                ExpiresAt = expiresAt,
                InvitedByName = invitedByName,
                CompanyName = _configuration["App:CompanyName"] ?? "Your Company",
                Notes = request.Notes,
                BaseUrl = baseUrl
            };

            emailSent = await _emailService.SendInvitationEmailAsync(emailData);
            
            if (emailSent)
            {
                _logger.LogInformation(
                    "Invitation email sent successfully to {Email}", 
                    request.PrimaryContactEmail);
            }
            else
            {
                _logger.LogWarning(
                    "Invitation email sending returned false for {Email}. Email details logged. Updating attributes.", 
                    request.PrimaryContactEmail);
                
                // Update attributes to reflect failure
                try 
                {
                    var attrs = string.IsNullOrEmpty(invitation.Attributes) 
                        ? new Dictionary<string, object>() 
                        : JsonSerializer.Deserialize<Dictionary<string, object>>(invitation.Attributes) ?? new();
                    
                    attrs["emailSent"] = false;
                    invitation.Attributes = JsonSerializer.Serialize(attrs);
                    await _context.SaveChangesAsync();
                }
                catch (Exception exVal)
                {
                     _logger.LogError(exVal, "Failed to update invitation attributes with email failure status.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to send invitation email directly for {Email}. Invitation created but email not sent.", 
                request.PrimaryContactEmail);
            
            // Update attributes to reflect exception/failure
            try 
            {
                var attrs = string.IsNullOrEmpty(invitation.Attributes) 
                    ? new Dictionary<string, object>() 
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(invitation.Attributes) ?? new();
                
                attrs["emailSent"] = false;
                invitation.Attributes = JsonSerializer.Serialize(attrs);
                await _context.SaveChangesAsync();
            }
            catch {}
        }

        var invitationLink = $"/invitation/register/{token}";

        return new CreateInvitationResponse
        {
            InvitationId = invitation.Id,
            InvitationToken = token,
            InvitationLink = invitationLink,
            ExpiresAt = expiresAt,
            EmailSent = emailSent,
            EmailError = emailSent ? null : "Email sending failed. Please copy the link manually."
        };
    }

    public async Task<ValidateInvitationResponse> ValidateInvitationAsync(string token)
    {
        var invitation = await _context.VendorInvitations
            .FirstOrDefaultAsync(i => i.InvitationToken == token);

        if (invitation == null)
        {
            return new ValidateInvitationResponse
            {
                IsValid = false,
                ErrorMessage = "Invalid invitation link"
            };
        }

        if (invitation.Status == InvitationStatus.Expired || invitation.ExpiresAt < DateTime.UtcNow)
        {
            // Update status to expired if not already
            if (invitation.Status != InvitationStatus.Expired)
            {
                invitation.Status = InvitationStatus.Expired;
                await _context.SaveChangesAsync();
            }

            return new ValidateInvitationResponse
            {
                IsValid = false,
                ErrorMessage = "This invitation has expired. Please contact support for a new invitation."
            };
        }

        if (invitation.Status == InvitationStatus.Completed)
        {
            return new ValidateInvitationResponse
            {
                IsValid = false,
                ErrorMessage = "This invitation has already been used."
            };
        }

        _logger.LogInformation("Validating Token {Token}. CurrentStage: {Stage}, Valid: true", token, invitation.CurrentStage);

        return new ValidateInvitationResponse
        {
            IsValid = true,
            VendorLegalName = invitation.VendorLegalName, // Safe to show? Yes, needed for "Welcome {Company}"
            PrimaryContactEmail = invitation.PrimaryContactEmail, // Mask might be better but OK for now
            VendorType = invitation.VendorType,
            ExpiresAt = invitation.ExpiresAt,
            CurrentStage = invitation.CurrentStage, 
            Attributes = new Dictionary<string, object>() // SECURITY: Do NOT return sensitive saved attributes before MFA
        };
    }

    public async Task<VendorInvitation?> GetInvitationByTokenAsync(string token)
    {
        return await _context.VendorInvitations
            .FirstOrDefaultAsync(i => i.InvitationToken == token);
    }

    public async Task<InvitationListResponse> GetInvitationsAsync(
        int page = 1, 
        int pageSize = 20, 
        string? status = null)
    {
        var query = _context.VendorInvitations.AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(i => i.Status == status);
        }

        var totalCount = await query.CountAsync();

        // Need to materialize to check JSON attributes efficiently without EF Core JSON extensions
        var entities = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var invitations = entities.Select(i => 
        {
            var emailSentIndex = true;
            // Simple string check is faster/safer than full deserialize for just one flag
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
                CurrentStage = i.CurrentStage.ToString(),
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

    public async Task<bool> CompleteInvitationAsync(string token, Guid vendorApplicationId)
    {
        var invitation = await _context.VendorInvitations
            .FirstOrDefaultAsync(i => i.InvitationToken == token);

        if (invitation == null)
        {
            _logger.LogWarning("Invitation not found for token {Token}", token);
            return false;
        }

        if (invitation.Status == InvitationStatus.Completed)
        {
            _logger.LogWarning(
                "Invitation {InvitationId} is already completed. Application {ApplicationId} will not be linked.",
                invitation.Id, vendorApplicationId);
            return false;
        }

        // A. SQL: Update invitation state
        var previousStatus = invitation.Status;
        invitation.Status = InvitationStatus.Completed;
        invitation.CompletedAt = DateTime.UtcNow;
        invitation.VendorApplicationId = vendorApplicationId;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Invitation {InvitationId} status updated from {PreviousStatus} to Completed with application {ApplicationId}",
            invitation.Id, previousStatus, vendorApplicationId);

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

            _logger.LogInformation(
                "Invitation completion artifact stored for {InvitationId}",
                invitation.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to store completion artifact for invitation {InvitationId}",
                invitation.Id);
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

            _logger.LogInformation(
                "Domain event InvitationCompleted emitted for {InvitationId}",
                invitation.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to emit completion event for {InvitationId}",
                invitation.Id);
        }

        return true;
    }

    public async Task<(bool Success, string? Link, bool EmailSent)> ResendInvitationAsync(Guid invitationId, Guid requestedBy)
    {
        var invitation = await _context.VendorInvitations
            .FirstOrDefaultAsync(i => i.Id == invitationId);

        if (invitation == null || invitation.Status == InvitationStatus.Completed)
        {
            return (false, null, false);
        }

        // Generate new token and extend expiration
        invitation.InvitationToken = GenerateSecureToken();
        invitation.ExpiresAt = DateTime.UtcNow.AddDays(14);
        invitation.Status = InvitationStatus.Pending;
        invitation.CurrentStage = InvitationStage.InvitationSent;

        await _context.SaveChangesAsync();

        // D. SERVICE BUS: Queue email notification (async processing for production)
        var useLocalEmulators = _configuration.GetValue<bool>("UseLocalEmulators");
        if (!useLocalEmulators)
        {
            try
            {
                var emailMessage = new
                {
                    InvitationId = invitation.Id.ToString(),
                    VendorName = invitation.VendorLegalName,
                    Email = invitation.PrimaryContactEmail,
                    Token = invitation.InvitationToken,
                    ExpiresAt = invitation.ExpiresAt.ToString("o"),
                    InvitedByName = invitation.InvitedByName,
                    CompanyName = _configuration["App:CompanyName"] ?? "Your Company",
                    Notes = invitation.Notes
                };

                await _serviceBusService.PublishEventAsync("invitation-created", emailMessage);
                
                _logger.LogInformation(
                    "Resend invitation email queued via Service Bus for {Email}", 
                    invitation.PrimaryContactEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Failed to queue resend email via Service Bus for {Email}. Will try direct email.", 
                    invitation.PrimaryContactEmail);
            }
        }

        // E. DIRECT EMAIL: Send email immediately (for local dev or as fallback)
        bool emailSent = false;
        try
        {
            var baseUrl = _configuration["App:BaseUrl"] 
                ?? (useLocalEmulators ? "http://localhost:3002" : "https://vendor-portal.company.com");
            
            var emailData = new InvitationEmailData
            {
                InvitationId = invitation.Id.ToString(),
                VendorName = invitation.VendorLegalName,
                Email = invitation.PrimaryContactEmail,
                Token = invitation.InvitationToken,
                ExpiresAt = invitation.ExpiresAt,
                InvitedByName = invitation.InvitedByName,
                CompanyName = _configuration["App:CompanyName"] ?? "Your Company",
                Notes = invitation.Notes,
                BaseUrl = baseUrl
            };

            emailSent = await _emailService.SendInvitationEmailAsync(emailData);
            
            if (emailSent)
            {
                _logger.LogInformation(
                    "✅ Resend invitation email sent successfully to {Email}", 
                    invitation.PrimaryContactEmail);
                Console.WriteLine($"✅ Resend invitation email sent to: {invitation.PrimaryContactEmail}");
            }
            else
            {
                _logger.LogWarning(
                    "⚠️ Resend invitation email sending returned false for {Email}. Email details logged to console.", 
                    invitation.PrimaryContactEmail);
                Console.WriteLine($"⚠️ Resend invitation email logged (not sent) for: {invitation.PrimaryContactEmail}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to send resend invitation email directly for {Email}. Invitation resent but email not sent.", 
                invitation.PrimaryContactEmail);
            // Don't fail the resend if email sending fails
        }

        _logger.LogInformation(
            "Invitation {InvitationId} resent by {RequestedBy}",
            invitationId, requestedBy);

        var link = $"/invitation/register/{invitation.InvitationToken}";
        return (true, link, emailSent);
    }

    public async Task<bool> CancelInvitationAsync(Guid invitationId, Guid requestedBy)
    {
        var invitation = await _context.VendorInvitations
            .FirstOrDefaultAsync(i => i.Id == invitationId);

        if (invitation == null || invitation.Status == InvitationStatus.Completed || invitation.Status == InvitationStatus.Cancelled)
        {
            return false;
        }

        var previousStatus = invitation.Status;
        invitation.Status = InvitationStatus.Cancelled;
        
        // Log who cancelled it in attributes
        var attributes = JsonSerializer.Deserialize<Dictionary<string, object>>(invitation.Attributes) ?? new();
        attributes["cancelledBy"] = requestedBy;
        attributes["cancelledAt"] = DateTime.UtcNow;
        invitation.Attributes = JsonSerializer.Serialize(attributes);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Invitation {InvitationId} status updated from {PreviousStatus} to Cancelled by {RequestedBy}",
            invitation.Id, previousStatus, requestedBy);

        // Emit domain event
        try
        {
            await EmitDomainEventAsync("InvitationCancelled", invitation.Id.ToString(), new
            {
                InvitationId = invitation.Id,
                CancelledBy = requestedBy,
                CancelledAt = DateTime.UtcNow,
                VendorName = invitation.VendorLegalName,
                Email = invitation.PrimaryContactEmail
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to emit InvitationCancelled event to CosmosDB for {InvitationId}", invitation.Id);
            // Continue - event consistency shouldn't block core logic
        }

        return true;
    }

    public async Task ExpireOldInvitationsAsync()
    {
        var expiredInvitations = await _context.VendorInvitations
            .Where(i => i.ExpiresAt < DateTime.UtcNow 
                     && i.Status == InvitationStatus.Pending)
            .ToListAsync();

        foreach (var invitation in expiredInvitations)
        {
            invitation.Status = InvitationStatus.Expired;
        }

        if (expiredInvitations.Any())
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation(
                "Expired {Count} old invitations", 
                expiredInvitations.Count);
        }
    }

    // --- HYBRID ARCHITECTURE PATTERN: Cosmos Helpers ---
    // Following same pattern as ArtifactService for consistency

    /// <summary>
    /// Store invitation artifact in Cosmos DB for complete audit trail
    /// </summary>
    private async Task SaveInvitationArtifactAsync(string invitationId, object payload)
    {
        var artifact = new InvitationArtifact
        {
            Id = invitationId,
            InvitationId = invitationId, // Partition key
            FullPayload = payload,
            CreatedAt = DateTime.UtcNow
        };

        await _cosmosArtifactsContainer.UpsertItemAsync(
            artifact, 
            new PartitionKey(invitationId));
    }

    /// <summary>
    /// Emit domain event to Cosmos DB for event sourcing
    /// </summary>
    private async Task EmitDomainEventAsync(string eventType, string entityId, object data)
    {
        var domainEvent = new CosmosModels.DomainEvent
        {
            Id = Guid.NewGuid().ToString(),
            EventType = eventType, // Partition key
            EntityId = entityId,
            Timestamp = DateTime.UtcNow,
            Data = data,
            SchemaVersion = "v1.0.0"
        };

        await _cosmosEventsContainer.CreateItemAsync(
            domainEvent, 
            new PartitionKey(eventType));
    }

    public async Task<bool> TriggerMfaAsync(string token)
    {
        var invitation = await _context.VendorInvitations.FirstOrDefaultAsync(i => i.InvitationToken == token);
        if (invitation == null || (invitation.Status != InvitationStatus.Pending && invitation.Status != InvitationStatus.Accepted)) return false;

        // Generate 6-digit code
        var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        _logger.LogInformation("DEBUG: Generated MFA Code for {Email}: {Code}", invitation.PrimaryContactEmail, code);
        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        // Update Attributes
        var attributes = JsonSerializer.Deserialize<Dictionary<string, object>>(invitation.Attributes) ?? new();
        attributes["mfaCode"] = code;
        attributes["mfaCodeExpiresAt"] = expiresAt;
        invitation.Attributes = JsonSerializer.Serialize(attributes);

        await _context.SaveChangesAsync();

        // Send Email
        return await _emailService.SendMfaCodeEmailAsync(invitation.PrimaryContactEmail, invitation.VendorLegalName, code);
    }

    public async Task<VerifyMfaResponse> VerifyMfaCodeAsync(string token, string code)
    {
        var invitation = await _context.VendorInvitations.FirstOrDefaultAsync(i => i.InvitationToken == token);
        if (invitation == null) return new VerifyMfaResponse { Success = false, Message = "Invalid token" };

        if (string.IsNullOrEmpty(invitation.Attributes)) 
        {
            _logger.LogWarning("MFA check failed: No attributes found for invitation {InvitationId}", invitation.Id);
            return new VerifyMfaResponse { Success = false, Message = "Server error: No attributes found" };
        }

        try 
        {
            var attributes = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(invitation.Attributes) ?? new();
            
            // Check for bypass (local dev/demo only - remove in prod)
            // SECURITY FIX: Wrapped in IsDevelopment check
            bool isDev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            bool bypass = isDev && code == "000000"; 
            string storedCodeStr = "";
            DateTime expiresAt = DateTime.MinValue;

            bool hasStoredCode = attributes.TryGetValue("mfaCode", out var storedCode);
            bool hasExpiresAt = attributes.TryGetValue("mfaCodeExpiresAt", out var expiresAtElement);

            if (bypass)
            {
                storedCodeStr = "000000";
                expiresAt = DateTime.UtcNow.AddHours(1);
            }
            else if (hasStoredCode && hasExpiresAt)
            {
                storedCodeStr = storedCode.GetString() ?? "";
                expiresAt = expiresAtElement.GetDateTime();
            }

            if ((bypass || (hasStoredCode && hasExpiresAt)) && storedCodeStr == code && expiresAt > DateTime.UtcNow)
            {
                    _logger.LogInformation("MFA Verified for {Id}. Restoring Attributes Length: {Len}", invitation.Id, invitation.Attributes?.Length ?? 0);
                    _logger.LogInformation("MFA Code valid. Transitioning Invitation {Id} from {OldStage} to {NewStage}", 
                        invitation.Id, invitation.CurrentStage, InvitationStage.MfaVerified);

                    // Update stage if not already further along
                    if (invitation.CurrentStage == InvitationStage.InvitationSent)
                    {
                        invitation.CurrentStage = InvitationStage.MfaVerified;
                    }
                    
                    // Clear the code after successful use
                    // We need to re-deserialize as object to modify and save back
                    var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(invitation.Attributes) ?? new();
                    dict.Remove("mfaCode");
                    dict.Remove("mfaCodeExpiresAt");
                    invitation.Attributes = JsonSerializer.Serialize(dict);

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Invitation {Id} saved with new stage: {Stage}", invitation.Id, invitation.CurrentStage);
                    
                    return new VerifyMfaResponse 
                    { 
                        Success = true, 
                        Message = "MFA Verified",
                        CurrentStage = invitation.CurrentStage,
                        Attributes = dict, // Return the full attributes (saved drafts)
                        VendorLegalName = invitation.VendorLegalName,
                        PrimaryContactEmail = invitation.PrimaryContactEmail,
                        VendorType = invitation.VendorType,
                        ExpiresAt = invitation.ExpiresAt
                    };
                }
                else 
                {
                     _logger.LogWarning("MFA Verification Failed for {Id}. Stored: {Stored}, Provided: {Provided}, Expires: {Expires}, Now: {Now}",
                        invitation.Id, storedCodeStr, code, expiresAt, DateTime.UtcNow);
                }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse attributes for invitation {InvitationId}", invitation.Id);
        }

        return new VerifyMfaResponse { Success = false, Message = "Invalid or expired MFA code" };
    }

    public async Task<bool> SubmitInitialInfoAsync(string token, Dictionary<string, object> initialInfo)
    {
        var invitation = await _context.VendorInvitations.FirstOrDefaultAsync(i => i.InvitationToken == token);
        if (invitation == null || invitation.CurrentStage != InvitationStage.MfaVerified) return false;

        // Merge initial info into attributes
        var attributes = JsonSerializer.Deserialize<Dictionary<string, object>>(invitation.Attributes) ?? new();
        foreach (var kvp in initialInfo)
        {
            attributes[kvp.Key] = kvp.Value;
        }
        
        invitation.Attributes = JsonSerializer.Serialize(attributes);
        invitation.CurrentStage = InvitationStage.InitialInfoCompleted;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SubmitEnrichmentAsync(string token, Dictionary<string, object> enrichmentData)
    {
        var invitation = await _context.VendorInvitations.FirstOrDefaultAsync(i => i.InvitationToken == token);
        if (invitation == null) return false;

        // Idempotency: If already completed, return true (succeeded previously)
        if (invitation.Status == InvitationStatus.Completed || invitation.CurrentStage == InvitationStage.Enriched)
        {
             _logger.LogInformation("SubmitEnrichment called for already completed/enriched invitation {Id}. Returning success.", invitation.Id);
             return true;
        }

        if (invitation.CurrentStage != InvitationStage.InitialInfoCompleted && invitation.CurrentStage != InvitationStage.MfaVerified) 
        {
             // Allow MfaVerified to skip InitialInfo if needed, or strict check? 
             // Strict: must be InitialInfoCompleted. But let's be robust.
             if (invitation.CurrentStage != InvitationStage.InitialInfoCompleted)
             {
                 _logger.LogWarning("SubmitEnrichment skipped due to invalid stage {Stage} for {Id}", invitation.CurrentStage, invitation.Id);
                 // return false; // Fail strict? Or proceed? Let's proceed if flow allows jumping.
             }
        }

        // 1. Merge enrichment data into attributes
        var attributes = JsonSerializer.Deserialize<Dictionary<string, object>>(invitation.Attributes) ?? new();
        foreach (var kvp in enrichmentData)
        {
            attributes[kvp.Key] = kvp.Value;
        }

        invitation.Attributes = JsonSerializer.Serialize(attributes);
        invitation.CurrentStage = InvitationStage.Enriched; // Mark enriched
        
        // REMOVED INTERMEDIATE SAVE to ensure atomicity
        // await _context.SaveChangesAsync();

        // 2. ATOMIC HANDOVER: Create Vendor Application Immediately
        try 
        {
            // Pass 'false' to indicate we shouldn't save inside the internal method
            var (appId, appName) = await CreateApplicationFromInvitationInternalAsync(invitation, attributes, saveChanges: false);
            
            // 3. FINAL ATOMIC COMMIT
            // Both the Invitation Update (Enriched -> Completed) and Application Creation happen here
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Atomic Creation Successful. App {AppId} created, Invitation {InvId} completed.", appId, invitation.Id);

            // 4. Artifacts & Events (Post-Commit Side Effects)
            // Ideally these should be reliable, but SQL consistency is the priority.
            await SaveInvitationArtifactAsync(invitation.Id.ToString(), new { 
                Action = "ApplicationCreated", 
                AppId = appId, 
                // Sanctions status is inside the entity now
            });

            await EmitDomainEventAsync("ApplicationCreatedFromInvitation", appId.ToString(), new {
                 ApplicationId = appId,
                 InvitationId = invitation.Id,
                 VendorName = appName
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create application from invitation {Id} during enrichment submission.", invitation.Id);
            return false; // Transaction rolls back automatically (changes not saved)
        }
    }

    /// <summary>
    /// Internal method to create Vendor Application from a fully refined Invitation.
    /// Encapsulates the logic previously in InvitationController.
    /// </summary>
    private async Task<(Guid AppId, string AppName)> CreateApplicationFromInvitationInternalAsync(
        VendorInvitation invitation, 
        Dictionary<string, object> attributes,
        bool saveChanges = true)
    {
        _logger.LogInformation("Starting Application Creation for Invitation {Id} (saveChanges={Save})", invitation.Id, saveChanges);

        // A. Extract Core Fields from Attributes or Invitation
        string companyName = invitation.VendorLegalName;
        if (attributes.TryGetValue("companyName", out var cn) && cn != null) companyName = cn.ToString() ?? companyName;

        string taxId = "";
        if (attributes.TryGetValue("taxId", out var ti) && ti != null) taxId = ti.ToString() ?? "";

        string contactName = invitation.InvitedByName; // Default
        if (attributes.TryGetValue("contactName", out var ctn) && ctn != null) contactName = ctn.ToString() ?? contactName;

        string contactEmail = invitation.PrimaryContactEmail;
        if (attributes.TryGetValue("email", out var em) && em != null) contactEmail = em.ToString() ?? contactEmail;


        // B. Create Application Entity
        var application = new VendorApplication
        {
            Id = Guid.NewGuid(),
            CompanyName = companyName,
            TaxId = taxId,
            ContactName = contactName,
            ContactEmail = contactEmail,
            Status = "Submitted", // Initial status
            RegistrationType = "Invitation",
            CreatedAt = DateTime.UtcNow
        };

        // C. Sanctions Screening (Fail-Closed Logic handled in Service)
        var screeningRequest = new ScreeningRequest
        {
            VendorId = application.Id.ToString(),
            EntityType = (invitation.VendorType == "Physical" || invitation.VendorType == "Participant") ? "Individual" : "Company",
            EntityName = !string.IsNullOrEmpty(companyName) ? companyName : contactName,
            TaxId = taxId,
            Address = new AddressInfo { Country = attributes.TryGetValue("country", out var c) ? c?.ToString() : "US" }
        };

        var screeningResult = await _sanctionsService.ScreenEntityAsync(screeningRequest);

        // Update Status based on Screening
        application.Status = "PendingReview"; // Default to pending review regardless of sanctions? 
                                              // Controller logic set it to PendingReview.
                                              // We store strict status in Attributes.

        // Enrich attributes with Screening & System Data
        attributes["SanctionsScreeningId"] = screeningResult.ScreeningId;
        attributes["SanctionsStatus"] = screeningResult.Status;
        attributes["SanctionsScore"] = screeningResult.OverallRisk;
        attributes["VendorType"] = invitation.VendorType;
        attributes["AccountGroup"] = invitation.AccountGroup;

        // Ensure internal fields are carried over
        if (attributes.TryGetValue("Currency", out var curr)) application.Attributes = JsonSerializer.Serialize(attributes); // Re-serialize full set
        else 
        {
            // If currency was in invitation root attributes but not passed in enrichment, ensure it's merged ?? 
            // We loaded 'attributes' from 'invitation.Attributes' at start of SubmitEnrichment, so it should be there.
        }
        
        application.Attributes = JsonSerializer.Serialize(attributes);

        // D. Add Application (Pending Save)
        _context.VendorApplications.Add(application);
        // if (saveChanges) await _context.SaveChangesAsync(); -- MOVED TO CALLER for Atomicity

        // E. Complete Invitation (Link logic)
        // We call the existing logic but refactored to be internal friendly? 
        // Or just execute it here.
        
        // Update Invitation (Pending Save)
        invitation.Status = InvitationStatus.Completed;
        invitation.CompletedAt = DateTime.UtcNow;
        invitation.VendorApplicationId = application.Id;
        invitation.SanctionsStatus = screeningResult.Status; // Update flat field on invitation too

        if (saveChanges) 
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Legacy non-atomic save executed.");
        }

        return (application.Id, companyName);

        // F. Artifacts & Events -- MOVED TO CALLER for Atomicity
        // (Only emit events if transaction commits)
    }

    private static string GenerateSecureToken()
    {
        // Generate a cryptographically secure random token
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        // Convert to base64url (URL-safe)
        var token = Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        return token;
    }

    private static string MapVendorTypeToAccountGroup(string vendorType)
    {
        return vendorType switch
        {
            "Physical" => "INDV",
            "Company" => "HQSU",
            "Meeting" => "EVNT",
            "Participant" => "PART",
            _ => "INDV" // Default fallback
        };
    }
}
