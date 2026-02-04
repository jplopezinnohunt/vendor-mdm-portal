using System.Text.RegularExpressions;

namespace VendorMdm.Core.Framework.Security;

/// <summary>
/// Default implementation of IInputSanitizer.
/// Provides comprehensive input sanitization for XSS, SQL injection, and file system attacks.
/// </summary>
public class InputSanitizer : IInputSanitizer
{
    private static readonly string[] DangerousHtmlTags = new[]
    {
        "<script", "</script", "<iframe", "</iframe", "<object", "</object",
        "<embed", "</embed", "<applet", "</applet", "<meta", "<link",
        "<style", "</style", "<form", "</form", "<input", "<button"
    };

    private static readonly string[] DangerousHtmlAttributes = new[]
    {
        "javascript:", "onerror=", "onload=", "onclick=", "onmouseover=",
        "onfocus=", "onblur=", "onchange=", "onsubmit=", "eval(",
        "expression(", "vbscript:", "data:text/html"
    };

    private static readonly string[] PathTraversalPatterns = new[]
    {
        "..", "~", "/", "\\", ":", "*", "?", "\"", "<", ">", "|"
    };

    /// <inheritdoc/>
    public string SanitizeHtml(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var sanitized = input;

        // Remove dangerous HTML tags (case-insensitive)
        foreach (var tag in DangerousHtmlTags)
        {
            sanitized = Regex.Replace(
                sanitized,
                Regex.Escape(tag),
                string.Empty,
                RegexOptions.IgnoreCase
            );
        }

        // Remove dangerous HTML attributes and JavaScript
        foreach (var attr in DangerousHtmlAttributes)
        {
            sanitized = Regex.Replace(
                sanitized,
                Regex.Escape(attr),
                string.Empty,
                RegexOptions.IgnoreCase
            );
        }

        // Remove any remaining script tags (defense in depth)
        sanitized = Regex.Replace(
            sanitized,
            @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>",
            string.Empty,
            RegexOptions.IgnoreCase
        );

        return sanitized;
    }

    /// <inheritdoc/>
    public string SanitizeSql(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // Escape single quotes (basic SQL injection prevention)
        // Note: EF Core handles this via parameterized queries
        // This is only for legacy raw SQL scenarios
        return input.Replace("'", "''");
    }

    /// <inheritdoc/>
    public string SanitizeFileName(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // Remove path traversal attempts
        var sanitized = input;
        foreach (var pattern in PathTraversalPatterns)
        {
            sanitized = sanitized.Replace(pattern, string.Empty);
        }

        // Remove invalid file name characters
        var invalidChars = Path.GetInvalidFileNameChars();
        sanitized = string.Concat(sanitized.Split(invalidChars));

        // Ensure the filename is not empty after sanitization
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            throw new ArgumentException("File name is invalid after sanitization", nameof(input));
        }

        return sanitized;
    }

    /// <inheritdoc/>
    public bool ContainsDangerousContent(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        // Check for dangerous HTML tags
        foreach (var tag in DangerousHtmlTags)
        {
            if (input.Contains(tag, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        // Check for dangerous HTML attributes
        foreach (var attr in DangerousHtmlAttributes)
        {
            if (input.Contains(attr, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
