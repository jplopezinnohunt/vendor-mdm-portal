using Microsoft.AspNetCore.Mvc;
using VendorMdm.Api.Models;
using VendorMdm.Api.Services;
using VendorMdm.Api.Data;
using Microsoft.EntityFrameworkCore;

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
    [HttpPost("create")]
    public async Task<IActionResult> CreateInvitation([FromBody] CreateInvitationRequest request)
    {
        try
        {
            // TODO: Get authenticated user from claims
            // For now, using mock data
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
            return BadRequest(new { error = ex.Message });
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

            // Link invitation to application
            await _invitationService.CompleteInvitationAsync(token, application.Id);

            _logger.LogInformation(
                "Vendor application {ApplicationId} created from invitation {Token}",
                application.Id, token);

            return Ok(new
            {
                applicationId = application.Id,
                status = "Submitted",
                message = "Your application has been submitted successfully!"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing invitation");
            return StatusCode(500, new { error = "Failed to complete registration" });
        }
    }

    /// <summary>
    /// Get list of all invitations (Approver/Admin only)
    /// </summary>
    [HttpGet("list")]
    public async Task<IActionResult> GetInvitations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null)
    {
        try
        {
            var response = await _invitationService.GetInvitationsAsync(page, pageSize, status);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invitations");
            return StatusCode(500, new { error = "Failed to get invitations" });
        }
    }

    /// <summary>
    /// Resend an invitation (Approver/Admin only)
    /// </summary>
    [HttpPost("resend/{id}")]
    public async Task<IActionResult> ResendInvitation(Guid id)
    {
        try
        {
            // TODO: Get authenticated user
            var requestedBy = Guid.NewGuid();

            var success = await _invitationService.ResendInvitationAsync(id, requestedBy);

            if (!success)
            {
                return NotFound(new { error = "Invitation not found or already completed" });
            }

            return Ok(new { message = "Invitation has been resent successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending invitation");
            return StatusCode(500, new { error = "Failed to resend invitation" });
        }
    }
}
