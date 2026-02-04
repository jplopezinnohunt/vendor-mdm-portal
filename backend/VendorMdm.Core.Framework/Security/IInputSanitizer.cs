namespace VendorMdm.Core.Framework.Security;

/// <summary>
/// Provides input sanitization services to prevent XSS, SQL injection, and other injection attacks.
/// Implements Section 7.C (Input Hygiene) from moderngoldenrules.md.
/// </summary>
public interface IInputSanitizer
{
    /// <summary>
    /// Sanitizes HTML input by removing dangerous tags and scripts.
    /// </summary>
    /// <param name="input">The input string to sanitize</param>
    /// <returns>Sanitized string safe for HTML display</returns>
    string SanitizeHtml(string input);

    /// <summary>
    /// Sanitizes SQL input by escaping dangerous characters.
    /// Note: EF Core uses parameterized queries by default. This is for legacy scenarios.
    /// </summary>
    /// <param name="input">The input string to sanitize</param>
    /// <returns>Sanitized string safe for SQL operations</returns>
    string SanitizeSql(string input);

    /// <summary>
    /// Sanitizes file names by removing invalid characters and path traversal attempts.
    /// </summary>
    /// <param name="input">The filename to sanitize</param>
    /// <returns>Sanitized filename safe for file system operations</returns>
    string SanitizeFileName(string input);

    /// <summary>
    /// Checks if a string contains potentially dangerous content without modifying it.
    /// </summary>
    /// <param name="input">The input string to check</param>
    /// <returns>True if dangerous content is detected, false otherwise</returns>
    bool ContainsDangerousContent(string input);
}
