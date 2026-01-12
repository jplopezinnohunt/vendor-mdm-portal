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
        _logger.LogInformation("GetReviewDetails called for {Id}", id);
        var app = await _context.VendorApplications.FindAsync(id);
        
        if (app == null) 
        {
            _logger.LogWarning("GetReviewDetails: Application {Id} not found", id);
            return NotFound();
        }

        _logger.LogInformation("GetReviewDetails: Application found. Attributes length: {Len}", app.Attributes?.Length ?? 0);
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

        // 1. Apply Enrichment (Updates)
        if (request.EnrichedAttributes != null && request.EnrichedAttributes.Any())
        {
            // Parse existing attributes
            Dictionary<string, object> existingAttributes = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(app.Attributes))
            {
                try
                {
                    existingAttributes = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(app.Attributes) 
                                         ?? new Dictionary<string, object>();
                }
                catch
                {
                    // If parsing fails, start fresh or log warning
                    _logger.LogWarning("Failed to parse existing attributes for App {Id}, overwriting.", id);
                }
            }

            // Merge new attributes
            foreach (var kvp in request.EnrichedAttributes)
            {
                // If the key maps to a core column, update the column directly
                /* 
                   Note: For now, we update attributes. 
                   If specific core columns need update (like CompanyName), we can do it here. 
                   e.g. if (kvp.Key == "companyName") app.CompanyName = kvp.Value.ToString();
                */
                if (kvp.Key.Equals("companyName", StringComparison.OrdinalIgnoreCase)) 
                    app.CompanyName = kvp.Value.ToString() ?? app.CompanyName;
                else if (kvp.Key.Equals("taxId", StringComparison.OrdinalIgnoreCase)) 
                    app.TaxId = kvp.Value.ToString() ?? app.TaxId;
                else if (kvp.Key.Equals("contactName", StringComparison.OrdinalIgnoreCase))
                    app.ContactName = kvp.Value.ToString() ?? app.ContactName;
                else if (kvp.Key.Equals("email", StringComparison.OrdinalIgnoreCase))
                    app.ContactEmail = kvp.Value.ToString() ?? app.ContactEmail;
                else
                {
                    // It's an extended attribute
                    existingAttributes[kvp.Key] = kvp.Value;
                }
            }

            // Serialize back
            app.Attributes = System.Text.Json.JsonSerializer.Serialize(existingAttributes);
        }

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

        _logger.LogInformation("Application {Id} approved by {Approver}. enriched fields: {Count}", id, "CurrentUser", request.EnrichedAttributes?.Count ?? 0); 

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
    public Dictionary<string, object>? EnrichedAttributes { get; set; }
}

public class RejectionRequest
{
    public string Reason { get; set; } = string.Empty;
}
