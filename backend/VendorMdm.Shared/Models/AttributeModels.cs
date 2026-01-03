namespace VendorMdm.Shared.Models;

#region VendorInvitation Attributes

/// <summary>
/// Attributes for VendorInvitation entity
/// </summary>
public class VendorInvitationAttributes
{
    public string? Notes { get; set; }
    public Dictionary<string, string>? CustomFields { get; set; }
    public InvitationMetadata? Metadata { get; set; }
}

public class InvitationMetadata
{
    public string? CampaignId { get; set; }
    public string? Source { get; set; }
    public Dictionary<string, string>? Tags { get; set; }
}

#endregion

#region VendorApplication Attributes

/// <summary>
/// Attributes for VendorApplication entity
/// </summary>
public class VendorApplicationAttributes
{
    public string? VendorType { get; set; }
    public string? AccountGroup { get; set; }
    public string? IndustryCode { get; set; }
    public List<string>? Certifications { get; set; }
    public List<AdditionalContact>? AdditionalContacts { get; set; }
    public Dictionary<string, object>? CustomFields { get; set; }
    public string? ApplicationNotes { get; set; }
    
    // NEW: Extended structured data for personal details enhancement
    public AddressDetails? Address { get; set; }
    public ContactDetails? ContactInfo { get; set; }
    public List<AttachmentMetadata>? Attachments { get; set; }
    public Dictionary<string, object>? OtherDetails { get; set; } // Vendor-type-specific fields
}

/// <summary>
/// Address details - supports full SAP address structure
/// </summary>
public class AddressDetails
{
    public string? HouseNo { get; set; }
    public string? StreetName { get; set; }
    public string? StreetName2 { get; set; }
    public string? StreetName3 { get; set; }
    public string? StreetName4 { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
}

/// <summary>
/// Contact information - extended with payment email and fax
/// </summary>
public class ContactDetails
{
    public string? Phone { get; set; }
    public string? MobilePhone { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? PaymentEmail { get; set; }
}

/// <summary>
/// Attachment metadata - references blobs in Azure Storage
/// </summary>
public class AttachmentMetadata
{
    public string FileName { get; set; } = string.Empty;
    public string BlobName { get; set; } = string.Empty; // GUID-based blob identifier
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
    public string Category { get; set; } = string.Empty; // "Identification", "BusinessLicense", etc.
    public string? UploadedBy { get; set; } // User ID or email
}

public class AdditionalContact
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty;
}


#endregion

#region ChangeRequest Attributes

/// <summary>
/// Attributes for ChangeRequest entity
/// </summary>
public class ChangeRequestAttributes
{
    public List<ApprovalHistoryEntry>? ApprovalHistory { get; set; }
    public string? RejectionReason { get; set; }
    public ChangeImpactAssessment? ImpactAssessment { get; set; }
    public List<string>? NotificationsSent { get; set; }
}

public class ApprovalHistoryEntry
{
    public Guid ApproverId { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // Approved, Rejected, Requested Changes
    public string? Comment { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ChangeImpactAssessment
{
    public string Severity { get; set; } = "Low"; // Low, Medium, High
    public List<string>? AffectedSystems { get; set; }
    public string? RiskMitigation { get; set; }
}

#endregion

#region Attachment Attributes

/// <summary>
/// Attributes for Attachment entity
/// </summary>
public class AttachmentAttributes
{
    public long? FileSizeBytes { get; set; }
    public string? MimeType { get; set; }
    public string? UploadedByName { get; set; }
    public VirusScanResult? VirusScan { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? OcrText { get; set; }
}

public class VirusScanResult
{
    public bool IsClean { get; set; }
    public DateTime ScannedAt { get; set; }
    public string? ThreatName { get; set; }
}

#endregion

#region UserRole Attributes

/// <summary>
/// Attributes for UserRole entity
/// </summary>
public class UserRoleAttributes
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Department { get; set; }
    public UiPreferences? UiPreferences { get; set; }
    public NotificationSettings? NotificationSettings { get; set; }
}

public class UiPreferences
{
    public string Theme { get; set; } = "light"; // light, dark, auto
    public string Language { get; set; } = "en";
    public string Timezone { get; set; } = "UTC";
    public Dictionary<string, object>? DashboardConfig { get; set; }
}

public class NotificationSettings
{
    public bool EmailEnabled { get; set; } = true;
    public bool SmsEnabled { get; set; } = false;
    public List<string>? SubscribedEvents { get; set; }
}

#endregion

#region WorkflowState Attributes

/// <summary>
/// Attributes for WorkflowState entity
/// </summary>
public class WorkflowStateAttributes
{
    public int? DisplayOrder { get; set; }
    public string? ColorCode { get; set; }
    public string? IconName { get; set; }
    public List<string>? TransitionsAllowed { get; set; }
}

#endregion

#region DocumentRegistry Attributes

/// <summary>
/// Attributes for DocumentRegistry entity (JSONB storage)
/// Stores OCR-extracted data, audit trail, and scan results
/// </summary>
public class DocumentRegistryAttributes
{
    /// <summary>
    /// OCR/AI extracted data (varies by document type)
    /// Examples:
    /// - Trade License: { "licenseNumber": "CN-123", "issuingAuthority": "DED Dubai" }
    /// - Passport: { "passportNumber": "N123", "fullName": "John Doe", "nationality": "UAE" }
    /// </summary>
    public Dictionary<string, object>? ExtractedData { get; set; }
    
    /// <summary>
    /// Lightweight audit trail (append-only)
    /// Tracks who viewed/downloaded/verified the document
    /// </summary>
    public List<DocumentAuditEntry>? AuditLog { get; set; }
    
    /// <summary>
    /// OCR confidence score (0.0 - 1.0)
    /// </summary>
    public double? OcrConfidence { get; set; }
    
    /// <summary>
    /// Malware/virus scan result
    /// </summary>
    public ScanResult? ScanResult { get; set; }
    
    /// <summary>
    /// Additional metadata specific to document type
    /// </summary>
    public Dictionary<string,  string>? Metadata { get; set; }
}

/// <summary>
/// Single audit log entry
/// </summary>
public class DocumentAuditEntry
{
    public string Action { get; set; } = string.Empty; // "Viewed", "Downloaded", "Verified", "Rejected"
    public string By { get; set; } = string.Empty; // User email or system identifier
    public DateTime At { get; set; }
    public string? IpAddress { get; set; }
}

/// <summary>
/// Scan result from malware/virus scanner
/// </summary>
public class ScanResult
{
    public string Status { get; set; } = "pending"; // "pending", "clean", "infected", "error"
    public string? Engine { get; set; } // "Azure Defender", "ClamAV", "VirusTotal"
    public DateTime? ScannedAt { get; set; }
    public string? ThreatName { get; set; }
}

#endregion
