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
    
    /// <summary>
    /// Pattern 15: Multi-Tenancy - Tenant/Agency ID (e.g., UNDP, UNICEF, WHO).
    /// Null for global/shared vendors.
    /// </summary>
    public Guid? TenantId { get; set; }
    
    /// <summary>
    /// Pattern 14: Data Residency - Geographic region for data storage compliance.
    /// Values: "EU", "US", "APAC", "GLOBAL"
    /// </summary>
    [MaxLength(20)]
    public string DataResidencyRegion { get; set; } = "GLOBAL";
    
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
/// External System ID mapping table - anti-corruption layer.
/// Maps canonical entity IDs to external system IDs (SAP, Salesforce, SuccessFactors, etc.).
/// Supports multi-system integration strategy.
/// </summary>
public class ExternalSystemMapping
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Canonical entity ID (e.g., Vendor.Id, Employee.Id)
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
    /// External system ID (e.g., SAP LIFNR, Salesforce Account ID, SuccessFactors Person ID)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ExternalSystemId { get; set; } = string.Empty;
    
    /// <summary>
    /// External system name (e.g., "SAP", "Salesforce", "SuccessFactors", "Workday")
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string SystemName { get; set; } = string.Empty;
    
    /// <summary>
    /// System environment or instance (e.g., "D01", "Production", "Sandbox")
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string SystemEnvironment { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Canonical Employee entity.
/// Represents internal staff members.
/// </summary>
public class Employee : CanonicalEntityBase
{
    [Required]
    [MaxLength(100)]
    public string GivenName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Surname { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Business-facing Employee ID (e.g., from HR system).
    /// NOT the database primary key.
    /// </summary>
    [MaxLength(50)]
    public string? EmployeeId { get; set; }

    // Inherits Attributes for: Department, Title, ManagerId, etc.
}

/// <summary>
/// Canonical Project entity.
/// Represents a strict business project / WBS element.
/// </summary>
public class Project : CanonicalEntityBase
{
    [Required]
    [MaxLength(50)]
    public string ProjectCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Inherits Attributes for: Type, Status, Budget, ManagerId
}

/// <summary>
/// Canonical Fund entity.
/// Represents a financial funding source or cost center group.
/// </summary>
public class Fund : CanonicalEntityBase
{
    [Required]
    [MaxLength(50)]
    public string FundCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(4)]
    public string? FiscalYear { get; set; }

    // Inherits Attributes for: Type, Description, Currency
}

/// <summary>
/// Canonical Customer entity.
/// Represents a client or customer organization.
/// </summary>
public class Customer : CanonicalEntityBase
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? TaxId { get; set; }

    [EmailAddress]
    [MaxLength(255)]
    public string? PrimaryContactEmail { get; set; }

    // Inherits Attributes for: Address, Industry, Segment
}

/// <summary>
/// Canonical User entity.
/// Represents a platform user (migrated from UserRole).
/// </summary>
public class User : CanonicalEntityBase
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public List<string> Roles { get; set; } = new() { "Viewer" }; // Admin, Requestor, Approver, Viewer

    /// <summary>
    /// Links this user to their Azure AD Object ID (GUID).
    /// Populated automatically upon first SSO login.
    /// </summary>
    [MaxLength(50)]
    public string? AzureAdObjectId { get; set; }

    /// <summary>
    /// Indicates the source of the identity: "Local" or "AzureAd".
    /// </summary>
    [MaxLength(20)]
    public string AuthProvider { get; set; } = "Local";

    /// <summary>
    /// Password Hash for Local Auth (only used for Bootstrap/Break-glass accounts).
    /// </summary>
    public string? PasswordHash { get; set; }

    public DateTime? LastLogonAt { get; set; }
    public bool IsBlocked { get; set; } = false;

    // --- Invitation & Security ---
    [MaxLength(100)]
    public string? InvitationToken { get; set; }
    
    public DateTime? InvitationExpiresAt { get; set; }
    
    [MaxLength(200)]
    public string? TwoFactorSecret { get; set; } // Base32 encoded secret
    
    public bool TwoFactorEnabled { get; set; } = false;
    
    public string? RecoveryCodes { get; set; } = "[]"; // JSON Array

    // --- Multi-Channel Auth ---
    [Required] 
    public string AuthMethod { get; set; } = "MagicLink"; // "AzureAd", "MagicLink", "LocalStrong"

    [MaxLength(100)]
    public string? MagicLinkToken { get; set; }

    public DateTime? MagicLinkExpiresAt { get; set; }

    // Inherits Attributes for: FullName, Department, UiPreferences, NotificationSettings
}
/// <summary>
/// Document Registry - Enterprise document management with lifecycle tracking.
/// Adapted from banking-grade architecture for vendor document handling.
/// 
/// Usage:
/// - Stores metadata for all uploaded documents (trade licenses, passports, bank certs)
/// - Supports security classification (1=Public to 4=Restricted/PII)
/// - Tracks document lifecycle (Pending → Verified → Archived)
/// - OCR-ready with extracted_data in JSONB
/// - Expiry tracking for compliance alerts
/// </summary>
public class DocumentRegistry : CanonicalEntityBase
{
    // ============================================
    // STRUCTURED COLUMNS (SQL) - For Indexing & Search
    // ============================================
    
    /// <summary>
    /// Entity type this document belongs to (e.g., "Vendor", "Employee", "Transaction")
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// Reference ID of the entity (e.g., Vendor UUID)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EntityRef { get; set; } = string.Empty;
    
    /// <summary>
    /// Document category for organization: "Legal", "Identity", "Finance", "Tax", "Banking"
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Specific document type: "TradeLicense", "Passport", "BankCertificate", "VatCertificate"
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string DocType { get; set; } = string.Empty;
    
    /// <summary>
    /// Security classification level:
    /// 1 = Public (non-sensitive)
    /// 2 = Internal (standard business docs)
    /// 3 = Confidential (sensitive business)
    /// 4 = Restricted (PII/highly sensitive - passports, IDs, salary slips)
    /// </summary>
    public int SecurityLevel { get; set; } = 2; // Default: Internal
    
    /// <summary>
    /// Azure Blob Storage path (key): {env}/{entityType}/{entityRef}/{category}/{docType}/{timestamp}_{guid}_{filename}.{ext}
    /// Example: prod/vendors/vendor-123/legal/trade-license/20260103_uuid_license.pdf
    /// </summary>
    [Required]
    public string StoragePath { get; set; } = string.Empty;
    
    /// <summary>
    /// MIME type (application/pdf, image/jpeg, etc.)
    /// </summary>
    [MaxLength(100)]
    public string? MimeType { get; set; }
    
    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSizeBytes { get; set; }
    
    // ============================================
    // LIFECYCLE MANAGEMENT (Compliance Critical)
    // ============================================
    
    /// <summary>
    /// Document status in workflow: "Pending", "Verified", "Rejected", "Archived"
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string DocumentStatus { get; set; } = "Pending";
    
    /// <summary>
    /// Expiration date (NULL if doesn't expire).
    /// INDEXED for compliance alerts (find expiring docs in next 30 days).
    /// </summary>
    public DateTime? ExpiryDate { get; set; }
    
    /// <summary>
    /// Who uploaded the document (user email or system identifier)
    /// </summary>
    [MaxLength(255)]
    public string? UploadedBy { get; set; }
    
    // ============================================
    // JSONB DATA (Semi-Structured) - Inherited
    // ============================================
    // From CanonicalEntityBase:
    // - Data (JSONB) - DocumentRegistryAttributes:
    //   {
    //     "extractedData": { "licenseNumber": "CN-123", "issuingAuthority": "DED Dubai" },
    //     "auditLog": [ { "action": "Viewed", "by": "user@x.com", "at": "..." } ],
    //     "ocrConfidence": 0.95,
    //     "scanResult": { "status": "clean", "engine": "Azure Defender" }
    //   }
}
