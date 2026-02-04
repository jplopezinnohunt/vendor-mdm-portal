namespace VendorMdm.Shared.Constants;

/// <summary>
/// Standard audit action types for the audit log system.
/// Used in AuditLog.Action field for consistent tracking.
/// </summary>
public static class AuditActions
{
    // ============================================
    // CRUD Operations
    // ============================================

    /// <summary>Entity was created</summary>
    public const string Created = "Created";

    /// <summary>Entity was updated</summary>
    public const string Updated = "Updated";

    /// <summary>Entity was soft deleted (Pattern 11)</summary>
    public const string SoftDeleted = "SoftDeleted";

    /// <summary>Entity was restored from soft delete</summary>
    public const string Restored = "Restored";

    // ============================================
    // Workflow Actions
    // ============================================

    /// <summary>Entity was submitted for approval</summary>
    public const string Submitted = "Submitted";

    /// <summary>Entity was approved</summary>
    public const string Approved = "Approved";

    /// <summary>Entity was rejected</summary>
    public const string Rejected = "Rejected";

    /// <summary>Entity was integrated to external system (e.g., SAP)</summary>
    public const string Integrated = "Integrated";

    // ============================================
    // Security Actions
    // ============================================

    /// <summary>User logged in</summary>
    public const string Login = "Login";

    /// <summary>User logged out</summary>
    public const string Logout = "Logout";

    /// <summary>User was blocked</summary>
    public const string Blocked = "Blocked";

    /// <summary>User was unblocked</summary>
    public const string Unblocked = "Unblocked";

    /// <summary>Password was changed</summary>
    public const string PasswordChanged = "PasswordChanged";

    /// <summary>Two-factor authentication was enabled</summary>
    public const string TwoFactorEnabled = "TwoFactorEnabled";

    /// <summary>Two-factor authentication was disabled</summary>
    public const string TwoFactorDisabled = "TwoFactorDisabled";

    // ============================================
    // Data Access Actions
    // ============================================

    /// <summary>Entity was viewed (for sensitive data tracking)</summary>
    public const string Viewed = "Viewed";

    /// <summary>Entity was exported (GDPR compliance)</summary>
    public const string Exported = "Exported";

    /// <summary>Entity was anonymized (GDPR right to be forgotten)</summary>
    public const string Anonymized = "Anonymized";

    // ============================================
    // Validation
    // ============================================

    public static readonly string[] All =
    {
        Created, Updated, SoftDeleted, Restored,
        Submitted, Approved, Rejected, Integrated,
        Login, Logout, Blocked, Unblocked, PasswordChanged,
        TwoFactorEnabled, TwoFactorDisabled,
        Viewed, Exported, Anonymized
    };

    public static bool IsValid(string action) => All.Contains(action);
}
