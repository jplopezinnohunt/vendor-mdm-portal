using System.Security.Claims;
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
            try
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
                        ?? context.User.FindFirst("email")?.Value
                        ?? context.User.FindFirst(ClaimTypes.Email)?.Value; // Added ClaimTypes.Email fallback

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GhostUserBlockingMiddleware");
                // Fail open or closed? Fail open for now to avoid blocking everyone on error, or strictly fail closed. 
                // Given the user is stuck, let's allow next but log.
                // Or return 500 with details.
                // Let's rethrow or better yet, return 500 but LOG IT so we see it.
                // Since this is middleware, if we don't call next(), it stops.
                // If we throw, the global exception handler (DeveloperExceptionPage) might show it.
                // But user sees "Network Error" (CORS?) or 500.
                
                // Let's attempt to proceed if DB check fails? Risk of security hole.
                // But for debugging, let's log and rethrow.
                throw; 
            }
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
