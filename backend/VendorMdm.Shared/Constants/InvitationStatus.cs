namespace VendorMdm.Shared.Constants;

/// <summary>
/// Status values for invitation entities.
/// Defines the state machine for invitation lifecycle.
/// </summary>
public static class InvitationStatus
{
    /// <summary>Invitation has been sent and is awaiting response</summary>
    public const string Pending = "Pending";

    /// <summary>Invitation has been accepted by recipient</summary>
    public const string Accepted = "Accepted";

    /// <summary>Invitation has expired without response</summary>
    public const string Expired = "Expired";

    /// <summary>Invitation workflow is complete (vendor created)</summary>
    public const string Completed = "Completed";

    /// <summary>Invitation was cancelled by sender</summary>
    public const string Cancelled = "Cancelled";

    /// <summary>Invitation was resent (new token generated)</summary>
    public const string Resent = "Resent";

    // ============================================
    // State Machine
    // ============================================

    private static readonly Dictionary<string, HashSet<string>> ValidTransitions = new()
    {
        [Pending] = new() { Accepted, Expired, Cancelled, Resent },
        [Accepted] = new() { Completed, Cancelled },
        [Expired] = new() { Pending, Resent }, // Allow resend
        [Completed] = new() { }, // Terminal
        [Cancelled] = new() { }, // Terminal
        [Resent] = new() { Accepted, Expired, Cancelled }
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
        Pending, Accepted, Expired, Completed, Cancelled, Resent
    };

    public static bool IsValid(string status) => All.Contains(status);

    public static bool IsTerminal(string status) =>
        status is Completed or Cancelled;

    public static bool CanAccept(string status) =>
        status is Pending or Resent;
}
