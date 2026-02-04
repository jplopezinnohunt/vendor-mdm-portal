namespace VendorMdm.Shared.Constants;

/// <summary>
/// Status values for vendor applications and similar workflow entities.
/// Defines the state machine for application lifecycle.
/// </summary>
public static class ApplicationStatus
{
    // ============================================
    // Initial States
    // ============================================

    /// <summary>Application is being drafted, not yet submitted</summary>
    public const string Draft = "Draft";

    /// <summary>Application has been submitted for review</summary>
    public const string Submitted = "Submitted";

    // ============================================
    // Review States
    // ============================================

    /// <summary>Application is pending initial review</summary>
    public const string PendingReview = "PendingReview";

    /// <summary>Application is actively being reviewed</summary>
    public const string UnderReview = "UnderReview";

    /// <summary>Additional information requested from applicant</summary>
    public const string InformationRequested = "InformationRequested";

    // ============================================
    // Terminal States
    // ============================================

    /// <summary>Application has been approved</summary>
    public const string Approved = "Approved";

    /// <summary>Application has been rejected</summary>
    public const string Rejected = "Rejected";

    /// <summary>Application workflow is complete</summary>
    public const string Completed = "Completed";

    /// <summary>Application was cancelled by applicant</summary>
    public const string Cancelled = "Cancelled";

    /// <summary>Application has expired</summary>
    public const string Expired = "Expired";

    // ============================================
    // State Machine
    // ============================================

    private static readonly Dictionary<string, HashSet<string>> ValidTransitions = new()
    {
        [Draft] = new() { Submitted, Cancelled },
        [Submitted] = new() { PendingReview, Cancelled },
        [PendingReview] = new() { UnderReview, InformationRequested, Approved, Rejected },
        [UnderReview] = new() { InformationRequested, Approved, Rejected },
        [InformationRequested] = new() { Submitted, Cancelled, Expired },
        [Approved] = new() { Completed },
        [Rejected] = new() { }, // Terminal
        [Completed] = new() { }, // Terminal
        [Cancelled] = new() { }, // Terminal
        [Expired] = new() { } // Terminal
    };

    public static bool IsValidTransition(string from, string to)
    {
        return ValidTransitions.ContainsKey(from) &&
               ValidTransitions[from].Contains(to);
    }

    public static string[] GetAllowedTransitions(string currentStatus)
    {
        return ValidTransitions.TryGetValue(currentStatus, out var transitions)
            ? transitions.ToArray()
            : Array.Empty<string>();
    }

    public static readonly string[] All =
    {
        Draft, Submitted, PendingReview, UnderReview, InformationRequested,
        Approved, Rejected, Completed, Cancelled, Expired
    };

    public static bool IsValid(string status) => All.Contains(status);

    public static bool IsTerminal(string status) =>
        status is Approved or Rejected or Completed or Cancelled or Expired;
}
