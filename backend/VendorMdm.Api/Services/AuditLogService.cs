using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VendorMdm.Api.Data;
using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Services;

/// <summary>
/// Implementation of Audit Log service (Pattern 16: Audit Trail & Temporal).
/// Provides comprehensive error handling following UI Error Handling Standard.
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly SqlDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        SqlDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditLogService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task LogAsync(
        string entityType,
        Guid entityId,
        string action,
        object? oldValues = null,
        object? newValues = null,
        string? reason = null)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var user = httpContext?.User;

            // Extract user information from claims
            var changedBy = user?.FindFirst(ClaimTypes.Email)?.Value 
                ?? user?.FindFirst(ClaimTypes.Name)?.Value 
                ?? "System";
            
            var changedByUserId = Guid.Empty;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                Guid.TryParse(userIdClaim, out changedByUserId);
            }

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                ChangedBy = changedBy,
                ChangedByUserId = changedByUserId,
                ChangedAt = DateTime.UtcNow,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                Reason = reason,
                IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
                UserAgent = httpContext?.Request.Headers["User-Agent"].ToString(),
                TenantId = null // TODO: Get from user context when multi-tenancy is implemented
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "✅ Audit log created: {EntityType} {EntityId} {Action} by {User}",
                entityType, entityId, action, changedBy);
        }
        catch (Exception ex)
        {
            // ✅ NEW RULE: Proper error handling
            // Audit logging should NOT break main operations
            // Log the error but don't throw
            _logger.LogError(ex,
                "❌ Failed to create audit log for {EntityType} {EntityId} {Action}",
                entityType, entityId, action);

            // Don't throw - audit logging is non-critical
            // The main operation should continue even if audit logging fails
        }
    }

    /// <inheritdoc />
    public async Task<List<AuditLog>> GetEntityLogsAsync(
        string entityType,
        Guid entityId)
    {
        try
        {
            // ✅ NEW RULE: Input validation
            if (string.IsNullOrWhiteSpace(entityType))
            {
                throw new ArgumentException("Entity type cannot be empty", nameof(entityType));
            }

            if (entityId == Guid.Empty)
            {
                throw new ArgumentException("Entity ID cannot be empty", nameof(entityId));
            }

            var logs = await _context.AuditLogs
                .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                .OrderByDescending(a => a.ChangedAt)
                .ToListAsync();

            _logger.LogInformation(
                "📋 Retrieved {Count} audit logs for {EntityType} {EntityId}",
                logs.Count, entityType, entityId);

            return logs;
        }
        catch (ArgumentException)
        {
            // Re-throw validation errors
            throw;
        }
        catch (Exception ex)
        {
            // ✅ NEW RULE: Proper error handling
            _logger.LogError(ex,
                "❌ Failed to get audit logs for {EntityType} {EntityId}",
                entityType, entityId);
            throw; // This is a query operation, so we can throw
        }
    }

    /// <inheritdoc />
    public async Task<List<AuditLog>> GetUserLogsAsync(
        Guid userId,
        DateTime? from = null,
        DateTime? to = null)
    {
        try
        {
            // ✅ NEW RULE: Input validation
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            }

            var query = _context.AuditLogs
                .Where(a => a.ChangedByUserId == userId);

            if (from.HasValue)
            {
                query = query.Where(a => a.ChangedAt >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(a => a.ChangedAt <= to.Value);
            }

            var logs = await query
                .OrderByDescending(a => a.ChangedAt)
                .Take(1000) // ✅ Limit results to prevent performance issues
                .ToListAsync();

            _logger.LogInformation(
                "📋 Retrieved {Count} audit logs for user {UserId} (from: {From}, to: {To})",
                logs.Count, userId, from, to);

            return logs;
        }
        catch (ArgumentException)
        {
            // Re-throw validation errors
            throw;
        }
        catch (Exception ex)
        {
            // ✅ NEW RULE: Proper error handling
            _logger.LogError(ex,
                "❌ Failed to get user logs for {UserId}",
                userId);
            throw;
        }
    }
}
