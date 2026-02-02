using VendorMdm.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace VendorMdm.Api.Middleware
{
    /// <summary>
    /// Blocks "ghost users" - users authenticated via Azure AD but not in the database.
    /// Only active in Production to prevent unauthorized access.
    /// </summary>
    public class GhostUserBlockingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GhostUserBlockingMiddleware> _logger;
        private readonly bool _isProduction;

        public GhostUserBlockingMiddleware(
            RequestDelegate next,
            ILogger<GhostUserBlockingMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _isProduction = env.IsProduction();
        }

        public async Task InvokeAsync(HttpContext context, SqlDbContext dbContext)
        {
            // Only enforce in Production
            if (!_isProduction)
            {
                await _next(context);
                return;
            }

            // Check if user is authenticated
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userEmail = context.User.Identity.Name 
                    ?? context.User.FindFirst("preferred_username")?.Value
                    ?? context.User.FindFirst("email")?.Value;

                if (!string.IsNullOrEmpty(userEmail))
                {
                    // Check if user exists in database
                    var userExists = await dbContext.Users
                        .AnyAsync(u => u.Email == userEmail);

                    if (!userExists)
                    {
                        _logger.LogWarning(
                            "[SECURITY] Ghost user blocked: {Email} authenticated via Azure AD but not in database",
                            userEmail);

                        context.Response.StatusCode = 403; // Forbidden
                        await context.Response.WriteAsJsonAsync(new
                        {
                            error = "Access Denied",
                            message = "Your account is not authorized to access this system. Please contact your administrator."
                        });
                        return;
                    }
                }
            }

            await _next(context);
        }
    }

    public static class GhostUserBlockingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGhostUserBlocking(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GhostUserBlockingMiddleware>();
        }
    }
}
