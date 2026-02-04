namespace VendorMdm.Shared.Ontology.Interfaces;

/// <summary>
/// Interface for entities that support soft delete (Pattern 11 from moderngoldenrules.md).
/// Implements "NEVER hard delete data" rule - all deletions are reversible.
///
/// Access Control:
/// - Soft delete operations require Admin role
/// - Restore operations require Admin role
/// - Default queries exclude soft-deleted records via HasQueryFilter
/// - Admin can view deleted records via IgnoreQueryFilters()
///
/// Audit Trail:
/// - All soft delete operations are logged with "SoftDeleted" action
/// - All restore operations are logged with "Restored" action
/// - Reason is mandatory for both operations
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Indicates if the entity has been soft deleted.
    /// When true, entity is excluded from default queries.
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// Timestamp when the entity was soft deleted (UTC).
    /// Null if not deleted.
    /// </summary>
    DateTime? DeletedAt { get; set; }

    /// <summary>
    /// User who performed the soft delete (email or user ID).
    /// Null if not deleted.
    /// </summary>
    string? DeletedBy { get; set; }

    /// <summary>
    /// Perform a soft delete on this entity.
    /// Sets IsDeleted=true, DeletedAt=now, DeletedBy=user.
    /// </summary>
    /// <param name="deletedBy">User performing the delete (email or ID)</param>
    void SoftDelete(string deletedBy);

    /// <summary>
    /// Restore a soft-deleted entity.
    /// Sets IsDeleted=false, clears DeletedAt and DeletedBy.
    /// </summary>
    /// <param name="restoredBy">User performing the restore (for logging)</param>
    void Restore(string restoredBy);
}
