namespace VendorMdm.Shared.Ontology.Interfaces;

/// <summary>
/// Interface for entities that support audit logging through Ontology Concepts.
/// Each entity's Ontology Concept MUST implement this interface to define
/// what should be audited, how it should be audited, and what values to capture.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// Get the entity type name for audit logs (e.g., "Vendor", "VendorInvitation").
    /// This MUST match the entity name in the database.
    /// </summary>
    string GetEntityType();

    /// <summary>
    /// Get the fields that should be audited.
    /// Defines critical fields (MUST audit), standard fields (SHOULD audit),
    /// and sensitive fields (MUST NOT audit).
    /// </summary>
    AuditableFields GetAuditableFields();

    /// <summary>
    /// Create an audit log entry for a specific action.
    /// The Concept determines what values to include based on the action.
    /// </summary>
    /// <param name="action">Action performed (e.g., "Created", "Updated", "Deleted")</param>
    /// <param name="oldState">Previous state of the entity (for updates/deletes)</param>
    /// <param name="newState">New state of the entity (for creates/updates)</param>
    /// <param name="reason">Human-readable reason for the action</param>
    /// <returns>Audit log entry ready to be persisted</returns>
    AuditLogEntry CreateAuditEntry(
        string action,
        object? oldState = null,
        object? newState = null,
        string? reason = null);

    /// <summary>
    /// Determine if a specific action should be audited.
    /// Allows Concepts to define which actions are audit-worthy.
    /// </summary>
    /// <param name="action">Action to check (e.g., "Created", "Updated")</param>
    /// <returns>True if action should be audited, false otherwise</returns>
    bool ShouldAudit(string action);
}

/// <summary>
/// Defines which fields of an entity should be audited.
/// </summary>
public class AuditableFields
{
    /// <summary>
    /// Fields that MUST be audited (e.g., LegalName, Status, TaxId).
    /// These are critical business fields that require full audit trail.
    /// </summary>
    public List<string> CriticalFields { get; set; } = new();

    /// <summary>
    /// Fields that SHOULD be audited (e.g., Email, Phone, Address).
    /// These are important fields but not critical for compliance.
    /// </summary>
    public List<string> StandardFields { get; set; } = new();

    /// <summary>
    /// Fields that MUST NOT be audited (e.g., Password, SSN, BankAccount).
    /// These contain sensitive data that should never appear in audit logs.
    /// </summary>
    public List<string> SensitiveFields { get; set; } = new();

    /// <summary>
    /// Get all auditable fields (critical + standard).
    /// </summary>
    public List<string> GetAllAuditableFields()
    {
        return CriticalFields.Concat(StandardFields).ToList();
    }

    /// <summary>
    /// Check if a field should be audited.
    /// </summary>
    public bool IsAuditable(string fieldName)
    {
        return CriticalFields.Contains(fieldName) || StandardFields.Contains(fieldName);
    }

    /// <summary>
    /// Check if a field is sensitive and should NOT be audited.
    /// </summary>
    public bool IsSensitive(string fieldName)
    {
        return SensitiveFields.Contains(fieldName);
    }
}

/// <summary>
/// Represents an audit log entry created by an Ontology Concept.
/// </summary>
public class AuditLogEntry
{
    /// <summary>
    /// Entity type (e.g., "Vendor", "VendorInvitation").
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Entity ID (GUID).
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// Action performed (e.g., "Created", "Updated", "Deleted").
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Previous values (for updates/deletes).
    /// Only includes auditable fields, excludes sensitive fields.
    /// </summary>
    public object? OldValues { get; set; }

    /// <summary>
    /// New values (for creates/updates).
    /// Only includes auditable fields, excludes sensitive fields.
    /// </summary>
    public object? NewValues { get; set; }

    /// <summary>
    /// Human-readable reason for the action.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Additional metadata (e.g., VendorType, AccountGroup, EventType).
    /// Concept-specific contextual information.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Schema version of the audit model.
    /// Used for audit log evolution when entity schema changes.
    /// </summary>
    public string SchemaVersion { get; set; } = "v1.0.0";
}
