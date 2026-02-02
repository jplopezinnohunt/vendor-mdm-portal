using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Data;

/// <summary>
/// Audit middleware that automatically captures all entity changes.
/// Implements Pattern 16: Audit Trail & Temporal.
/// </summary>
public class AuditInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditInterceptor> _logger;

    public AuditInterceptor(
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditInterceptor> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Captures audit logs before SaveChanges is called.
    /// </summary>
    public List<AuditLog> CaptureAuditLogs(DbContext context)
    {
        var auditLogs = new List<AuditLog>();
        var httpContext = _httpContextAccessor.HttpContext;

        // Get user information from claims
        var userEmail = httpContext?.User?.Identity?.Name ?? "System";
        var userIdClaim = httpContext?.User?.Claims?.FirstOrDefault(c => c.Type == "sub" || c.Type == "id");
        var userId = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var uid) ? uid : Guid.Empty;

        // Get IP address and user agent
        var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = httpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown";

        // Process each changed entity
        foreach (var entry in context.ChangeTracker.Entries())
        {
            // Skip AuditLog itself to avoid recursion
            if (entry.Entity is AuditLog)
                continue;

            // Only track entities with changes
            if (entry.State == EntityState.Unchanged)
                continue;

            var auditLog = CreateAuditLog(entry, userEmail, userId, ipAddress, userAgent);
            if (auditLog != null)
            {
                auditLogs.Add(auditLog);
            }
        }

        return auditLogs;
    }

    private AuditLog? CreateAuditLog(
        EntityEntry entry,
        string userEmail,
        Guid userId,
        string ipAddress,
        string userAgent)
    {
        var entityType = entry.Entity.GetType().Name;
        var entityId = GetEntityId(entry);

        if (entityId == Guid.Empty)
        {
            _logger.LogWarning("Could not extract entity ID for {EntityType}", entityType);
            return null;
        }

        var action = entry.State switch
        {
            EntityState.Added => "Created",
            EntityState.Modified => "Updated",
            EntityState.Deleted => "Deleted",
            _ => "Unknown"
        };

        string? oldValues = null;
        string? newValues = null;

        try
        {
            if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
            {
                // Capture original values
                var originalValues = entry.Properties
                    .Where(p => p.IsModified || entry.State == EntityState.Deleted)
                    .ToDictionary(
                        p => p.Metadata.Name,
                        p => p.OriginalValue
                    );
                oldValues = JsonSerializer.Serialize(originalValues);
            }

            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                // Capture current values
                var currentValues = entry.Properties
                    .Where(p => entry.State == EntityState.Added || p.IsModified)
                    .ToDictionary(
                        p => p.Metadata.Name,
                        p => p.CurrentValue
                    );
                newValues = JsonSerializer.Serialize(currentValues);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serializing audit values for {EntityType}", entityType);
        }

        return new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            ChangedBy = userEmail,
            ChangedByUserId = userId,
            ChangedAt = DateTime.UtcNow,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            // TenantId will be set by multi-tenancy pattern (Phase 10B)
            TenantId = null
        };
    }

    private Guid GetEntityId(EntityEntry entry)
    {
        // Try to get Id property
        var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
        if (idProperty?.CurrentValue is Guid id)
        {
            return id;
        }

        return Guid.Empty;
    }
}
