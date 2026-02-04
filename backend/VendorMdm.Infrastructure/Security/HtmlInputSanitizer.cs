using System.Text.RegularExpressions;
using VendorMdm.Core.Framework.Security;
using Ganss.Xss;

namespace VendorMdm.Infrastructure.Security
{
    /// <summary>
    /// Anti-XSS sanitizer using HtmlSanitizer library.
    /// Implements IInputSanitizer from Core.Framework.
    /// </summary>
    public class HtmlInputSanitizer : IInputSanitizer
    {
        private readonly HtmlSanitizer _sanitizer;

        // Patterns for detecting dangerous content
        private static readonly Regex ScriptPattern = new(@"<script[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex EventHandlerPattern = new(@"on\w+\s*=", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex SqlInjectionPattern = new(@"(\b(union|select|insert|update|delete|drop|alter|exec|execute)\b.*\b(from|into|table|database)\b)|(--)|(;.*\b(drop|delete|update)\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex PathTraversalPattern = new(@"\.\.[/\\]", RegexOptions.Compiled);

        public HtmlInputSanitizer()
        {
            _sanitizer = new HtmlSanitizer();
            // Allow only safe tags
            _sanitizer.AllowedTags.Clear();
            _sanitizer.AllowedTags.Add("b");
            _sanitizer.AllowedTags.Add("i");
            _sanitizer.AllowedTags.Add("u");
            _sanitizer.AllowedTags.Add("p");
            _sanitizer.AllowedTags.Add("br");
        }

        /// <inheritdoc />
        public string SanitizeHtml(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return _sanitizer.Sanitize(input);
        }

        /// <inheritdoc />
        public string SanitizeSql(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Note: This is a fallback. EF Core uses parameterized queries by default.
            // This removes common SQL injection patterns for legacy scenarios.
            return input
                .Replace("'", "''")
                .Replace("--", "")
                .Replace(";", "");
        }

        /// <inheritdoc />
        public string SanitizeFileName(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Remove path traversal attempts
            var sanitized = PathTraversalPattern.Replace(input, "");

            // Remove invalid file name characters
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                sanitized = sanitized.Replace(c.ToString(), "");
            }

            // Limit length
            if (sanitized.Length > 255)
                sanitized = sanitized.Substring(0, 255);

            return sanitized;
        }

        /// <inheritdoc />
        public bool ContainsDangerousContent(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            return ScriptPattern.IsMatch(input) ||
                   EventHandlerPattern.IsMatch(input) ||
                   SqlInjectionPattern.IsMatch(input) ||
                   PathTraversalPattern.IsMatch(input);
        }
    }
}
