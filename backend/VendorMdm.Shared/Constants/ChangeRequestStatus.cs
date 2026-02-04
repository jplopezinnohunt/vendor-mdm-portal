namespace VendorMdm.Shared.Constants;

/// <summary>
/// Status values for change request entities.
/// Defines the state machine for change request lifecycle.
/// Compliant with: moderngoldenrules.md Section 11 (EDA) + state-machine-standard.md
/// </summary>
public static class ChangeRequestStatus
{
    // ============================================
    // Initial States
    // ============================================

    /// <summary>Change request is being drafted, not yet submitted</summary>
    public const string Draft = "Draft";

    /// <summary>Change request has been submitted for review</summary>
    public const string Submitted = "Submitted";

    // ============================================
    // Review States
    // ============================================

    /// <summary>Change request is being actively reviewed</summary>
    public const string UnderReview = "UnderReview";

    /// <summary>Additional information requested from requestor</summary>
    public const string InformationRequested = "InformationRequested";

    // ============================================
    // Terminal States
    // ============================================

    /// <summary>Change request has been approved</summary>
    public const string Approved = "Approved";

    /// <summary>Change request has been rejected</summary>
    public const string Rejected = "Rejected";

    /// <summary>Change request was cancelled by requestor</summary>
    public const string Cancelled = "Cancelled";

    /// <summary>Change request has expired without action</summary>
    public const string Expired = "Expired";

    // ============================================
    // State Machine
    // ============================================

    private static readonly Dictionary<string, HashSet<string>> ValidTransitions = new()
    {
        [Draft] = new() { Submitted, Cancelled },
        [Submitted] = new() { UnderReview, Approved, Rejected, Cancelled },
        [UnderReview] = new() { InformationRequested, Approved, Rejected },
        [InformationRequested] = new() { Submitted, Cancelled, Expired },
        [Approved] = new() { }, // Terminal
        [Rejected] = new() { Draft }, // Allow re-submission from rejection
        [Cancelled] = new() { }, // Terminal
        [Expired] = new() { } // Terminal
    };

    /// <summary>
    /// Validates if a status transition is allowed by the state machine.
    /// </summary>
    /// <param name="from">Current status</param>
    /// <param name="to">Target status</param>
    /// <returns>True if transition is valid</returns>
    public static bool IsValidTransition(string from, string to)
    {
        return ValidTransitions.ContainsKey(from) &&
               ValidTransitions[from].Contains(to);
    }

    /// <summary>
    /// Gets all allowed transitions from the current status.
    /// </summary>
    /// <param name="currentStatus">Current status</param>
    /// <returns>Array of valid target statuses</returns>
    public static string[] GetAllowedTransitions(string currentStatus)
    {
        return ValidTransitions.TryGetValue(currentStatus, out var transitions)
            ? transitions.ToArray()
            : Array.Empty<string>();
    }

    /// <summary>
    /// All valid status values.
    /// </summary>
    public static readonly string[] All =
    {
        Draft, Submitted, UnderReview, InformationRequested,
        Approved, Rejected, Cancelled, Expired
    };

    /// <summary>
    /// Checks if a status value is valid.
    /// </summary>
    public static bool IsValid(string status) => All.Contains(status);

    /// <summary>
    /// Checks if a status is a terminal state (no further transitions allowed).
    /// </summary>
    public static bool IsTerminal(string status) =>
        status is Approved or Cancelled or Expired;

    /// <summary>
    /// Checks if the change request can be approved from current status.
    /// </summary>
    public static bool CanApprove(string status) =>
        status is Submitted or UnderReview;

    /// <summary>
    /// Checks if the change request can be rejected from current status.
    /// </summary>
    public static bool CanReject(string status) =>
        status is Submitted or UnderReview;

    /// <summary>
    /// Checks if the change request is pending action (not terminal).
    /// </summary>
    public static bool IsPending(string status) =>
        status is Draft or Submitted or UnderReview or InformationRequested;
}
