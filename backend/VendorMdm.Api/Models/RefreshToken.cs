namespace VendorMdm.Api.Models;

/// <summary>
/// RefreshToken entity for database storage.
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }
    public required string Token { get; set; }
    public Guid UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}
