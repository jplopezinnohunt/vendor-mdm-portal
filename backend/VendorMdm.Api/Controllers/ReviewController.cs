using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using VendorMdm.Api.Data;
using VendorMdm.Shared.Models;
using VendorMdm.Api.Services;
using System.Text.Json;

namespace VendorMdm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ApproverOnly")]
public class ReviewController : ControllerBase
{
    private readonly SqlDbContext _context;
    private readonly IVendorApplicationService _applicationService;
    private readonly ILogger<ReviewController> _logger;

    public ReviewController(SqlDbContext context, IVendorApplicationService applicationService, ILogger<ReviewController> logger)
    {
        _context = context;
        _applicationService = applicationService;
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
            // Fetch raw applications first
            var pendingAppsRaw = await _context.VendorApplications
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
                    a.Attributes 
                })
                .ToListAsync();

            // Parse Attributes in memory
            var pendingApps = pendingAppsRaw.Select(a => new
            {
                a.Id,
                a.CompanyName,
                a.ContactName,
                a.ContactEmail,
                a.RegistrationType,
                a.CreatedAt,
                Attributes = string.IsNullOrEmpty(a.Attributes) 
                    ? new Dictionary<string, object>() 
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(a.Attributes)
            });

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
        try 
        {
            var approverId = User.Identity?.Name ?? "System";
            await _applicationService.ApproveApplicationAsync(id, request.EnrichedAttributes, request.ForceSanctionsOverride, approverId);
            return Ok(new { message = "Application approved", status = "Approved" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Approval failed for {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Approval failed unexpectedly for {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Reject a vendor application
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectApplication(Guid id, [FromBody] RejectionRequest request)
    {
        try
        {
            var approverId = User.Identity?.Name ?? "System";
            await _applicationService.RejectApplicationAsync(id, request.Reason, approverId);
            return Ok(new { message = "Application rejected", status = "Rejected" });
        }
        catch (KeyNotFoundException)
        {
             return NotFound();
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Rejection failed for {Id}", id);
             return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

public class ApprovalRequest
{
    public string? Comments { get; set; }
    public Dictionary<string, object>? EnrichedAttributes { get; set; }
    public bool ForceSanctionsOverride { get; set; } = false;
}

public class RejectionRequest
{
    public string Reason { get; set; } = string.Empty;
}
