using System.Security.Cryptography;

namespace VendorMdm.Api.Middleware;

/// <summary>
/// Middleware that injects security headers on every HTTP response.
/// Implements Section 7.B (Network & Transport) from moderngoldenrules.md.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;

    public SecurityHeadersMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<SecurityHeadersMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
        var isProduction = environment.Equals("Production", StringComparison.OrdinalIgnoreCase);

        // 1. HSTS (Strict-Transport-Security) - Only for HTTPS in Production
        if (context.Request.IsHttps || isProduction)
        {
            var hstsMaxAge = _configuration["Security:HSTS:MaxAge"] ?? "31536000"; // 1 year default
            var includeSubDomains = _configuration.GetValue("Security:HSTS:IncludeSubDomains", true);
            var preload = _configuration.GetValue("Security:HSTS:Preload", true);

            var hstsValue = $"max-age={hstsMaxAge}";
            if (includeSubDomains) hstsValue += "; includeSubDomains";
            if (preload) hstsValue += "; preload";

            context.Response.Headers["Strict-Transport-Security"] = hstsValue;
        }

        // 2. CSP (Content-Security-Policy)
        var cspNonce = GenerateNonce();
        context.Items["csp-nonce"] = cspNonce;

        var cspPolicy = _configuration["Security:CSP:Policy"]
            ?? "default-src 'self'; script-src 'self' 'nonce-{nonce}'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self'; connect-src 'self'; frame-ancestors 'none';";

        cspPolicy = cspPolicy.Replace("{nonce}", cspNonce);
        context.Response.Headers["Content-Security-Policy"] = cspPolicy;

        // 3. X-Frame-Options (Clickjacking protection)
        context.Response.Headers["X-Frame-Options"] = "DENY";

        // 4. X-Content-Type-Options (MIME sniffing protection)
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";

        // 5. Referrer-Policy (Privacy protection)
        var referrerPolicy = _configuration["Security:ReferrerPolicy"] ?? "no-referrer";
        context.Response.Headers["Referrer-Policy"] = referrerPolicy;

        // 6. X-XSS-Protection (Legacy but still useful)
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

        // 7. Permissions-Policy (Feature policy)
        context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

        _logger.LogDebug(
            "Security headers injected for {Method} {Path} (Environment: {Environment})",
            context.Request.Method,
            context.Request.Path,
            environment);

        await _next(context);
    }

    private static string GenerateNonce()
    {
        var bytes = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}

/// <summary>
/// Extension methods for SecurityHeadersMiddleware.
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    /// <summary>
    /// Adds the SecurityHeadersMiddleware to the application pipeline.
    /// MUST be added early in the pipeline (before authentication).
    /// </summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
