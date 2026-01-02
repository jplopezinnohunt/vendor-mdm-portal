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
