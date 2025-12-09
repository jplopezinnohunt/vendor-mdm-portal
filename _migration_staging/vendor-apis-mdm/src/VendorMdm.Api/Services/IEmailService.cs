namespace VendorMdm.Api.Services;

/// <summary>
/// Service for sending invitation emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an invitation email to a vendor
    /// </summary>
    Task<bool> SendInvitationEmailAsync(InvitationEmailData data);
}

/// <summary>
/// Data model for invitation email
/// </summary>
public class InvitationEmailData
{
    public string InvitationId { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string InvitedByName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Notes { get; set; }
    public string? BaseUrl { get; set; }
}

