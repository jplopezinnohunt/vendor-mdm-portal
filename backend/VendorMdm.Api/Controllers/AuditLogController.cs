using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorMdm.Api.Services;
using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Controllers;

/// <summary>
/// API Controller for Audit Log operations (Pattern 16: Audit Trail & Temporal).
/// Provides endpoints for querying audit history with proper error handling.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
[Authorize]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuditLogController> _logger;

    public AuditLogController(
        IAuditLogService auditLogService,
        ILogger<AuditLogController> logger)
    {
        _auditLogService = auditLogService;
        _logger = logger;
    }

    /// <summary>
    /// Get audit logs for a specific entity.
    /// </summary>
    /// <param name="entityType">Type of entity (e.g., "Vendor", "VendorInvitation")</param>
    /// <param name="entityId">ID of the entity</param>
    /// <returns>List of audit logs ordered by most recent first</returns>
    /// <response code="200">Returns the list of audit logs</response>
    /// <response code="400">If entity type or ID is invalid</response>
    /// <response code="401">If user is not authenticated</response>
    /// <response code="500">If an error occurs while retrieving logs</response>
    [HttpGet("{entityType}/{entityId}")]
    [ProducesResponseType(typeof(List<AuditLog>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<ActionResult<List<AuditLog>>> GetEntityLogs(
        string entityType,
        Guid entityId)
    {
        try
        {
            // ✅ NEW RULE: Input validation
            if (string.IsNullOrWhiteSpace(entityType))
            {
                return BadRequest(new ErrorResponse 
                { 
                    Message = "Entity type is required",
                    Code = "INVALID_ENTITY_TYPE"
                });
            }

            if (entityId == Guid.Empty)
            {
                return BadRequest(new ErrorResponse 
                { 
                    Message = "Entity ID is required",
                    Code = "INVALID_ENTITY_ID"
                });
            }

            var logs = await _auditLogService.GetEntityLogsAsync(entityType, entityId);
            
            _logger.LogInformation(
                "✅ Retrieved {Count} audit logs for {EntityType} {EntityId}",
                logs.Count, entityType, entityId);
            
            return Ok(logs);
        }
        catch (ArgumentException ex)
        {
            // ✅ NEW RULE: Proper error handling
            _logger.LogWarning(ex,
                "⚠️ Invalid argument for audit log query: {EntityType} {EntityId}",
                entityType, entityId);
            
            return BadRequest(new ErrorResponse 
            { 
                Message = ex.Message,
                Code = "VALIDATION_ERROR"
            });
        }
        catch (Exception ex)
        {
            // ✅ NEW RULE: Proper error handling
            _logger.LogError(ex,
                "❌ Error getting audit logs for {EntityType} {EntityId}",
                entityType, entityId);
            
            return StatusCode(500, new ErrorResponse 
            { 
                Message = "Failed to retrieve audit logs. Please try again later.",
                Code = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Get audit logs for a specific user (Admin only).
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="from">Start date (optional)</param>
    /// <param name="to">End date (optional)</param>
    /// <returns>List of audit logs ordered by most recent first (max 1000)</returns>
    /// <response code="200">Returns the list of audit logs</response>
    /// <response code="400">If user ID is invalid</response>
    /// <response code="401">If user is not authenticated</response>
    /// <response code="403">If user is not an Admin</response>
    /// <response code="500">If an error occurs while retrieving logs</response>
    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<AuditLog>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<ActionResult<List<AuditLog>>> GetUserLogs(
        Guid userId,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        try
        {
            // ✅ NEW RULE: Input validation
            if (userId == Guid.Empty)
            {
                return BadRequest(new ErrorResponse 
                { 
                    Message = "User ID is required",
                    Code = "INVALID_USER_ID"
                });
            }

            var logs = await _auditLogService.GetUserLogsAsync(userId, from, to);
            
            _logger.LogInformation(
                "✅ Retrieved {Count} audit logs for user {UserId} (from: {From}, to: {To})",
                logs.Count, userId, from, to);
            
            return Ok(logs);
        }
        catch (ArgumentException ex)
        {
            // ✅ NEW RULE: Proper error handling
            _logger.LogWarning(ex,
                "⚠️ Invalid argument for user audit log query: {UserId}",
                userId);
            
            return BadRequest(new ErrorResponse 
            { 
                Message = ex.Message,
                Code = "VALIDATION_ERROR"
            });
        }
        catch (Exception ex)
        {
            // ✅ NEW RULE: Proper error handling
            _logger.LogError(ex,
                "❌ Error getting user logs for {UserId}",
                userId);
            
            return StatusCode(500, new ErrorResponse 
            { 
                Message = "Failed to retrieve user logs. Please try again later.",
                Code = "INTERNAL_ERROR"
            });
        }
    }
}

/// <summary>
/// Standard error response format.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Machine-readable error code.
    /// </summary>
    public string Code { get; set; } = string.Empty;
}
