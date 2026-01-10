using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VendorMdm.Api.Middleware;

namespace VendorMdm.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            // Default to anonymous/mock if not authenticated
            if (User.Identity?.IsAuthenticated != true)
            {
                 // In development, return a mock profile if no auth
                 // But for this task, we want to enforce real auth checks or return specific status
                 return Unauthorized(new { message = "Not authenticated" });
            }

            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var name = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("name")?.Value ?? "Unknown User";
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst(ClaimTypes.Upn)?.Value ?? User.FindFirst("preferred_username")?.Value;
            var isImpersonated = User.HasClaim(c => c.Type == "Impersonated");

            return Ok(new
            {
                UserId = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value ?? "mock-id",
                DisplayName = name,
                Email = email,
                Roles = roles,
                IsImpersonated = isImpersonated
            });
        }

        [HttpPost("impersonate")]
        [Authorize(Roles = "Admin")] // Strict check: Only actual Admins can call this
        public IActionResult Impersonate([FromBody] ImpersonationRequest request)
        {
            if (string.IsNullOrEmpty(request.Role))
            {
                return BadRequest("Role is required");
            }

            // Simple format: DisplayName|Role|Email
            var value = $"{request.DisplayName ?? "Impersonated User"}|{request.Role}|{request.Email ?? "impersonated@example.com"}";
            
            Response.Cookies.Append(ImpersonationMiddleware.ImpersonationCookieName, value, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Ensure we are using HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = System.DateTime.Now.AddHours(1)
            });

            return Ok(new { message = $"Impersonating {request.Role}" });
        }

        [HttpPost("stop-impersonation")]
        public IActionResult StopImpersonation()
        {
            Response.Cookies.Delete(ImpersonationMiddleware.ImpersonationCookieName);
            return Ok(new { message = "Impersonation stopped" });
        }
    }

    public class ImpersonationRequest
    {
        public string Role { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
    }
}
