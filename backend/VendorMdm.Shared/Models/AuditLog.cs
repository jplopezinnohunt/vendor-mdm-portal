using System;

namespace VendorMdm.Shared.Models;

/// <summary>
/// Audit log for tracking all entity changes (Pattern 16: Audit Trail)
/// Provides complete "who, what, when, why" tracking for compliance
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Type of entity being tracked (e.g., "Vendor", "Event", "User")
    /// </summary>
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the entity being tracked
    /// </summary>
    public Guid EntityId { get; set; }
    
    /// <summary>
    /// Action performed: "Created", "Updated", "Deleted", "Approved", "Rejected"
    /// </summary>
    public string Action { get; set; } = string.Empty;
    
    /// <summary>
    /// Email of user who made the change
    /// </summary>
    public string ChangedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// User ID who made the change
    /// </summary>
    public Guid ChangedByUserId { get; set; }
    
    /// <summary>
    /// Timestamp of the change (UTC)
    /// </summary>
    public DateTime ChangedAt { get; set; }
    
    /// <summary>
    /// JSON snapshot of entity state BEFORE the change
    /// </summary>
    public string? OldValues { get; set; }
    
    /// <summary>
    /// JSON snapshot of entity state AFTER the change
    /// </summary>
    public string? NewValues { get; set; }
    
    /// <summary>
    /// User-provided justification for the change (required for sensitive operations)
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// IP address of the user making the change
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent (browser) of the user making the change
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Tenant ID (for multi-tenancy support - Pattern 15)
    /// </summary>
    public Guid? TenantId { get; set; }
}
