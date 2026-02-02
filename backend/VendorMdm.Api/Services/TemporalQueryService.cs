using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VendorMdm.Api.Data;
using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Services;

/// <summary>
/// Service for temporal queries - reconstruct entity state at any point in time.
/// Implements Pattern 16: Audit Trail & Temporal.
/// </summary>
public interface ITemporalQueryService
{
    Task<T?> GetEntityAsOfAsync<T>(Guid entityId, DateTime pointInTime) where T : class, new();
    Task<List<AuditLog>> GetEntityHistoryAsync(string entityType, Guid entityId);
    Task<List<AuditLog>> GetUserActivityAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null);
}

public class TemporalQueryService : ITemporalQueryService
{
    private readonly SqlDbContext _context;
    private readonly ILogger<TemporalQueryService> _logger;

    public TemporalQueryService(SqlDbContext context, ILogger<TemporalQueryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Reconstructs entity state at a specific point in time from audit logs.
    /// </summary>
    public async Task<T?> GetEntityAsOfAsync<T>(Guid entityId, DateTime pointInTime) where T : class, new()
    {
        var entityType = typeof(T).Name;

        // Get all audit logs up to the point in time, ordered chronologically
        var auditLogs = await _context.AuditLogs
            .Where(a => a.EntityType == entityType && 
                       a.EntityId == entityId && 
                       a.ChangedAt <= pointInTime)
            .OrderBy(a => a.ChangedAt)
            .ToListAsync();

        if (!auditLogs.Any())
        {
            _logger.LogWarning("No audit history found for {EntityType} {EntityId} before {PointInTime}", 
                entityType, entityId, pointInTime);
            return null;
        }

        // Start with empty entity
        var entity = new T();
        var entityDict = new Dictionary<string, object?>();

        // Replay all changes chronologically
        foreach (var log in auditLogs)
        {
            if (log.Action == "Created" && log.NewValues != null)
            {
                // Initial creation
                var newValues = JsonSerializer.Deserialize<Dictionary<string, object?>>(log.NewValues);
                if (newValues != null)
                {
                    foreach (var kvp in newValues)
                    {
                        entityDict[kvp.Key] = kvp.Value;
                    }
                }
            }
            else if (log.Action == "Updated" && log.NewValues != null)
            {
                // Apply updates
                var newValues = JsonSerializer.Deserialize<Dictionary<string, object?>>(log.NewValues);
                if (newValues != null)
                {
                    foreach (var kvp in newValues)
                    {
                        entityDict[kvp.Key] = kvp.Value;
                    }
                }
            }
            else if (log.Action == "Deleted")
            {
                // Entity was deleted before this point in time
                _logger.LogInformation("{EntityType} {EntityId} was deleted at {DeletedAt}", 
                    entityType, entityId, log.ChangedAt);
                return null;
            }
        }

        // Map dictionary back to entity
        try
        {
            var json = JsonSerializer.Serialize(entityDict);
            entity = JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reconstructing {EntityType} {EntityId} as of {PointInTime}", 
                entityType, entityId, pointInTime);
        }

        return entity;
    }

    /// <summary>
    /// Gets complete change history for an entity.
    /// </summary>
    public async Task<List<AuditLog>> GetEntityHistoryAsync(string entityType, Guid entityId)
    {
        return await _context.AuditLogs
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.ChangedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all activity by a specific user.
    /// </summary>
    public async Task<List<AuditLog>> GetUserActivityAsync(
        Guid userId, 
        DateTime? startDate = null, 
        DateTime? endDate = null)
    {
        var query = _context.AuditLogs
            .Where(a => a.ChangedByUserId == userId);

        if (startDate.HasValue)
        {
            query = query.Where(a => a.ChangedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.ChangedAt <= endDate.Value);
        }

        return await query
            .OrderByDescending(a => a.ChangedAt)
            .ToListAsync();
    }
}
