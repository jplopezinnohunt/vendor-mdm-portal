using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
/// </summary>
public abstract class CanonicalEntityBase
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
    [Column(TypeName = "nvarchar(max)")]
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
    
    public static readonly string[] All = { Portal, Sap, Api, Migration, Batch };
    
    public static bool IsValid(string sourceSystem) => All.Contains(sourceSystem);
}
