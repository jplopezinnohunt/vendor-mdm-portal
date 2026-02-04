using Microsoft.AspNetCore.Mvc.Filters;
using VendorMdm.Core.Framework.Security;

namespace VendorMdm.Api.Filters;

/// <summary>
/// Action filter that automatically sanitizes string properties in DTOs to prevent XSS attacks.
/// Implements Section 7.C (Input Hygiene) from moderngoldenrules.md.
/// </summary>
public class InputSanitizationActionFilter : IAsyncActionFilter
{
    private readonly IInputSanitizer _sanitizer;
    private readonly ILogger<InputSanitizationActionFilter> _logger;

    public InputSanitizationActionFilter(
        IInputSanitizer sanitizer,
        ILogger<InputSanitizationActionFilter> logger)
    {
        _sanitizer = sanitizer;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        // Sanitize all action arguments
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument == null) continue;

            SanitizeObject(argument, context.HttpContext.TraceIdentifier);
        }

        await next();
    }

    private void SanitizeObject(object obj, string traceId)
    {
        if (obj == null) return;

        var type = obj.GetType();

        // Skip primitives and framework types
        if (type.IsPrimitive || type == typeof(string) || type.Namespace?.StartsWith("System") == true)
        {
            return;
        }

        // Get all string properties that are writable
        var properties = type.GetProperties()
            .Where(p => p.PropertyType == typeof(string) && p.CanWrite && p.CanRead);

        foreach (var prop in properties)
        {
            try
            {
                var value = prop.GetValue(obj) as string;
                if (string.IsNullOrEmpty(value)) continue;

                // Check if contains dangerous content
                if (_sanitizer.ContainsDangerousContent(value))
                {
                    var sanitized = _sanitizer.SanitizeHtml(value);

                    if (value != sanitized)
                    {
                        prop.SetValue(obj, sanitized);

                        _logger.LogWarning(
                            "[XSS] Sanitized input for {Property} in {Type}. TraceId: {TraceId}, " +
                            "OriginalLength: {OriginalLength}, SanitizedLength: {SanitizedLength}",
                            prop.Name,
                            type.Name,
                            traceId,
                            value.Length,
                            sanitized.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error sanitizing property {Property} in {Type}. TraceId: {TraceId}",
                    prop.Name,
                    type.Name,
                    traceId);
            }
        }

        // Recursively sanitize nested objects and collections
        var nestedProperties = type.GetProperties()
            .Where(p => p.CanRead && !p.PropertyType.IsPrimitive &&
                        p.PropertyType != typeof(string) &&
                        p.PropertyType.Namespace?.StartsWith("System") == false);

        foreach (var prop in nestedProperties)
        {
            try
            {
                var value = prop.GetValue(obj);
                if (value != null)
                {
                    SanitizeObject(value, traceId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error sanitizing nested property {Property} in {Type}. TraceId: {TraceId}",
                    prop.Name,
                    type.Name,
                    traceId);
            }
        }
    }
}
