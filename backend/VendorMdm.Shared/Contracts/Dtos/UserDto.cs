namespace VendorMdm.Shared.Contracts.Dtos;

/// <summary>
/// Represents the strict API contract for a User.
/// Prevents leaking internal SQL Entities, passwords, or sensitive fields.
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public string AuthProvider { get; set; } = string.Empty;
    public string AuthMethod { get; set; } = string.Empty;
    public bool TwoFactorEnabled { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime? LastLogonAt { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Minimal user info for display in lists or references
/// </summary>
public class UserSummaryDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

public class CreateUserRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new() { "Viewer" };
    public string AuthMethod { get; set; } = "MagicLink";
}

public class UpdateUserRequestDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}
