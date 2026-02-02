using VendorMdm.Core.Framework.Security;
using Ganss.Xss;

namespace VendorMdm.Infrastructure.Security
{
    /// <summary>
    /// Anti-XSS sanitizer using HtmlSanitizer library.
    /// </summary>
    public class HtmlInputSanitizer : IInputSanitizer
    {
        private readonly HtmlSanitizer _sanitizer;

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

        public string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return _sanitizer.Sanitize(input);
        }
    }
}
