using System.Text.RegularExpressions;
using VendorMdm.Shared.Models.SapSimulation;

namespace VendorMdm.Api.Services.Helpers;

/// <summary>
/// Validator for SWIFT/BIC codes
/// Format: AAAA BB CC DDD
/// - AAAA: Bank code (4 letters)
/// - BB: Country code (2 letters)
/// - CC: Location code (2 letters or digits)
/// - DDD: Branch code (3 letters or digits, optional)
/// </summary>
public class SwiftValidator
{
    private static readonly Regex SwiftRegex8 = new(@"^[A-Z]{6}[A-Z0-9]{2}$");
    private static readonly Regex SwiftRegex11 = new(@"^[A-Z]{6}[A-Z0-9]{5}$");

    public SwiftValidation Validate(string swift)
    {
        var result = new SwiftValidation();
        
        // Remove spaces and convert to uppercase
        swift = swift?.Replace(" ", "").ToUpper() ?? "";
        
        if (string.IsNullOrEmpty(swift))
        {
            result.Valid = false;
            return result;
        }

        // Must be 8 or 11 characters
        if (swift.Length != 8 && swift.Length != 11)
        {
            result.Valid = false;
            return result;
        }

        // Validate format
        bool validFormat = swift.Length == 8 
            ? SwiftRegex8.IsMatch(swift) 
            : SwiftRegex11.IsMatch(swift);

        if (!validFormat)
        {
            result.Valid = false;
            return result;
        }

        // Extract components
        result.BankCode = swift.Substring(0, 4);
        result.CountryCode = swift.Substring(4, 2);
        result.LocationCode = swift.Substring(6, 2);
        result.BranchCode = swift.Length == 11 ? swift.Substring(8, 3) : "XXX";
        result.Valid = true;

        return result;
    }
}
