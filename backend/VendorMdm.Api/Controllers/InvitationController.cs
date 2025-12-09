using Microsoft.AspNetCore.Mvc;
using VendorMdm.Api.Models; // DTOs
using VendorMdm.Api.Services;
using VendorMdm.Api.Data;
using VendorMdm.Shared.Models; // SQL entities
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace VendorMdm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvitationController : ControllerBase
{
    private readonly IInvitationService _invitationService;
    private readonly SqlDbContext _context;
    private readonly ILogger<InvitationController> _logger;

    public InvitationController(
        IInvitationService invitationService,
        SqlDbContext context,
        ILogger<InvitationController> logger)
    {
        _invitationService = invitationService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a new vendor invitation (Approver/Admin only)
    /// </summary>
    [Authorize(Policy = "AdminOrApprover")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateInvitation([FromBody] CreateInvitationRequest request)
    {
        try
        {
            // For now, using a dummy user until we parse the token properly
            // In real impl: var userId = Guid.Parse(User.FindFirst("sub").Value);
            var invitedBy = Guid.NewGuid();
            var invitedByName = "System Admin"; // In production: User.Identity.Name

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
            return StatusCode(500, new { error = "Failed to create invitation" });
        }
    }

    /// <summary>
    /// Validate an invitation token
    /// </summary>
    [HttpGet("validate/{token}")]
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
                expiresAt = invitation.ExpiresAt,
                status = invitation.Status
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

            // Create vendor application
            var application = new VendorApplication
            {
                Id = Guid.NewGuid(),
                CompanyName = request.CompanyName,
                TaxId = request.TaxId,
                ContactName = request.ContactName,
                ContactEmail = request.Email,
                Status = "Submitted",
                RegistrationType = "Invitation",
                CreatedAt = DateTime.UtcNow
            };

            _context.VendorApplications.Add(application);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Vendor application {ApplicationId} created for invitation token {Token}",
                application.Id, token);

            // Link invitation to application and update status
            var completed = await _invitationService.CompleteInvitationAsync(token, application.Id);
            
            if (!completed)
            {
                _logger.LogWarning(
                    "Failed to complete invitation for token {Token}. Application {ApplicationId} was created but invitation status not updated.",
                    token, application.Id);
                // Don't fail the request - application was created successfully
                // The invitation might already be completed or in an invalid state
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
                status = "Submitted",
                message = "Your application has been submitted successfully!"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing invitation for token {Token}", token);
            return StatusCode(500, new { error = "Failed to complete registration" });
        }
    }

}
