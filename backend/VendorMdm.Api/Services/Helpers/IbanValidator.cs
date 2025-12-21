using System.Numerics;
using System.Text.RegularExpressions;
using VendorMdm.Shared.Models.SapSimulation;

namespace VendorMdm.Api.Services.Helpers;

/// <summary>
/// IBAN validator implementing mod-97 algorithm
/// Supports all SEPA and international IBAN formats
/// Pattern from UNESCO Mo UV bank validation
/// </summary>
public class IbanValidator
{
    private static readonly Dictionary<string, int> IbanLengths = new()
    {
        ["AD"] = 24, ["AE"] = 23, ["AL"] = 28, ["AT"] = 20, ["AZ"] = 28,
        ["BA"] = 20, ["BE"] = 16, ["BG"] = 22, ["BH"] = 22, ["BR"] = 29,
        ["CH"] = 21, ["CR"] = 21, ["CY"] = 28, ["CZ"] = 24, ["DE"] = 22,
        ["DK"] = 18, ["DO"] = 28, ["EE"] = 20, ["ES"] = 24, ["FI"] = 18,
        ["FR"] = 27, ["GB"] = 22, ["GE"] = 22, ["GI"] = 23, ["GR"] = 27,
        ["GT"] = 28, ["HR"] = 21, ["HU"] = 28, ["IE"] = 22, ["IL"] = 23,
        ["IS"] = 26, ["IT"] = 27, ["JO"] = 30, ["KW"] = 30, ["KZ"] = 20,
        ["LB"] = 28, ["LI"] = 21, ["LT"] = 20, ["LU"] = 20, ["LV"] = 21,
        ["MC"] = 27, ["MD"] = 24, ["ME"] = 22, ["MK"] = 19, ["MR"] = 27,
        ["MT"] = 31, ["MU"] = 30, ["NL"] = 18, ["NO"] = 15, ["PK"] = 24,
        ["PL"] = 28, ["PS"] = 29, ["PT"] = 25, ["QA"] = 29, ["RO"] = 24,
        ["RS"] = 22, ["SA"] = 24, ["SE"] = 24, ["SI"] = 19, ["SK"] = 24,
        ["SM"] = 27, ["TN"] = 24, ["TR"] = 26
    };

    public IbanValidation Validate(string iban)
    {
        var result = new IbanValidation();
        
        // Remove spaces and convert to uppercase
        iban = iban?.Replace(" ", "").Replace("-", "").ToUpper() ?? "";
        
        if (string.IsNullOrEmpty(iban))
        {
            result.Valid = false;
            result.Format = "IBAN cannot be empty";
            return result;
        }

        // Must start with 2-letter country code
        if (iban.Length < 2 || !char.IsLetter(iban[0]) || !char.IsLetter(iban[1]))
        {
            result.Valid = false;
            result.Format = "IBAN must start with 2-letter country code";
            return result;
        }

        var countryCode = iban.Substring(0, 2);
        result.Country = countryCode;

        // Check length
        if (!IbanLengths.TryGetValue(countryCode, out int expectedLength))
        {
            result.Valid = false;
            result.Format = $"Unsupported country code: {countryCode}";
            return result;
        }

        if (iban.Length != expectedLength)
        {
            result.Valid = false;
            result.Format = $"Invalid length for {countryCode}. Expected {expectedLength}, got {iban.Length}";
            return result;
        }

        // Validate characters (alphanumeric only)
        if (!Regex.IsMatch(iban, @"^[A-Z0-9]+$"))
        {
            result.Valid = false;
            result.Format = "IBAN contains invalid characters. Only A-Z and 0-9 allowed";
            return result;
        }

        // Validate checksum (mod 97)
        if (!ValidateChecksum(iban))
        {
            result.Valid = false;
            result.Checksum = "Invalid IBAN checksum (mod 97 validation failed)";
            return result;
        }

        // Extract bank code and account number (country-specific)
        ExtractComponents(iban, countryCode, result);

        result.Valid = true;
        result.Checksum = "valid";
        result.Format = "valid";
        
        return result;
    }

    private bool ValidateChecksum(string iban)
    {
        // Move first 4 characters to end
        string rearranged = iban.Substring(4) + iban.Substring(0, 4);
        
        // Replace letters with numbers (A=10, B=11, ..., Z=35)
        string numericIban = string.Empty;
        foreach (char c in rearranged)
        {
            if (char.IsLetter(c))
                numericIban += (c - 'A' + 10).ToString();
            else
                numericIban += c;
        }

        // Calculate mod 97
        try
        {
            BigInteger ibanNumber = BigInteger.Parse(numericIban);
            return ibanNumber % 97 == 1;
        }
        catch
        {
            return false;
        }
    }

    private void ExtractComponents(string iban, string countryCode, IbanValidation result)
    {
        // Extract bank code and account number (simplified - country-specific rules vary)
        switch (countryCode)
        {
            case "FR":  // France: 4-char country+check, 5-char bank, 5-char branch, 11-char account, 2-char key
                if (iban.Length  >= 27)
                {
                    result.BankCode = iban.Substring(4, 5);
                    result.AccountNumber = iban.Substring(14, 11);
                }
                break;
            
            case "DE":  // Germany: 4-char country+check, 8-char bank, 10-char account
                if (iban.Length >= 22)
                {
                    result.BankCode = iban.Substring(4, 8);
                    result.AccountNumber = iban.Substring(12, 10);
                }
                break;
            
            case "GB":  // UK: 4-char country+check, 4-char bank, 6-char sort code, 8-char account
                if (iban.Length >= 22)
                {
                    result.BankCode = iban.Substring(4, 4);
                    result.AccountNumber = iban.Substring(14, 8);
                }
                break;
            
            case "ES":  // Spain: 4-char country+check, 4-char bank, 4-char branch, 2-char check, 10-char account
                if (iban.Length >= 24)
                {
                    result.BankCode = iban.Substring(4, 4);
                    result.AccountNumber = iban.Substring(14, 10);
                }
                break;
            
            case "IT":  // Italy: similar to France
                if (iban.Length >= 27)
                {
                    result.BankCode = iban.Substring(5, 5);
                    result.AccountNumber = iban.Substring(15, 12);
                }
                break;
            
            default:
                // Generic extraction - bank code typically after country+check
                if (iban.Length > 8)
                {
                    result.BankCode = iban.Substring(4, Math.Min(4, iban.Length - 4));
                    result.AccountNumber = iban.Substring(8);
                }
                break;
        }
    }
}
