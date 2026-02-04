namespace VendorMdm.Api.Middleware;

/// <summary>
/// Middleware that adds API versioning headers to all responses.
/// Prepares the system for future API versioning without breaking changes.
///
/// Headers added:
/// - X-API-Version: Current API version
/// - X-API-Deprecation: Deprecation notice (empty if not deprecated)
/// - X-API-Sunset: Sunset date for deprecated endpoints
/// </summary>
public class ApiVersionHeaderMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiVersionHeaderMiddleware> _logger;

    /// <summary>
    /// Current API version. Update when releasing new versions.
    /// </summary>
    public const string CurrentVersion = "1.0";

    public ApiVersionHeaderMiddleware(RequestDelegate next, ILogger<ApiVersionHeaderMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add version headers before response starts
        context.Response.OnStarting(() =>
        {
            // Always add current version
            context.Response.Headers["X-API-Version"] = CurrentVersion;

            // Add deprecation header (empty = not deprecated)
            // Future: populate from endpoint metadata or configuration
            if (!context.Response.Headers.ContainsKey("X-API-Deprecation"))
            {
                context.Response.Headers["X-API-Deprecation"] = string.Empty;
            }

            return Task.CompletedTask;
        });

        await _next(context);
    }
}

/// <summary>
/// Extension methods for registering the API version middleware.
/// </summary>
public static class ApiVersionHeaderMiddlewareExtensions
{
    /// <summary>
    /// Adds API version headers to all responses.
    /// Should be called early in the pipeline.
    /// </summary>
    public static IApplicationBuilder UseApiVersionHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApiVersionHeaderMiddleware>();
    }
}
