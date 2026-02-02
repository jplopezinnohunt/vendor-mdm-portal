namespace VendorMdm.Core.Framework.Security
{
    /// <summary>
    /// Responsible for sanitizing untrusted user input to prevent XSS.
    /// </summary>
    public interface IInputSanitizer
    {
        /// <summary>
        /// Strips dangerous HTML and Script tags from the input.
        /// </summary>
        /// <param name="input">Raw string.</param>
        /// <returns>Safe string.</returns>
        string Sanitize(string input);
    }
}
