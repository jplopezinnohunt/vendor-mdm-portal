using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using VendorMdm.Api.Data;
using VendorMdm.Shared.Constants;
using VendorMdm.Shared.Models;
using VendorMdm.Api.Services;
using System.Text.Json;
using System.Security.Claims;

namespace VendorMdm.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
[Authorize] // Allow all authenticated users
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
    /// Approvers see all pending reviews
    /// Requestors see only their own submissions
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingReviews()
    {
        try
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var isApprover = userRoles.Contains("Approver");

            _logger.LogInformation("GetPendingReviews called by {Email} with roles: {Roles}", userEmail, string.Join(", ", userRoles));

            // Fetch applications with Submitted or PendingReview status (both are reviewable)
            var reviewableStatuses = new[] { ApplicationStatus.Submitted, ApplicationStatus.PendingReview };
            var query = _context.VendorApplications
                .Where(a => reviewableStatuses.Contains(a.Status));

            // If user is NOT an Approver, filter to show only their own submissions
            if (!isApprover && !string.IsNullOrEmpty(userEmail))
            {
                query = query.Where(a => a.ContactEmail == userEmail);
                _logger.LogInformation("Filtering reviews for Requestor: {Email}", userEmail);
            }

            var pendingAppsRaw = await query
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
            var pendingAppsList = pendingAppsRaw.Select(a => 
            {
                Dictionary<string, object> attributes = new();
                try
                {
                    if (!string.IsNullOrEmpty(a.Attributes))
                    {
                        attributes = JsonSerializer.Deserialize<Dictionary<string, object>>(a.Attributes) ?? new();
                    }
                }
                catch
                {
                    // Ignore invalid JSON to prevent crash
                }

                return new
                {
                    a.Id,
                    a.CompanyName,
                    a.ContactName,
                    a.ContactEmail,
                    a.RegistrationType,
                    a.CreatedAt,
                    Attributes = attributes
                };
            }).ToList();

            _logger.LogInformation("Returning {Count} pending reviews", pendingAppsList.Count);
            return Ok(pendingAppsList);
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
    /// Approve a vendor application (Approvers only)
    /// Pattern 4: Result Pattern - Uses Result<T> for clean error handling.
    /// </summary>
    [HttpPost("{id}/approve")]
    [Authorize(Policy = "ApproverOnly")]
    public async Task<IActionResult> ApproveApplication(Guid id, [FromBody] ApprovalRequest request)
    {
        var approverId = User.Identity?.Name ?? "System";
        var result = await _applicationService.ApproveApplicationAsync(id, request.EnrichedAttributes, request.ForceSanctionsOverride, approverId);

        if (result.IsFailure)
        {
            _logger.LogWarning("Approval failed for {Id}: {Error}", id, result.Error);
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Application approved", status = "Approved", vendorId = result.Value.Id });
    }

    /// <summary>
    /// Reject a vendor application (Approvers only)
    /// Pattern 4: Result Pattern - Uses Result for clean error handling.
    /// </summary>
    [HttpPost("{id}/reject")]
    [Authorize(Policy = "ApproverOnly")]
    public async Task<IActionResult> RejectApplication(Guid id, [FromBody] RejectionRequest request)
    {
        var approverId = User.Identity?.Name ?? "System";
        var result = await _applicationService.RejectApplicationAsync(id, request.Reason, approverId);

        if (result.IsFailure)
        {
            _logger.LogWarning("Rejection failed for {Id}: {Error}", id, result.Error);
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Application rejected", status = "Rejected" });
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
