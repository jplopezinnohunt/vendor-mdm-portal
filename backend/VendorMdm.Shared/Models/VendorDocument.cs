using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VendorMdm.Shared.Constants;

namespace VendorMdm.Shared.Models;

/// <summary>
/// Vendor document entity with full taxonomy support.
/// Part of the Document Registry feature for tracking vendor compliance documents.
/// </summary>
public class VendorDocument
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the canonical Vendor entity.
    /// </summary>
    [Required]
    public Guid VendorId { get; set; }

    /// <summary>
    /// Document type code from DocumentType constants (e.g., DOCTYPE_VAT_CERT).
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>
    /// Category code from DocumentCategory constants (e.g., DOC_LEG_REG).
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string CategoryCode { get; set; } = string.Empty;

    /// <summary>
    /// Azure Blob Storage path (e.g., "vendors/{vendorId}/documents/{category}/{filename}").
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string BlobName { get; set; } = string.Empty;

    /// <summary>
    /// Original filename as uploaded by user.
    /// </summary>
    [MaxLength(255)]
    public string? OriginalFileName { get; set; }

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long? FileSize { get; set; }

    /// <summary>
    /// MIME type of the file (e.g., "application/pdf").
    /// </summary>
    [MaxLength(100)]
    public string? MimeType { get; set; }

    /// <summary>
    /// Security level from SecurityLevel constants (1=Public, 2=Internal, 3=Confidential, 4=Restricted).
    /// </summary>
    public int SecurityLevel { get; set; } = Constants.SecurityLevel.Internal;

    /// <summary>
    /// Current status from DocumentStatus constants (Pending, Uploaded, Processing, Verified, Rejected, Archived, Expired).
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = DocumentStatus.Pending;

    /// <summary>
    /// User who uploaded the document.
    /// </summary>
    [MaxLength(255)]
    public string? UploadedBy { get; set; }

    /// <summary>
    /// Timestamp when document was uploaded.
    /// </summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who verified/approved the document.
    /// </summary>
    [MaxLength(255)]
    public string? VerifiedBy { get; set; }

    /// <summary>
    /// Timestamp when document was verified.
    /// </summary>
    public DateTime? VerifiedAt { get; set; }

    /// <summary>
    /// Document expiry date (for certificates, licenses, etc.).
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Country code for country-specific documents (ISO 3166-1 alpha-2).
    /// </summary>
    [MaxLength(2)]
    public string? CountryCode { get; set; }

    /// <summary>
    /// SHA256 hash of file content for integrity verification.
    /// </summary>
    [MaxLength(64)]
    public string? ContentHash { get; set; }

    /// <summary>
    /// Reason for rejection (if Status = Rejected).
    /// </summary>
    [MaxLength(1000)]
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Semi-structured data for extracted information (OCR results, metadata).
    /// JSON column for flexible storage.
    /// </summary>
    public string ExtractedData { get; set; } = "{}";

    /// <summary>
    /// General attributes JSON column for extensibility.
    /// Stores: virusScanResult, thumbnailUrl, processingMetadata, etc.
    /// </summary>
    public string Attributes { get; set; } = "{}";

    /// <summary>
    /// Soft delete flag (Pattern 11).
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Soft delete timestamp.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// User who deleted the document.
    /// </summary>
    [MaxLength(255)]
    public string? DeletedBy { get; set; }
}
