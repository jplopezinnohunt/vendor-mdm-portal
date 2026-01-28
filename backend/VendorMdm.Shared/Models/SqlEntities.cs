using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendorMdm.Shared.Models;

public class ChangeRequest
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Draft"; // Draft, Submitted, Approved, Integrated

    public string? SapVendorId { get; set; } // Null for new vendors

    [Required]
    public Guid RequesterId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Semi-structured attributes for flexible data storage.
    /// Stores: approvalHistory, rejectionReason, changeImpactAssessment, notificationsSent
    /// </summary>
    public string Attributes { get; set; } = "{}";
}

public class VendorApplication
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? TaxId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ContactName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string ContactEmail { get; set; } = string.Empty;

    public string Status { get; set; } = "Pending";
    
    [MaxLength(20)]
    public string RegistrationType { get; set; } = "SelfRegistration";
    
    public Guid? InvitationId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Semi-structured attributes for flexible data storage.
    /// Stores: industryMetadata, certifications, additionalContacts, customFields
    /// </summary>
    public string Attributes { get; set; } = "{}";
}

public class WorkflowState
{
    [Key]
    [MaxLength(20)]
    public string StateName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Semi-structured attributes for flexible data storage.
    /// Stores: displayOrder, colorCode, iconName, transitionsAllowed
    /// </summary>
    public string Attributes { get; set; } = "{}";
}

public class SapEnvironment
{
    [Key]
    [MaxLength(3)]
    public string EnvironmentCode { get; set; } = string.Empty; // D01, Q01, P01

    public string Description { get; set; } = string.Empty;
}

public class UserRole
{
    [Key]
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = "User"; // Admin, Requestor, Approver

    /// <summary>
    /// Semi-structured attributes for flexible data storage.
    /// Stores: fullName, email, phoneNumber, department, uiPreferences, notificationSettings
    /// </summary>
    public string Attributes { get; set; } = "{}";
}

public class Attachment
{
    [Key]
    public Guid Id { get; set; }
    public Guid LinkedEntityId { get; set; } // ChangeRequest or VendorApplication
    public string FileName { get; set; } = string.Empty;
    public string BlobUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Semi-structured attributes for flexible data storage.
    /// Stores: fileSizeBytes, mimeType, uploadedByName, virusScanResult, thumbnailUrl
    /// </summary>
    public string Attributes { get; set; } = "{}";
}

public class VendorInvitation
{
    [Key]
    public Guid Id { get; set; }

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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime ExpiresAt { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = InvitationStatus.Pending;

    public DateTime? CompletedAt { get; set; }

    public Guid? VendorApplicationId { get; set; }

    [MaxLength(50)]
    public string VendorType { get; set; } = string.Empty;

    [MaxLength(10)]
    public string AccountGroup { get; set; } = string.Empty;

    [MaxLength(20)]
    public string SanctionsStatus { get; set; } = "NotScreened";

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? SanctionsScore { get; set; }

    [MaxLength(20)]
    public string ReviewStatus { get; set; } = "NotRequired";

    [MaxLength(1000)]
    [Obsolete("Use Attributes JSON column instead. Will be removed in next major version.")]
    public string? Notes { get; set; }

    /// <summary>
    /// Current stage of the invitation workflow.
    /// Stage 1: InvitationSent
    /// Stage 2: MfaVerified
    /// Stage 3: InitialInfoCompleted
    /// Stage 4: Enriched
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string CurrentStage { get; set; } = InvitationStage.InvitationSent;

    /// <summary>
    /// Semi-structured attributes for flexible data storage.
    /// Stores: notes, customFields, invitationMetadata, uiPreferences, mfaCode, mfaCodeExpiresAt
    /// </summary>
    public string Attributes { get; set; } = "{}";
}

public static class InvitationStage
{
    public const string InvitationSent = "InvitationSent";
    public const string MfaVerified = "MfaVerified";
    public const string InitialInfoCompleted = "InitialInfoCompleted";
    public const string Enriched = "Enriched";
}

public static class InvitationStatus
{
    public const string Pending = "Pending";
    public const string Accepted = "Accepted";
    public const string Expired = "Expired";
    public const string Completed = "Completed";
    public const string PendingReview = "PendingReview";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";
}
