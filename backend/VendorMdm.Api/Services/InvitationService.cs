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
    Task<(bool Success, string? Link)> ResendInvitationAsync(Guid invitationId, Guid requestedBy);
    Task<bool> TriggerMfaAsync(string token);
    Task<bool> VerifyMfaCodeAsync(string token, string code);
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

            var emailSent = await _emailService.SendInvitationEmailAsync(emailData);
            
            if (emailSent)
            {
                _logger.LogInformation(
                    "Invitation email sent successfully to {Email}", 
                    request.PrimaryContactEmail);
            }
            else
            {
                _logger.LogWarning(
                    "Invitation email sending returned false for {Email}. Email details logged.", 
                    request.PrimaryContactEmail);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to send invitation email directly for {Email}. Invitation created but email not sent.", 
                request.PrimaryContactEmail);
            // Don't fail the invitation creation if email sending fails
        }

        var invitationLink = $"/invitation/register/{token}";

        return new CreateInvitationResponse
        {
            InvitationId = invitation.Id,
            InvitationToken = token,
            InvitationLink = invitationLink,
            ExpiresAt = expiresAt
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

        return new ValidateInvitationResponse
        {
            IsValid = true,
            VendorLegalName = invitation.VendorLegalName,
            PrimaryContactEmail = invitation.PrimaryContactEmail,
            VendorType = invitation.VendorType,
            ExpiresAt = invitation.ExpiresAt,
            CurrentStage = invitation.CurrentStage
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

        var invitations = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new InvitationListItem
            {
                Id = i.Id,
                VendorLegalName = i.VendorLegalName,
                PrimaryContactEmail = i.PrimaryContactEmail,
                Status = i.Status,
                InvitedByName = i.InvitedByName,
                CreatedAt = i.CreatedAt,
                ExpiresAt = i.ExpiresAt,
                VendorApplicationId = i.VendorApplicationId
            })
            .ToListAsync();

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

    public async Task<(bool Success, string? Link)> ResendInvitationAsync(Guid invitationId, Guid requestedBy)
    {
        var invitation = await _context.VendorInvitations
            .FirstOrDefaultAsync(i => i.Id == invitationId);

        if (invitation == null || invitation.Status == InvitationStatus.Completed)
        {
            return (false, null);
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

            var emailSent = await _emailService.SendInvitationEmailAsync(emailData);
            
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
        return (true, link);
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

    public async Task<bool> VerifyMfaCodeAsync(string token, string code)
    {
        var invitation = await _context.VendorInvitations.FirstOrDefaultAsync(i => i.InvitationToken == token);
        if (invitation == null) return false;

        if (string.IsNullOrEmpty(invitation.Attributes)) 
        {
            _logger.LogWarning("MFA check failed: No attributes found for invitation {InvitationId}", invitation.Id);
            return false;
        }

        try 
        {
            var attributes = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(invitation.Attributes) ?? new();
            
            if (attributes.TryGetValue("mfaCode", out var storedCode) && 
                attributes.TryGetValue("mfaCodeExpiresAt", out var expiresAtElement))
            {
                var storedCodeStr = storedCode.GetString();
                var expiresAt = expiresAtElement.GetDateTime();

                if (storedCodeStr == code && expiresAt > DateTime.UtcNow)
                {
                    invitation.CurrentStage = InvitationStage.MfaVerified;
                    
                    // Clear the code after successful use
                    var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(invitation.Attributes) ?? new();
                    dict.Remove("mfaCode");
                    dict.Remove("mfaCodeExpiresAt");
                    invitation.Attributes = JsonSerializer.Serialize(dict);

                    await _context.SaveChangesAsync();
                    return true;
                }
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse attributes for invitation {InvitationId}", invitation.Id);
        }

        return false;
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
        if (invitation == null || invitation.CurrentStage != InvitationStage.InitialInfoCompleted) return false;

        // Merge enrichment data into attributes
        var attributes = JsonSerializer.Deserialize<Dictionary<string, object>>(invitation.Attributes) ?? new();
        foreach (var kvp in enrichmentData)
        {
            attributes[kvp.Key] = kvp.Value;
        }

        invitation.Attributes = JsonSerializer.Serialize(attributes);
        invitation.CurrentStage = InvitationStage.Enriched;
        
        // DO NOT mark as Completed yet. The Controller's CompleteInvitation endpoint must be called to create Application.
        // invitation.Status = InvitationStatus.Completed;
        // invitation.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
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
