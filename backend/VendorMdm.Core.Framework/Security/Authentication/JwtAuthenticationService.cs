using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using VendorMdm.Core.Framework.Primitives;
using VendorMdm.Core.Framework.Logging;

namespace VendorMdm.Core.Framework.Security.Authentication;

/// <summary>
/// JWT-based implementation of IAuthenticationService.
/// Supports multi-channel authentication (AzureAD, MagicLink, LocalStrong).
/// </summary>
public class JwtAuthenticationService : IAuthenticationService
{
    private readonly JwtAuthenticationOptions _options;
    private readonly IStructuredLogger _logger;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public JwtAuthenticationService(
        JwtAuthenticationOptions options,
        IStructuredLogger logger,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
    }

    public async Task<Result<AuthToken>> AuthenticateAsync(
        string identifier,
        string method,
        Dictionary<string, string>? credentials = null)
    {
        using var _ = _logger.BeginOperation("Authenticate", ("Identifier", identifier), ("Method", method));

        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(identifier))
                return Result.Fail<AuthToken>("Identifier is required");

            if (string.IsNullOrWhiteSpace(method))
                return Result.Fail<AuthToken>("Authentication method is required");

            // Get user by identifier
            var user = await _userRepository.GetByIdentifierAsync(identifier);
            if (user == null)
            {
                _logger.LogWarning("User not found", ("Identifier", identifier));
                return Result.Fail<AuthToken>("Invalid credentials");
            }

            // Check if user is blocked
            if (user.IsBlocked)
            {
                _logger.LogWarning("User is blocked", ("UserId", user.Id));
                return Result.Fail<AuthToken>("User account is blocked");
            }

            // Authenticate based on method
            var authResult = method.ToLower() switch
            {
                "azuread" => await AuthenticateAzureAdAsync(user, credentials),
                "magiclink" => await AuthenticateMagicLinkAsync(user, credentials),
                "localstrong" => await AuthenticateLocalStrongAsync(user, credentials),
                _ => Result.Fail($"Unsupported authentication method: {method}")
            };

            if (authResult.IsFailure)
                return Result.Fail<AuthToken>(authResult.Error);

            // Generate tokens
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            // Store refresh token
            await _refreshTokenRepository.StoreAsync(user.Id, refreshToken, DateTime.UtcNow.AddDays(30));

            _logger.LogInformation("User authenticated successfully", ("UserId", user.Id), ("Method", method));

            return Result.Ok(new AuthToken
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes),
                TokenType = "Bearer",
                UserId = user.Id,
                UserIdentifier = user.Email ?? user.Username
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication failed", ("Identifier", identifier));
            return Result.Fail<AuthToken>("Authentication failed");
        }
    }

    public async Task<Result<AuthToken>> RefreshTokenAsync(string refreshToken)
    {
        using var _ = _logger.BeginOperation("RefreshToken");

        try
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return Result.Fail<AuthToken>("Refresh token is required");

            // Validate refresh token
            var storedToken = await _refreshTokenRepository.GetAsync(refreshToken);
            if (storedToken == null)
            {
                _logger.LogWarning("Invalid refresh token");
                return Result.Fail<AuthToken>("Invalid refresh token");
            }

            if (storedToken.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token expired", ("ExpiresAt", storedToken.ExpiresAt));
                await _refreshTokenRepository.DeleteAsync(refreshToken);
                return Result.Fail<AuthToken>("Refresh token expired");
            }

            // Get user
            var user = await _userRepository.GetByIdAsync(storedToken.UserId);
            if (user == null || user.IsBlocked)
            {
                _logger.LogWarning("User not found or blocked", ("UserId", storedToken.UserId));
                return Result.Fail<AuthToken>("Invalid refresh token");
            }

            // Generate new tokens
            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken();

            // Delete old refresh token
            await _refreshTokenRepository.DeleteAsync(refreshToken);

            // Store new refresh token
            await _refreshTokenRepository.StoreAsync(user.Id, newRefreshToken, DateTime.UtcNow.AddDays(30));

            _logger.LogInformation("Token refreshed successfully", ("UserId", user.Id));

            return Result.Ok(new AuthToken
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes),
                TokenType = "Bearer",
                UserId = user.Id,
                UserIdentifier = user.Email ?? user.Username
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return Result.Fail<AuthToken>("Token refresh failed");
        }
    }

    public async Task<Result> RevokeTokenAsync(string token)
    {
        using var _ = _logger.BeginOperation("RevokeToken");

        try
        {
            if (string.IsNullOrWhiteSpace(token))
                return Result.Fail("Token is required");

            // Delete refresh token
            await _refreshTokenRepository.DeleteAsync(token);

            _logger.LogInformation("Token revoked successfully");
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token revocation failed");
            return Result.Fail("Token revocation failed");
        }
    }

    public async Task<Result<TokenClaims>> ValidateTokenAsync(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                return Result.Fail<TokenClaims>("Token is required");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_options.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _options.Issuer,
                ValidateAudience = true,
                ValidAudience = _options.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            var userId = Guid.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("UserId claim not found"));
            var userIdentifier = principal.FindFirst(ClaimTypes.Email)?.Value ?? principal.FindFirst(ClaimTypes.Name)?.Value ?? "";
            var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
            var appContext = principal.FindFirst("AppContext")?.Value ?? "";
            var issuedAt = DateTime.Parse(principal.FindFirst("iat")?.Value ?? DateTime.UtcNow.ToString());
            var expiresAt = DateTime.Parse(principal.FindFirst("exp")?.Value ?? DateTime.UtcNow.ToString());

            return Result.Ok(new TokenClaims
            {
                UserId = userId,
                UserIdentifier = userIdentifier,
                Roles = roles,
                AppContext = appContext,
                IssuedAt = issuedAt,
                ExpiresAt = expiresAt
            });
        }
        catch (SecurityTokenExpiredException)
        {
            return Result.Fail<TokenClaims>("Token expired");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation failed");
            return Result.Fail<TokenClaims>("Invalid token");
        }
    }

    public async Task<Result<Guid>> CreateAccountAsync(string identifier, string initialRole, string appContext)
    {
        using var _ = _logger.BeginOperation("CreateAccount", ("Identifier", identifier), ("Role", initialRole));

        try
        {
            // Check if user already exists
            var existingUser = await _userRepository.GetByIdentifierAsync(identifier);
            if (existingUser != null)
            {
                _logger.LogWarning("User already exists", ("Identifier", identifier));
                return Result.Fail<Guid>("User already exists");
            }

            // Create user
            var userId = Guid.NewGuid();
            await _userRepository.CreateAsync(userId, identifier, initialRole, appContext);

            _logger.LogInformation("Account created successfully", ("UserId", userId));
            return Result.Ok(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Account creation failed", ("Identifier", identifier));
            return Result.Fail<Guid>("Account creation failed");
        }
    }

    // Private helper methods

    private async Task<Result> AuthenticateAzureAdAsync(UserEntity user, Dictionary<string, string>? credentials)
    {
        // Azure AD authentication logic
        // In production, validate Azure AD token
        if (credentials == null || !credentials.ContainsKey("azureAdToken"))
            return Result.Fail("Azure AD token is required");

        // TODO: Validate Azure AD token with Microsoft Graph API
        // For now, accept any token (will implement in Week 2)

        return Result.Ok();
    }

    private async Task<Result> AuthenticateMagicLinkAsync(UserEntity user, Dictionary<string, string>? credentials)
    {
        // Magic link authentication logic
        if (credentials == null || !credentials.ContainsKey("magicLinkToken"))
            return Result.Fail("Magic link token is required");

        // Validate magic link token
        var token = credentials["magicLinkToken"];
        var isValid = await _userRepository.ValidateMagicLinkAsync(user.Id, token);

        if (!isValid)
            return Result.Fail("Invalid magic link token");

        return Result.Ok();
    }

    private async Task<Result> AuthenticateLocalStrongAsync(UserEntity user, Dictionary<string, string>? credentials)
    {
        // Local strong password authentication
        if (credentials == null || !credentials.ContainsKey("password"))
            return Result.Fail("Password is required");

        var password = credentials["password"];
        var isValid = await _userRepository.ValidatePasswordAsync(user.Id, password);

        if (!isValid)
            return Result.Fail("Invalid password");

        return Result.Ok();
    }

    private string GenerateAccessToken(UserEntity user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_options.SecretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("AppContext", user.AppContext ?? "")
        };

        // Add roles
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}

/// <summary>
/// Configuration options for JWT authentication.
/// </summary>
public class JwtAuthenticationOptions
{
    public required string SecretKey { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 30;
}

/// <summary>
/// User entity for authentication.
/// </summary>
public class UserEntity
{
    public required Guid Id { get; set; }
    public required string Username { get; set; }
    public string? Email { get; set; }
    public required bool IsBlocked { get; set; }
    public required string[] Roles { get; set; }
    public string? AppContext { get; set; }
}

/// <summary>
/// Stored refresh token.
/// </summary>
public class StoredRefreshToken
{
    public required Guid UserId { get; set; }
    public required string Token { get; set; }
    public required DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Repository for user operations.
/// Apps must implement this interface.
/// </summary>
public interface IUserRepository
{
    Task<UserEntity?> GetByIdentifierAsync(string identifier);
    Task<UserEntity?> GetByIdAsync(Guid userId);
    Task CreateAsync(Guid userId, string identifier, string initialRole, string appContext);
    Task<bool> ValidateMagicLinkAsync(Guid userId, string token);
    Task<bool> ValidatePasswordAsync(Guid userId, string password);
}

/// <summary>
/// Repository for refresh token operations.
/// Apps must implement this interface.
/// </summary>
public interface IRefreshTokenRepository
{
    Task StoreAsync(Guid userId, string token, DateTime expiresAt);
    Task<StoredRefreshToken?> GetAsync(string token);
    Task DeleteAsync(string token);
}
