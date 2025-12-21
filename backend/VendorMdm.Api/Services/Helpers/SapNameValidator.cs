using System.Text.RegularExpressions;
using VendorMdm.Shared.Models.SapSimulation;

namespace VendorMdm.Api.Services.Helpers;

/// <summary>
/// Validator for SAP vendor names following SAP business rules
/// SAP NAME1 and NAME2 fields: Max 35 characters each
/// SAP SORT1 (search term): Max 20 characters
/// </summary>
public class SapNameValidator
{
    private const int MAX_NAME_LENGTH = 35;
    private const int MAX_SEARCH_TERM_LENGTH = 20;
    private static readonly Regex AllowedCharsRegex = new(@"^[A-Za-z0-9 \-\.,]+$");

    public NameValidationResult Validate(string name, string nameType)
    {
        var result = new NameValidationResult
        {
            Valid = true
        };
        
        if (string.IsNullOrWhiteSpace(name))
        {
            result.Valid = false;
            result.Errors.Add("Name cannot be empty");
            return result;
        }

        // Trim and normalize
        name = name.Trim();
        result.Normalized = name;
        
        // Check length
        if (name.Length > MAX_NAME_LENGTH)
        {
            result.Valid = false;
            result.Errors.Add($"Name exceeds maximum length of {MAX_NAME_LENGTH} characters");
        }

        // Check allowed characters
        if (!AllowedCharsRegex.IsMatch(name))
        {
            result.Valid = false;
            result.Errors.Add("Name contains invalid characters. Only A-Z, 0-9, space, hyphen, period, and comma allowed");
        }

        // Check for purely numeric
        if (name.All(char.IsDigit))
        {
            result.Valid = false;
            result.Errors.Add("Name cannot be purely numeric");
        }

        // Check for leading/trailing spaces (already trimmed, but check original)
        if (name != name.Trim())
        {
            result.Warnings.Add("Leading or trailing spaces removed");
        }

        // Check for consecutive spaces
        if (name.Contains("  "))
        {
            result.Warnings.Add("Name contains consecutive spaces");
            name = Regex.Replace(name, @"\s+", " ");
            result.Normalized = name;
        }

        // Convert to SAP format
        result.SapFormat = ConvertToSapFormat(name, nameType);

        return result;
    }

    private SapNameFormat ConvertToSapFormat(string name, string nameType)
    {
        if (nameType == "PERSON")
        {
            // For persons, split into first and last name
            var parts = name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            return new SapNameFormat
            {
                Name1 = parts[0].ToUpper(),
                Name2 = parts.Length > 1 ? parts[1] : "",
                SearchTerm = parts[0].Substring(0, Math.Min(MAX_SEARCH_TERM_LENGTH, parts[0].Length)).ToUpper()
            };
        }
        else // COMPANY
        {
            // For companies, split at 35 characters if needed
            return new SapNameFormat
            {
                Name1 = name.Length > MAX_NAME_LENGTH ? name.Substring(0, MAX_NAME_LENGTH) : name,
                Name2 = name.Length > MAX_NAME_LENGTH ? name.Substring(MAX_NAME_LENGTH, Math.Min(MAX_NAME_LENGTH, name.Length - MAX_NAME_LENGTH)) : "",
                SearchTerm = name.Substring(0, Math.Min(MAX_SEARCH_TERM_LENGTH, name.Length)).ToUpper()
            };
        }
    }
}
