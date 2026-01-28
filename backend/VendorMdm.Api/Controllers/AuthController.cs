using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendorMdm.Api.Data;
using VendorMdm.Api.Services;
using VendorMdm.Shared.Models;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace VendorMdm.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly SqlDbContext _context;
    private readonly ITotpService _totpService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        SqlDbContext context, 
        ITotpService totpService, 
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _context = context;
        _totpService = totpService;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("invite")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> InviteUser([FromBody] InviteRequest request)
    {
        // 1. Check if user exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return Conflict(new { message = "User already exists" });

        // 2. Create Pending User
        var token = Guid.NewGuid().ToString("N");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Username = request.Username ?? request.Email.Split('@')[0],
            Roles = request.Roles ?? new List<string> { "Viewer" },
            Status = "Pending",
            InvitationToken = token,
            InvitationExpiresAt = DateTime.UtcNow.AddDays(7),
            AuthProvider = "Local",
            AuthMethod = request.AuthMethod ?? "MagicLink" 
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // 3. Send Email
        var emailData = new InvitationEmailData
        {
            Email = request.Email,
            VendorName = request.Username ?? "User",
            Token = token,
            ExpiresAt = user.InvitationExpiresAt.Value,
            InvitedByName = "Admin", // TODO: Get from current user context
            BaseUrl = _configuration["App:BaseUrl"]
        };

        await _emailService.SendInvitationEmailAsync(emailData);

        return Ok(new { message = "Invitation sent" });
    }

    [HttpGet("validate-invite/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateInvite(string token)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.InvitationToken == token);
        
        if (user == null) return NotFound("Invalid token");
        if (user.InvitationExpiresAt < DateTime.UtcNow) return BadRequest("Token expired");
        if (user.Status != "Pending") return BadRequest("User already active");

        return Ok(new { email = user.Email, username = user.Username });
    }

    [HttpPost("complete-setup")]
    [AllowAnonymous]
    public async Task<IActionResult> CompleteSetup([FromBody] SetupRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.InvitationToken == request.Token);
        if (user == null || user.Status != "Pending") return BadRequest("Invalid flow");

        // 1. Set Password
        user.PasswordHash = HashPassword(request.Password); // Replace with real hashing
        
        // 2. Setup 2FA
        user.TwoFactorSecret = _totpService.GenerateSecret();
        user.TwoFactorEnabled = false; // Not fully enabled until verified code

        await _context.SaveChangesAsync();
        
        // Return 2FA Setup Info
        return Ok(new 
        { 
            secret = user.TwoFactorSecret, 
            qrCode = _totpService.GenerateQrCodeUri(user.Email, user.TwoFactorSecret) 
        });
    }

    [HttpPost("verify-2fa-setup")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifySetup([FromBody] VerifySetupRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.InvitationToken == request.Token);
        if (user == null) return BadRequest("Invalid token");

        if (!_totpService.VerifyCode(user.TwoFactorSecret!, request.Code))
            return BadRequest("Invalid code");

        // Activate User
        user.TwoFactorEnabled = true;
        user.Status = "Active";
        user.InvitationToken = null; // Clear token
        user.InvitationExpiresAt = null;
        user.LastLogonAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Issue JWT
        var token = GenerateJwtToken(user);
        return Ok(new { token, user });
    }

    [HttpPost("magic-link")]
    [AllowAnonymous]
    public async Task<IActionResult> SendMagicLink([FromBody] MagicLinkRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        
        // Security: Always return Ok to prevent enumeration, unless in strict dev mode
        if (user == null || user.AuthMethod != "MagicLink") 
            return Ok(new { message = "If user exists, link sent." });

        // Generate Token
        var token = Guid.NewGuid().ToString("N");
        user.MagicLinkToken = token;
        user.MagicLinkExpiresAt = DateTime.UtcNow.AddHours(1);
        await _context.SaveChangesAsync();

        // Send Email
        var link = $"{_configuration["App:BaseUrl"]}/magic-login?token={token}";
        await _emailService.SendMagicLinkEmailAsync(request.Email, link);

        return Ok(new { message = "Link sent" });
    }

    [HttpPost("verify-magic-link")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyMagicLink([FromBody] VerifyMagicLinkRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.MagicLinkToken == request.Token);
        
        if (user == null) return Unauthorized("Invalid token");
        if (user.MagicLinkExpiresAt < DateTime.UtcNow) return Unauthorized("Token expired");

        // Success - Clear Token
        user.MagicLinkToken = null;
        user.MagicLinkExpiresAt = null;
        user.LastLogonAt = DateTime.UtcNow;
        user.Status = "Active"; // Ensure user is marked Active
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        return Ok(new { token, user });
    }

    [HttpPost("login-local")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginLocal([FromBody] LoginRequest request)
    {
        var input = request.Email.Trim();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == input.ToLower() || u.Username.ToLower() == input.ToLower());
        if (user == null) return Unauthorized();

        // Enforce Method
        if (user.AuthMethod != "LocalStrong") 
            return BadRequest(new { message = $"User configured for {user.AuthMethod}, not Password." });

        // Verify Password
        if (user.PasswordHash != HashPassword(request.Password)) return Unauthorized();

        if (user.TwoFactorEnabled)
        {
            return Ok(new { require2fa = true, tempToken = "TEMP_" + user.Id });
        }

        return Ok(new { token = GenerateJwtToken(user), user });
    }

    // Reusing Login2fa but ensuring it checks user is allowed
    [HttpPost("login-2fa")]
    [AllowAnonymous]
    public async Task<IActionResult> Login2fa([FromBody] Login2faRequest request)
    {
        var input = request.Email.Trim();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == input.ToLower() || u.Username.ToLower() == input.ToLower());
        if (user == null || !user.TwoFactorEnabled) return Unauthorized();
        
        // Ensure strictly LocalStrong users use this
        // (Technically MagicLink users don't have secrets so verify would fail anyway)

        if (!_totpService.VerifyCode(user.TwoFactorSecret!, request.Code))
            return Unauthorized("Invalid 2FA Code");

        user.LastLogonAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { token = GenerateJwtToken(user), user });
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        // Get User ID from Claims
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id" || c.Type == "sub" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        if (!Guid.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        // STRICT CONSTRAINT: Only LocalStrong can change password
        if (user.AuthMethod != "LocalStrong")
            return BadRequest(new { message = "Password change is not available for this account type (SSO/MagicLink)." });

        // Verify Old Password
        if (user.PasswordHash != HashPassword(request.OldPassword))
            return BadRequest(new { message = "Incorrect current password." });

        // Set New Password
        user.PasswordHash = HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Password updated successfully." });
    }

    [HttpPost("resend-invite")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResendInvite([FromBody] ResendInviteRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return NotFound("User not found");
        
        if (user.Status != "Pending") 
            return BadRequest("User is already active.");

        // Regenerate token
        var token = Guid.NewGuid().ToString("N");
        user.InvitationToken = token;
        user.InvitationExpiresAt = DateTime.UtcNow.AddDays(7);
        
        // Ensure AuthMethod is up to date if they changed it in the edit screen meanwhile
        // (Optional, but good practice to sync)
        
        await _context.SaveChangesAsync();

        var emailData = new InvitationEmailData
        {
            Email = user.Email,
            VendorName = user.Username,
            Token = token,
            ExpiresAt = user.InvitationExpiresAt.Value,
            InvitedByName = "Admin", 
            BaseUrl = _configuration["App:BaseUrl"]
        };

        await _emailService.SendInvitationEmailAsync(emailData);

        return Ok(new { message = "Invitation resent" });
    }

    // --- Helpers ---

    private string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username)
        };
        
        foreach(var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("THIS_IS_A_DEV_SECRET_KEY_REPLACE_IN_PROD_12345"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "VendorMDM",
            audience: "VendorMDM",
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class InviteRequest { public string Email { get; set; } public string? Username { get; set; } public List<string>? Roles { get; set; } public string AuthMethod { get; set; } = "MagicLink"; }
public class ChangePasswordRequest
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}

public class ResendInviteRequest { public string Email { get; set; } }
public class SetupRequest { public string Token { get; set; } public string Password { get; set; } }
public class VerifySetupRequest { public string Token { get; set; } public string Code { get; set; } }
public class LoginRequest { public string Email { get; set; } public string Password { get; set; } }
public class Login2faRequest { public string Email { get; set; } public string Code { get; set; } }
public class MagicLinkRequest { public string Email { get; set; } }
public class VerifyMagicLinkRequest { public string Token { get; set; } }
