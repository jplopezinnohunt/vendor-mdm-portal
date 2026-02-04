using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VendorMdm.Shared.Ontology.Interfaces;

namespace VendorMdm.Shared.Models;

/// <summary>
/// Base class for all canonical entities in the Vendor Platform.
/// MANDATORY: All domain entities MUST inherit from this base class.
///
/// Provides:
/// - Global immutable ID (UUID)
/// - Entity versioning for optimistic concurrency
/// - Lifecycle status tracking
/// - Source system tracking
/// - Semi-structured data storage (JSONB pattern)
/// - Audit timestamps
/// - Schema versioning
/// - Soft delete support (Pattern 11)
/// </summary>
public abstract class CanonicalEntityBase : ISoftDeletable
{
    /// <summary>
    /// Global immutable identifier (UUID).
    /// Unique across all systems and applications.
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Entity version for optimistic concurrency control and audit trail.
    /// Increments on each update.
    /// Used to detect concurrent modifications.
    /// </summary>
    [Required]
    public int EntityVersion { get; set; } = 1;
    
    /// <summary>
    /// Current lifecycle status of the entity.
    /// Values depend on entity type (e.g., Pending, Active, Suspended, Archived).
    /// Must be defined by entity-specific status enum/constants.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Active";
    
    /// <summary>
    /// Source system that created/owns this entity.
    /// Valid values: Portal, SAP, API, Migration, Batch
    /// Used for audit trail and data lineage tracking.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string SourceSystem { get; set; } = "Portal";
    
    /// <summary>
    /// Semi-structured data payload containing entity-specific attributes.
    /// Stored as JSON (nvarchar(max) - SQL Server JSONB equivalent).
    /// MUST be validated against versioned JSON Schema before persistence.
    /// 
    /// Use for:
    /// - Volatile/evolving data
    /// - Context-specific fields
    /// - Nested structures
    /// - UI preferences
    /// </summary>
    [Required]
    public string Data { get; set; } = "{}";
    
    /// <summary>
    /// Entity creation timestamp (UTC).
    /// Automatically set on creation, immutable.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Last update timestamp (UTC).
    /// Automatically updated on each modification.
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Schema version for the Data JSON payload.
    /// Format: v{major}.{minor}.{patch} (e.g., "v1.0.0", "v2.1.0")
    /// Used for schema evolution and backward compatibility.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string SchemaVersion { get; set; } = "v1.0.0";

    // ============================================
    // SOFT DELETE FIELDS (Pattern 11)
    // ============================================

    /// <summary>
    /// Indicates if the entity has been soft deleted.
    /// Default queries exclude records where IsDeleted=true.
    /// Use IgnoreQueryFilters() to include deleted records.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Timestamp when the entity was soft deleted (UTC).
    /// Null if entity is not deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// User who performed the soft delete (email or user ID).
    /// Null if entity is not deleted.
    /// </summary>
    [MaxLength(256)]
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Perform a soft delete on this entity.
    /// Sets IsDeleted=true, DeletedAt=now, DeletedBy=user.
    /// IMPORTANT: Caller must log this action via IStructuredLogger and AuditLog.
    /// </summary>
    /// <param name="deletedBy">User performing the delete (email or ID)</param>
    public void SoftDelete(string deletedBy)
    {
        if (string.IsNullOrWhiteSpace(deletedBy))
            throw new ArgumentException("DeletedBy is required for audit trail", nameof(deletedBy));

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Restore a soft-deleted entity.
    /// Sets IsDeleted=false, clears DeletedAt and DeletedBy.
    /// IMPORTANT: Caller must log this action via IStructuredLogger and AuditLog.
    /// </summary>
    /// <param name="restoredBy">User performing the restore (for logging - not stored)</param>
    public void Restore(string restoredBy)
    {
        if (string.IsNullOrWhiteSpace(restoredBy))
            throw new ArgumentException("RestoredBy is required for audit trail", nameof(restoredBy));

        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
        UpdatedAt = DateTime.UtcNow;
    }

    // ============================================
    // ENTITY METHODS
    // ============================================

    /// <summary>
    /// Increment entity version (for updates).
    /// Call before saving changes to database.
    /// </summary>
    public void IncrementVersion()
    {
        EntityVersion++;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Validate that the entity has all required canonical fields.
    /// Throws exception if validation fails.
    /// </summary>
    public virtual void ValidateCanonicalFields()
    {
        if (Id == Guid.Empty)
            throw new InvalidOperationException("Canonical entity must have a valid Id");
            
        if (string.IsNullOrWhiteSpace(Status))
            throw new InvalidOperationException("Canonical entity must have a Status");
            
        if (string.IsNullOrWhiteSpace(SourceSystem))
            throw new InvalidOperationException("Canonical entity must have a SourceSystem");
            
        if (string.IsNullOrWhiteSpace(SchemaVersion))
            throw new InvalidOperationException("Canonical entity must have a SchemaVersion");
    }
}

/// <summary>
/// Valid source systems for canonical entities.
/// </summary>
public static class SourceSystems
{
    public const string Portal = "Portal";
    public const string Sap = "SAP";
    public const string Api = "API";
    public const string Migration = "Migration";
    public const string Batch = "Batch";
    public const string HR = "HR"; // Workday/SuccessFactors
    public const string Finance = "Finance"; // SAP FM
    public const string Projects = "Projects"; // SAP PS

    public static readonly string[] All = { Portal, Sap, Api, Migration, Batch, HR, Finance, Projects };

    /// <summary>
    /// Defines the default Authority (Source System) for each Entity Type in this App.
    /// </summary>
    private static readonly Dictionary<Type, string> EntityDefaults = new()
    {
        { typeof(Vendor), Sap },               // Mastered in SAP
        { typeof(Employee), HR },              // Mastered in HR
        { typeof(Project), Projects },         // Mastered in Project System
        { typeof(Fund), Finance },             // Mastered in Finance
        { typeof(Customer), Sap },             // Mastered in SAP
        { typeof(VendorInvitationCanonical), Portal }, // Local
        { typeof(ChangeRequestCanonical), Portal },    // Local
        { typeof(User), Portal }               // Local
    };

    public static string GetDefaultSource(Type entityType)
    {
        return EntityDefaults.TryGetValue(entityType, out var source) ? source : Portal;
    }
    
    public static bool IsValid(string sourceSystem) => All.Contains(sourceSystem);
}

/// <summary>
/// Extensions for checking entity origin.
/// </summary>
public static class EntityOriginExtensions 
{
    private static readonly string[] NativeSystems = { SourceSystems.Portal, SourceSystems.Api };

    /// <summary>
    /// Returns true if the entity is natively mastered in this platform.
    /// </summary>
    public static bool IsNative(this CanonicalEntityBase entity) 
    {
        return NativeSystems.Contains(entity.SourceSystem); 
    }

    /// <summary>
    /// Returns true if the entity is consumed from an external system.
    /// </summary>
    public static bool IsExternal(this CanonicalEntityBase entity)
    {
        return !IsNative(entity);
    }
}
