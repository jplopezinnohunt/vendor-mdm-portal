using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using VendorMdm.Api.Models; // DTOs
using VendorMdm.Api.Services;
using VendorMdm.Api.Data;
using VendorMdm.Shared.Mapping;
using Microsoft.AspNetCore.RateLimiting;
using VendorMdm.Shared.Models; // SQL entities
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using VendorMdm.Shared.Models.Sanctions;

namespace VendorMdm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvitationController : ControllerBase
{
    private readonly IInvitationService _invitationService;
    private readonly SqlDbContext _context;
    private readonly ILogger<InvitationController> _logger;
    private readonly ISanctionsScreeningService _sanctionsScreeningService;

    [ActivatorUtilitiesConstructor]
    public InvitationController(
        IInvitationService invitationService,
        SqlDbContext context,
        ILogger<InvitationController> logger,
        ISanctionsScreeningService sanctionsScreeningService)
    {
        _invitationService = invitationService;
        _context = context;
        _logger = logger;
        _sanctionsScreeningService = sanctionsScreeningService;
    }

    /// <summary>
    /// Create a new vendor invitation (Approver only)
    /// </summary>
    [Authorize(Policy = "ApproverOnly")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateInvitation([FromBody] CreateInvitationRequest request)
    {
        try
        {
            // For now, using a dummy user until we parse the token properly
            // In real impl: var userId = Guid.Parse(User.FindFirst("sub").Value);
            var invitedBy = Guid.NewGuid();
            var invitedByName = "Internal Approver"; // In production: User.Identity.Name

            var response = await _invitationService.CreateInvitationAsync(
                request,
                invitedBy,
                invitedByName);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invitation");
            return StatusCode(500, new { error = "Failed to create invitation", detail = ex.Message, stack = ex.StackTrace });
        }
    }

    /// <summary>
    /// Get all invitations with optional status filter (Approver only)
    /// </summary>
    [Authorize(Policy = "ApproverOnly")]
    [HttpGet("list")]
    public async Task<IActionResult> ListInvitations([FromQuery] string? status = null)
    {
        try
        {
            var query = _context.VendorInvitations.AsQueryable();
            var totalCount = await _context.VendorInvitations.CountAsync();
            _logger.LogInformation("ListInvitations called. Total invitations in DB: {Count}. Status filter: {Status}", totalCount, status);

            // Filter by status if provided
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(i => i.Status == status);
            }

            var invitations = await query
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new
                {
                    id = i.Id,
                    vendorLegalName = i.VendorLegalName,
                    primaryContactEmail = i.PrimaryContactEmail,
                    status = i.Status,
                    invitedByName = i.InvitedByName,
                    createdAt = i.CreatedAt,
                    expiresAt = i.ExpiresAt,
                    vendorType = i.VendorType,
                    accountGroup = i.AccountGroup,
                    vendorApplicationId = i.VendorApplicationId,
                    currentStage = i.CurrentStage,
                    // Check attributes for email failure flag
                    emailSent = i.Attributes != null && !i.Attributes.Contains("\"emailSent\":false") // Simple check avoids full parsing for list
                })
                .ToListAsync();

            return Ok(new { invitations });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing invitations");
            return StatusCode(500, new { error = "Failed to list invitations" });
        }
    }

    /// <summary>
    /// Resend an invitation email (Approver only)
    /// </summary>
    [Authorize(Policy = "ApproverOnly")]
    [HttpPost("resend/{id}")]
    public async Task<IActionResult> ResendInvitation(Guid id)
    {
        try
        {
            var requestedBy = Guid.NewGuid(); // In production: parse from token
            var (success, invitationLink, emailSent, emailError) = await _invitationService.ResendInvitationAsync(id, requestedBy);

            if (!success)
            {
                return NotFound(new { error = "Invitation not found or already completed" });
            }

            if (!emailSent && string.IsNullOrEmpty(emailError))
            {
                emailError = "Unknown error: Backend returned no specific failure message.";
            }

            return Ok(new { message = "Invitation resent successfully", invitationLink, emailSent, emailError });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending invitation {Id}", id);
            return StatusCode(500, new { error = "Failed to resend invitation" });
        }
    }

    /// <summary>
    /// Cancel an invitation (Approver only)
    /// </summary>
    [Authorize(Policy = "ApproverOnly")]
    [HttpPost("cancel/{id}")]
    public async Task<IActionResult> CancelInvitation(Guid id)
    {
        try
        {
            var requestedBy = Guid.NewGuid(); // In production: parse from token
            var success = await _invitationService.CancelInvitationAsync(id, requestedBy);

            if (!success)
            {
                return NotFound(new { error = "Invitation not found, or it is already completed/cancelled" });
            }

            return Ok(new { message = "Invitation cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling invitation {Id}", id);
            return StatusCode(500, new { error = "Failed to cancel invitation" });
        }
    }

    /// <summary>
    /// Validate an invitation token
    /// </summary>
    [HttpGet("validate/{token}")]
    [AllowAnonymous]
    [EnableRateLimiting("anonymous")]
    public async Task<IActionResult> ValidateInvitation(string token)
    {
        try
        {
            var response = await _invitationService.ValidateInvitationAsync(token);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating invitation");
            return StatusCode(500, new { error = "Failed to validate invitation" });
        }
    }

    [HttpPost("trigger-mfa/{token}")]
    public async Task<IActionResult> TriggerMfa(string token)
    {
        try
        {
            var result = await _invitationService.TriggerMfaAsync(token);
            if (!result) return BadRequest(new { error = "Failed to trigger MFA" });
            return Ok(new { message = "MFA code sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering MFA");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("verify-mfa/{token}")]
    public async Task<IActionResult> VerifyMfa(string token, [FromBody] MfaVerifyRequest request)
    {
        try
        {
            var result = await _invitationService.VerifyMfaCodeAsync(token, request.Code);
            if (!result.Success) return BadRequest(new { error = result.Message });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying MFA");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("submit-initial/{token}")]
    public async Task<IActionResult> SubmitInitial(string token, [FromBody] Dictionary<string, object> initialInfo)
    {
        try
        {
            var result = await _invitationService.SubmitInitialInfoAsync(token, initialInfo);
            if (!result) return BadRequest(new { error = "Failed to submit initial information" });
            return Ok(new { message = "Initial information saved" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting initial info");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("submit-enrichment/{token}")]
    public async Task<IActionResult> SubmitEnrichment(string token, [FromBody] Dictionary<string, object> enrichmentData)
    {
        try
        {
            var result = await _invitationService.SubmitEnrichmentAsync(token, enrichmentData);
            if (!result) return BadRequest(new { error = "Failed to submit enrichment data" });
            return Ok(new { message = "Enrichment data saved" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting enrichment data");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get invitation details by token (for pre-filling form)
    /// </summary>
    [HttpGet("details/{token}")]
    public async Task<IActionResult> GetInvitationDetails(string token)
    {
        try
        {
            var invitation = await _invitationService.GetInvitationByTokenAsync(token);

            if (invitation == null)
            {
                return NotFound(new { error = "Invitation not found" });
            }

            // Only return non-sensitive data
            return Ok(new
            {
                vendorLegalName = invitation.VendorLegalName,
                primaryContactEmail = invitation.PrimaryContactEmail,
                vendorType = invitation.VendorType,
                expiresAt = invitation.ExpiresAt,
                status = invitation.Status,
                accountGroup = invitation.AccountGroup,
                currentStage = invitation.CurrentStage
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invitation details");
            return StatusCode(500, new { error = "Failed to get invitation details" });
        }
    }

    /// <summary>
    /// Complete vendor registration via invitation
    /// </summary>
    [AllowAnonymous]
    [HttpPost("complete/{token}")]
    public async Task<IActionResult> CompleteInvitation(
        string token,
        [FromBody] CompleteInvitationRequest request)
    {
        try
        {
            // Validate invitation
            var validation = await _invitationService.ValidateInvitationAsync(token);
            
            if (!validation.IsValid)
            {
                _logger.LogWarning("Invitation validation failed for token {Token}: {Error}", token, validation.ErrorMessage);
                return BadRequest(new { error = validation.ErrorMessage });
            }

            // Retrieve the invitation record to get details like VendorType
            var invitation = await _context.VendorInvitations.FirstOrDefaultAsync(i => i.InvitationToken == token);
            if (invitation == null)
            {
                return BadRequest(new { error = "Invitation record not found" });
            }

            // Create vendor application
            var application = new VendorApplication
            {
                Id = Guid.NewGuid(),
                CompanyName = request.CompanyName,
                TaxId = string.IsNullOrWhiteSpace(request.TaxId) ? null : request.TaxId,
                ContactName = request.ContactName,
                ContactEmail = request.Email,
                Status = "Submitted",
                RegistrationType = "Invitation",
                CreatedAt = DateTime.UtcNow
            };

            // Perform Sanctions Screening
            // Map request to ScreeningRequest using explicit VendorType
            var screeningRequest = new ScreeningRequest
            {
                VendorId = application.Id.ToString(),
                EntityType = (invitation.VendorType == "Physical" || invitation.VendorType == "Participant") ? "Individual" : "Company",
                EntityName = !string.IsNullOrEmpty(request.CompanyName) ? request.CompanyName : request.ContactName,
                TaxId = request.TaxId,
            };

            // Basic parsing of Attributes for specific screening fields
            if (request.Attributes.TryGetValue("PersonalDetails", out var pdObj) && pdObj is JsonElement pd)
            {
                 // parsing logic if needed, e.g. DOB
            }

            var screeningResult = await _sanctionsScreeningService.ScreenEntityAsync(screeningRequest);

            // Update Application Status based on screening
            application.Status = "PendingReview";
            
            // Enrich Attributes with Screening Info, Vendor Type and Account Group
            var attributesDict = new Dictionary<string, object>(request.Attributes);
            attributesDict["SanctionsScreeningId"] = screeningResult.ScreeningId;
            attributesDict["SanctionsStatus"] = screeningResult.Status;
            attributesDict["SanctionsScore"] = screeningResult.OverallRisk;
            attributesDict["VendorType"] = invitation.VendorType;
            attributesDict["AccountGroup"] = invitation.AccountGroup;
            
            attributesDict["SanctionsScore"] = screeningResult.OverallRisk;
            attributesDict["VendorType"] = invitation.VendorType;
            attributesDict["AccountGroup"] = invitation.AccountGroup;
            
            // Merge internal attributes from Invitation (Currency, SapLanguage) if not already present
            if (!string.IsNullOrEmpty(invitation.Attributes))
            {
                var invAttributes = JsonSerializer.Deserialize<Dictionary<string, object>>(invitation.Attributes);
                if (invAttributes != null)
                {
                    if (invAttributes.ContainsKey("Currency") && !attributesDict.ContainsKey("Currency"))
                        attributesDict["Currency"] = invAttributes["Currency"];
                    
                    if (invAttributes.ContainsKey("SapLanguage") && !attributesDict.ContainsKey("SapLanguage"))
                        attributesDict["SapLanguage"] = invAttributes["SapLanguage"];

                    // Map additional internal fields
                    if (invAttributes.ContainsKey("TaxCode1") && !attributesDict.ContainsKey("TaxCode1"))
                        attributesDict["TaxCode1"] = invAttributes["TaxCode1"];

                    if (invAttributes.ContainsKey("TaxCode2") && !attributesDict.ContainsKey("TaxCode2"))
                        attributesDict["TaxCode2"] = invAttributes["TaxCode2"];

                    if (invAttributes.ContainsKey("PermittedPayee") && !attributesDict.ContainsKey("PermittedPayee"))
                        attributesDict["PermittedPayee"] = invAttributes["PermittedPayee"];
                }
            }

            application.Attributes = JsonSerializer.Serialize(attributesDict);

            _context.VendorApplications.Add(application);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Vendor application {ApplicationId} created for invitation token {Token}. Screening Status: {ScreeningStatus}",
                application.Id, token, screeningResult.Status);

            // Link invitation to application and update status
            var completed = await _invitationService.CompleteInvitationAsync(token, application.Id);
            
            // Update Invitation with Sanctions Info (Direct SQL update as CompleteUtils might not allow partial update)
            var inv = await _context.VendorInvitations.FirstOrDefaultAsync(i => i.InvitationToken == token);
            if (inv != null)
            {
                inv.SanctionsStatus = screeningResult.Status;
                // inv.SanctionsScore = (decimal)screeningResult.Matches.Max(m => m.MatchScore); // Simplified
                inv.ReviewStatus = "Pending";
                await _context.SaveChangesAsync();
            }

            if (!completed)
            {
                _logger.LogWarning(
                    "Failed to complete invitation for token {Token}. Application {ApplicationId} was created but invitation status not updated.",
                    token, application.Id);
            }
            else
            {
                _logger.LogInformation(
                    "Invitation for token {Token} completed successfully with application {ApplicationId}",
                    token, application.Id);
            }

            return Ok(new
            {
                applicationId = application.Id,
                status = "PendingReview",
                message = "Your application has been submitted and is under review."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing invitation for token {Token}", token);
            return StatusCode(500, new { error = "Failed to complete registration" });
        }
    }

    /// <summary>
    /// Save vendor registration as Draft
    /// </summary>
    [AllowAnonymous]
    [HttpPost("save-draft/{token}")]
    public async Task<IActionResult> SaveDraft(
        string token,
        [FromBody] CompleteInvitationRequest request)
    {
        try
        {
            // Validate invitation exists and is valid (or pending draft)
            var validation = await _invitationService.ValidateInvitationAsync(token);
            if (!validation.IsValid)
            {
                return BadRequest(new { error = validation.ErrorMessage });
            }

            // Create/Update vendor application in Draft mode
            // Note: In a real scenario we might check if app already exists for this token and update it
            var application = new VendorApplication
            {
                Id = Guid.NewGuid(),
                CompanyName = request.CompanyName,
                TaxId = request.TaxId, // Nullable in draft? Schema check needed.
                ContactName = request.ContactName,
                ContactEmail = request.Email,
                Status = "Draft",
                RegistrationType = "Invitation",
                CreatedAt = DateTime.UtcNow,
                Attributes = JsonSerializer.Serialize(request.Attributes)
            };

            _context.VendorApplications.Add(application);
            
            // Link invitation to this draft application
            var invitation = await _context.VendorInvitations.FirstOrDefaultAsync(i => i.InvitationToken == token);
            if (invitation != null)
            {
                _logger.LogInformation("SaveDraft: Linking Invitation {InvId} to App {AppId}. Attributes Count: {Count}", 
                    invitation.Id, application.Id, request.Attributes?.Count ?? 0);

                if (request.Attributes != null && request.Attributes.Count > 0)
                {
                    var debugJson = JsonSerializer.Serialize(request.Attributes);
                    _logger.LogInformation("SaveDraft: Saving attributes: {Json}", debugJson);
                }

                invitation.VendorApplicationId = application.Id;
                
                // CRITICAL FIX: Save attributes to invitation for restoration via VerifyMfa
                invitation.Attributes = JsonSerializer.Serialize(request.Attributes); 

                // Update status to Accepted to indicate work has started, but not Completed
                if (invitation.Status == InvitationStatus.Pending)
                {
                    invitation.Status = InvitationStatus.Accepted;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                applicationId = application.Id,
                status = "Draft",
                message = "Application saved as draft."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving draft for token {Token}", token);
            return StatusCode(500, new { error = "Failed to save draft" });
        }
    }

}
