using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Services;

/// <summary>
/// Service for managing audit logs (Pattern 16: Audit Trail & Temporal).
/// Provides comprehensive audit trail for compliance and temporal reconstruction.
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Log an action performed on an entity.
    /// </summary>
    /// <param name="entityType">Type of entity (e.g., "Vendor", "VendorInvitation")</param>
    /// <param name="entityId">ID of the entity</param>
    /// <param name="action">Action performed (e.g., "Created", "Updated", "Deleted", "Approved")</param>
    /// <param name="oldValues">Previous values (for updates)</param>
    /// <param name="newValues">New values</param>
    /// <param name="reason">Reason for the change (optional)</param>
    Task LogAsync(
        string entityType,
        Guid entityId,
        string action,
        object? oldValues = null,
        object? newValues = null,
        string? reason = null);

    /// <summary>
    /// Get all audit logs for a specific entity.
    /// </summary>
    /// <param name="entityType">Type of entity</param>
    /// <param name="entityId">ID of the entity</param>
    /// <returns>List of audit logs ordered by most recent first</returns>
    Task<List<AuditLog>> GetEntityLogsAsync(
        string entityType,
        Guid entityId);

    /// <summary>
    /// Get all audit logs for a specific user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="from">Start date (optional)</param>
    /// <param name="to">End date (optional)</param>
    /// <returns>List of audit logs ordered by most recent first (max 1000)</returns>
    Task<List<AuditLog>> GetUserLogsAsync(
        Guid userId,
        DateTime? from = null,
        DateTime? to = null);
}
