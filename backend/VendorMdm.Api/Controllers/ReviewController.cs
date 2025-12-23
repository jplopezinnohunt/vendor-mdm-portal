using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using VendorMdm.Api.Data;
using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ApproverOnly")]
public class ReviewController : ControllerBase
{
    private readonly SqlDbContext _context;
    private readonly ILogger<ReviewController> _logger;

    public ReviewController(SqlDbContext context, ILogger<ReviewController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all applications pending review
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingReviews()
    {
        try
        {
            // Fetch applications with "PendingReview" status
            var pendingApps = await _context.VendorApplications
                .Where(a => a.Status == "PendingReview")
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new
                {
                    a.Id,
                    a.CompanyName,
                    a.ContactName,
                    a.ContactEmail,
                    a.RegistrationType,
                    a.CreatedAt,
                    // Parse Attributes JSON if needed, or send raw
                    Attributes = a.Attributes 
                })
                .ToListAsync();

            // Also fetch linked invitations to get Sanctions Info if not fully synced
            // (Optional optimization: Join query)
            
            return Ok(pendingApps);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pending reviews");
            return StatusCode(500, new { error = "Failed to fetch pending reviews" });
        }
    }

    /// <summary>
    /// Get details of a specific application for review
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetReviewDetails(Guid id)
    {
        var app = await _context.VendorApplications.FindAsync(id);
        if (app == null) return NotFound();

        return Ok(app);
    }

    /// <summary>
    /// Approve a vendor application
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveApplication(Guid id, [FromBody] ApprovalRequest request)
    {
        var app = await _context.VendorApplications.FindAsync(id);
        if (app == null) return NotFound();

        if (app.Status != "PendingReview")
            return BadRequest("Application is not pending review");

        app.Status = "Approved";
        app.UpdatedAt = DateTime.UtcNow;
        
        // Update Invitation Status if linked
        if (app.InvitationId.HasValue || app.RegistrationType == "Invitation")
        {
            // Find invitation by app ID if direct link not stored on app (Schema check needed)
             var inv = await _context.VendorInvitations.FirstOrDefaultAsync(i => i.VendorApplicationId == id);
             if (inv != null) 
             {
                 inv.ReviewStatus = "Approved";
                 inv.Status = InvitationStatus.Approved; 
             }
        }

        // TODO: Trigger SAP Integration Event here

        await _context.SaveChangesAsync();

        _logger.LogInformation("Application {Id} approved by {Approver}", id, "CurrentUser"); 

        return Ok(new { message = "Application approved", status = "Approved" });
    }

    /// <summary>
    /// Reject a vendor application
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectApplication(Guid id, [FromBody] RejectionRequest request)
    {
        var app = await _context.VendorApplications.FindAsync(id);
        if (app == null) return NotFound();

        app.Status = "Rejected";
        app.UpdatedAt = DateTime.UtcNow;
        
        // Update Invitation
        var inv = await _context.VendorInvitations.FirstOrDefaultAsync(i => i.VendorApplicationId == id);
        if (inv != null) 
        {
            inv.ReviewStatus = "Rejected";
            inv.Status = InvitationStatus.Rejected;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Application {Id} rejected by {Approver}. Reason: {Reason}", id, "CurrentUser", request.Reason);

        return Ok(new { message = "Application rejected", status = "Rejected" });
    }
}

public class ApprovalRequest
{
    public string? Comments { get; set; }
}

public class RejectionRequest
{
    public string Reason { get; set; } = string.Empty;
}
