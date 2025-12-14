using System.ComponentModel.DataAnnotations;

namespace VendorMdm.Shared.Models;

/// <summary>
/// Canonical Vendor entity - master record for all vendor data.
/// Replaces VendorApplication as single source of truth.
/// 
/// Usage:
/// - Created from VendorInvitation → VendorApplication flow
/// - Can also be created directly via API or SAP sync
/// - Mapped to SAP Business Partner via SapMapper (no SAP fields here)
/// </summary>
public class Vendor : CanonicalEntityBase
{
    /// <summary>
    /// Legal company name (searchable, indexed).
    /// Universal field - all vendors must have this.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string LegalName { get; set; } = string.Empty;
    
    /// <summary>
    /// Tax identification number (e.g., VAT, EIN).
    /// Indexed for search and validation.
    /// </summary>
    [MaxLength(100)]
    public string? TaxId { get; set; }
    
    /// <summary>
    /// Primary contact email address (universal, indexed).
    /// Used for communications and notifications.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string PrimaryContactEmail { get; set; } = string.Empty;
    
    // Inherited from CanonicalEntityBase:
    // - Id (UUID)
    // - EntityVersion (int)
    // - Status (string) - use VendorStatus constants
    // - SourceSystem (string)
    // - Data (JSON) - stores certifications, addresses, contacts, etc.
    // - CreatedAt, UpdatedAt (DateTime)
    // - SchemaVersion (string)
}

/// <summary>
/// Vendor lifecycle status values and state machine.
/// </summary>
public static class VendorStatus
{
    public const string Pending = "Pending";
    public const string Active = "Active";
    public const string Suspended = "Suspended";
    public const string Archived = "Archived";
    
    private static readonly Dictionary<string, HashSet<string>> ValidTransitions = new()
    {
        [Pending] = new() { Active, Archived },
        [Active] = new() { Suspended, Archived },
        [Suspended] = new() { Active, Archived },
        [Archived] = new() { } // Terminal state
    };
    
    public static bool IsValidTransition(string from, string to)
    {
        return ValidTransitions.ContainsKey(from) && 
               ValidTransitions[from].Contains(to);
    }
    
    public static string[] GetAllowedTransitions(string currentStatus)
    {
        return ValidTransitions.ContainsKey(currentStatus) 
            ? ValidTransitions[currentStatus].ToArray() 
            : Array.Empty<string>();
    }
}

/// <summary>
/// Canonical VendorInvitation entity (migrated to canonical pattern).
/// Tracks invitation-based vendor onboarding.
/// </summary>
public class VendorInvitationCanonical : CanonicalEntityBase
{
    [Required]
    [MaxLength(100)]
    public string InvitationToken { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string VendorLegalName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string PrimaryContactEmail { get; set; } = string.Empty;
    
    [Required]
    public Guid InvitedBy { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string InvitedByName { get; set; } = string.Empty;
    
    [Required]
    public DateTime ExpiresAt { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    public Guid? VendorId { get; set; } // Links to canonical Vendor
    
    // Status values: Pending, Accepted, Expired, Completed, Cancelled
    // Data JSON: notes, customFields, metadata
}

/// <summary>
/// Canonical ChangeRequest entity (migrated to canonical pattern).
/// Tracks vendor data modification requests.
/// NO SAP FIELDS - use SapIdMapping for SAP vendor lookup.
/// </summary>
public class ChangeRequestCanonical : CanonicalEntityBase
{
    [Required]
    public Guid VendorId { get; set; } // Canonical Vendor ID (not SAP ID!)
    
    [Required]
    public Guid RequesterId { get; set; }
    
    // Status values: Draft, Submitted, Approved, Integrated, Rejected
    // Data JSON: approvalHistory, rejectionReason, impactAssessment, changes
}

/// <summary>
/// SAP ID mapping table - anti-corruption layer.
/// Maps canonical entity IDs to SAP system IDs.
/// NO SAP fields should exist in canonical entities!
/// </summary>
public class SapIdMapping
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Canonical entity ID (e.g., Vendor.Id)
    /// </summary>
    [Required]
    public Guid CanonicalEntityId { get; set; }
    
    /// <summary>
    /// Entity type (e.g., "Vendor", "Customer", "Employee")
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// SAP ID (e.g., LIFNR for vendors, KUNNR for customers)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string SapId { get; set; } = string.Empty;
    
    /// <summary>
    /// SAP environment (D01, Q01, P01)
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string SapEnvironment { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
