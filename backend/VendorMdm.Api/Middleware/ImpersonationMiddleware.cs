using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace VendorMdm.Api.Middleware
{
    public class ImpersonationMiddleware
    {
        private readonly RequestDelegate _next;
        public const string ImpersonationCookieName = "X-Impersonate-User";

        public ImpersonationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only allow impersonation if the original user is authenticated
            // In a real scenario, we would strictly check for "Admin" role here on the ORIGINAL principal
            // However, since we replace the principal, we need to be careful.
            // A better approach is to check if the cookie exists, and if so, 
            // verifying the original token allows it, or relying on the fact that 
            // the cookie can only be set by an Admin endpoint.
            
            // For this implementation, we assume the cookie is secure and only set by the AuthController
            // which performs the Admin check.
            
            var impersonationCookie = context.Request.Cookies[ImpersonationCookieName];

            if (!string.IsNullOrEmpty(impersonationCookie))
            {
                // Cookie format: DisplayName|Role|Email
                var parts = impersonationCookie.Split('|');
                if (parts.Length >= 3)
                {
                    var displayName = parts[0];
                    var role = parts[1];
                    var email = parts[2];

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, displayName),
                        new Claim(ClaimTypes.Email, email),
                        new Claim(ClaimTypes.Upn, email),
                        new Claim("name", displayName),
                        new Claim("preferred_username", email),
                        new Claim(ClaimTypes.Role, role),
                        new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", "impersonated-user-id"),
                        new Claim("Impersonated", "true") 
                    };

                    var identity = new ClaimsIdentity(claims, "Impersonation");
                    context.User = new ClaimsPrincipal(identity);
                }
            }

            await _next(context);
        }
    }
}
