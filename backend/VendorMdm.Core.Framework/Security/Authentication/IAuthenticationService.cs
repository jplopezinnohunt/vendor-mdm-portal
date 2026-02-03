using VendorMdm.Core.Framework.Primitives;

namespace VendorMdm.Core.Framework.Security.Authentication;

/// <summary>
/// Core authentication service for all MDM applications.
/// Provides multi-channel authentication (AzureAD, MagicLink, LocalStrong).
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user with the specified identifier and method.
    /// </summary>
    /// <param name="identifier">User identifier (email, username, phone)</param>
    /// <param name="method">Authentication method (AzureAD, MagicLink, LocalStrong)</param>
    /// <param name="credentials">Optional credentials (password, token, etc.)</param>
    /// <returns>Authentication token if successful</returns>
    Task<Result<AuthToken>> AuthenticateAsync(
        string identifier, 
        string method, 
        Dictionary<string, string>? credentials = null);

    /// <summary>
    /// Refreshes an expired authentication token.
    /// </summary>
    /// <param name="refreshToken">Refresh token</param>
    /// <returns>New authentication token if successful</returns>
    Task<Result<AuthToken>> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Revokes an authentication token (logout).
    /// </summary>
    /// <param name="token">Token to revoke</param>
    /// <returns>Success or failure</returns>
    Task<Result> RevokeTokenAsync(string token);

    /// <summary>
    /// Validates an authentication token.
    /// </summary>
    /// <param name="token">Token to validate</param>
    /// <returns>Token claims if valid</returns>
    Task<Result<TokenClaims>> ValidateTokenAsync(string token);

    /// <summary>
    /// Creates a new user account.
    /// </summary>
    /// <param name="identifier">User identifier (email, username)</param>
    /// <param name="initialRole">Initial role to assign</param>
    /// <param name="appContext">Application context (VendorMDM, EmployeeMDM, etc.)</param>
    /// <returns>User ID if successful</returns>
    Task<Result<Guid>> CreateAccountAsync(
        string identifier, 
        string initialRole, 
        string appContext);
}

/// <summary>
/// Authentication token with access and refresh tokens.
/// </summary>
public record AuthToken
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required string TokenType { get; init; } = "Bearer";
    public required Guid UserId { get; init; }
    public required string UserIdentifier { get; init; }
}

/// <summary>
/// Token claims extracted from a validated token.
/// </summary>
public record TokenClaims
{
    public required Guid UserId { get; init; }
    public required string UserIdentifier { get; init; }
    public required string[] Roles { get; init; }
    public required string AppContext { get; init; }
    public required DateTime IssuedAt { get; init; }
    public required DateTime ExpiresAt { get; init; }
}
