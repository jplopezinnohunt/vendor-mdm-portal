namespace VendorMdm.Shared.Constants;

/// <summary>
/// Status values for user entities.
/// Defines the state machine for user lifecycle.
/// </summary>
public static class UserStatus
{
    /// <summary>User has been invited but not yet activated</summary>
    public const string Pending = "Pending";

    /// <summary>User account is active and can log in</summary>
    public const string Active = "Active";

    /// <summary>User account is temporarily suspended</summary>
    public const string Suspended = "Suspended";

    /// <summary>User account is archived (soft deleted equivalent)</summary>
    public const string Archived = "Archived";

    /// <summary>User account is locked due to security concerns</summary>
    public const string Locked = "Locked";

    // ============================================
    // State Machine
    // ============================================

    private static readonly Dictionary<string, HashSet<string>> ValidTransitions = new()
    {
        [Pending] = new() { Active, Archived },
        [Active] = new() { Suspended, Locked, Archived },
        [Suspended] = new() { Active, Archived },
        [Locked] = new() { Active, Suspended, Archived },
        [Archived] = new() { } // Terminal
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

    public static readonly string[] All = { Pending, Active, Suspended, Archived, Locked };

    public static bool IsValid(string status) => All.Contains(status);

    public static bool CanLogin(string status) => status == Active;
}
