using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendorMdm.Api.Data;
using Microsoft.AspNetCore.RateLimiting;
using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthDiscoveryController : ControllerBase
{
    private readonly SqlDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthDiscoveryController(SqlDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpGet("discover")] // Changed from Post("lookup") to Get("discover")
    [AllowAnonymous]
    [EnableRateLimiting("anonymous")] // Added EnableRateLimiting attribute

    public async Task<IActionResult> LookupUser([FromBody] LookupRequest request)
    {
        var input = request.Email.Trim();

        // 1. Check Domain Rules (if any hardcoded)
        if (input.EndsWith("@unesco.org", StringComparison.OrdinalIgnoreCase)) 
        {
            return Ok(new { authMethod = "AzureAd" });
        }

        // 2. Lookup in DB (Case Insensitive)
        // Note: EF Core translates .ToLower() to SQL lower() usually.
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == input.ToLower() || u.Username.ToLower() == input.ToLower());

        if (user == null)
        {
            // Security: Don't reveal user existence? 
            // For B2B portals, user usually knows they should have access.
            // But standard practice is to maybe default to MagicLink (and send nothing) or return generic.
            // In this specific internal/vendor case, we'll default to MagicLink flow (which will fail silently on send if user missing)
            // OR return "MagicLink" so UI shows "Check your email".
            return Ok(new { authMethod = "MagicLink" });
        }

        return Ok(new { authMethod = user.AuthMethod });
    }
}

public class LookupRequest
{
    public string Email { get; set; }
}
