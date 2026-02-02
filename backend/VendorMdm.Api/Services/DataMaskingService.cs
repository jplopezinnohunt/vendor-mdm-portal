using System.Text;

namespace VendorMdm.Api.Services;

/// <summary>
/// Service for masking PII data in compliance with GDPR.
/// Implements Pattern 17: Data Privacy & Masking.
/// </summary>
public interface IDataMaskingService
{
    string MaskEmail(string email);
    string MaskTaxId(string taxId);
    string MaskBankAccount(string account);
    string MaskPhoneNumber(string phone);
    string MaskCreditCard(string cardNumber);
}

public class DataMaskingService : IDataMaskingService
{
    /// <summary>
    /// Masks email: john.doe@example.com → j***@example.com
    /// </summary>
    public string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return "***";

        var parts = email.Split('@');
        if (parts[0].Length == 0)
            return $"***@{parts[1]}";

        return $"{parts[0][0]}***@{parts[1]}";
    }

    /// <summary>
    /// Masks tax ID: 123-45-6789 → ***-**-6789
    /// </summary>
    public string MaskTaxId(string taxId)
    {
        if (string.IsNullOrWhiteSpace(taxId))
            return "***";

        // Keep last 4 characters
        if (taxId.Length <= 4)
            return "***";

        var lastFour = taxId.Substring(taxId.Length - 4);
        var masked = new string('*', taxId.Length - 4);
        
        // Preserve formatting (dashes, spaces)
        var result = new StringBuilder();
        int maskedIndex = 0;
        
        for (int i = 0; i < taxId.Length; i++)
        {
            if (char.IsDigit(taxId[i]) || char.IsLetter(taxId[i]))
            {
                if (i >= taxId.Length - 4)
                {
                    result.Append(taxId[i]);
                }
                else
                {
                    result.Append('*');
                }
            }
            else
            {
                result.Append(taxId[i]); // Preserve formatting
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Masks bank account: 1234567890 → ******7890
    /// </summary>
    public string MaskBankAccount(string account)
    {
        if (string.IsNullOrWhiteSpace(account))
            return "***";

        if (account.Length <= 4)
            return "***";

        return $"******{account.Substring(account.Length - 4)}";
    }

    /// <summary>
    /// Masks phone: +1-555-123-4567 → +1-***-***-4567
    /// </summary>
    public string MaskPhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return "***";

        if (phone.Length <= 4)
            return "***";

        var lastFour = phone.Substring(phone.Length - 4);
        var prefix = phone.Length > 10 ? phone.Substring(0, Math.Min(3, phone.Length - 7)) : "";
        
        return $"{prefix}***-***-{lastFour}";
    }

    /// <summary>
    /// Masks credit card: 4532-1234-5678-9010 → ****-****-****-9010
    /// </summary>
    public string MaskCreditCard(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
            return "***";

        // Remove spaces/dashes for processing
        var digitsOnly = new string(cardNumber.Where(char.IsDigit).ToArray());
        
        if (digitsOnly.Length <= 4)
            return "***";

        var lastFour = digitsOnly.Substring(digitsOnly.Length - 4);
        return $"****-****-****-{lastFour}";
    }
}
