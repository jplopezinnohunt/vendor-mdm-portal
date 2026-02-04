namespace VendorMdm.Shared.Constants;

/// <summary>
/// Status values for GDPR-related entity states.
/// Used when processing data subject requests.
/// </summary>
public static class GdprStatus
{
    /// <summary>Entity has been marked for deletion (soft delete)</summary>
    public const string Deleted = "Deleted";

    /// <summary>Processing of entity data has been restricted (Article 18)</summary>
    public const string ProcessingRestricted = "ProcessingRestricted";

    /// <summary>Data subject has filed an objection (Article 21)</summary>
    public const string ObjectionPending = "ObjectionPending";

    /// <summary>Entity data has been anonymized (right to be forgotten)</summary>
    public const string Anonymized = "Anonymized";

    /// <summary>Export request is pending</summary>
    public const string ExportPending = "ExportPending";

    /// <summary>Data has been exported</summary>
    public const string Exported = "Exported";

    /// <summary>Rectification request is pending</summary>
    public const string RectificationPending = "RectificationPending";

    /// <summary>Data has been rectified</summary>
    public const string Rectified = "Rectified";

    public static readonly string[] All =
    {
        Deleted, ProcessingRestricted, ObjectionPending, Anonymized,
        ExportPending, Exported, RectificationPending, Rectified
    };

    public static bool IsValid(string status) => All.Contains(status);

    /// <summary>
    /// Check if the entity is in a state where data access should be restricted
    /// </summary>
    public static bool IsRestricted(string status) =>
        status is ProcessingRestricted or ObjectionPending or Deleted or Anonymized;
}
