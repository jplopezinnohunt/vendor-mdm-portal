namespace VendorMdm.Shared.Models.SapSimulation;

/// <summary>
/// Request to validate vendor name against SAP business rules
/// </summary>
public class NameValidationRequest
{
    public string Name { get; set; } = string.Empty;
    public string NameType { get; set; } = "PERSON";  // "PERSON" or "COMPANY"
}

/// <summary>
/// Result of name validation
/// </summary>
public class NameValidationResult
{
    public bool Valid { get; set; }
    public string Normalized { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public SapNameFormat SapFormat { get; set; } = new();
}

/// <summary>
/// Name formatted for SAP fields
/// </summary>
public class SapNameFormat
{
    public string Name1 { get; set; } = string.Empty;     // Max 35 chars, uppercase
    public string Name2 { get; set; } = string.Empty;     // Max 35 chars
    public string SearchTerm { get; set; } = string.Empty; // Max 20 chars, uppercase
}

/// <summary>
/// Request to validate bank account details
/// </summary>
public class BankValidationRequest
{
    public string BankCountry { get; set; } = string.Empty;
    public string? Iban { get; set; }
    public string? Swift { get; set; }
    public string? AccountNumber { get; set; }
    public string? RoutingNumber { get; set; }
    public string? AccountType { get; set; }  // For US: "Checking" or "Savings"
}

/// <summary>
/// Result of bank validation
/// </summary>
public class BankValidationResult
{
    public bool Valid { get; set; }
    public BankFieldValidations Validations { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Individual validation results for each bank field
/// </summary>
public class BankFieldValidations
{
    public IbanValidation? Iban { get; set; }
    public SwiftValidation? Swift { get; set; }
    public AccountNumberValidation? AccountNumber { get; set; }
    public RoutingNumberValidation? RoutingNumber { get; set; }
}

/// <summary>
/// IBAN validation details
/// </summary>
public class IbanValidation
{
    public bool Valid { get; set; }
    public string Checksum { get; set; } = string.Empty;  // "valid" or error message
    public string Format { get; set; } = string.Empty;     // "valid" or error message
    public string Country { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
}

/// <summary>
/// SWIFT/BIC validation details
/// </summary>
public class SwiftValidation
{
    public bool Valid { get; set; }
    public string BankCode { get; set; } = string.Empty;      // 4 chars
    public string CountryCode { get; set; } = string.Empty;   // 2 chars
    public string LocationCode { get; set; } = string.Empty;  // 2 chars
    public string BranchCode { get; set; } = string.Empty;    // 3 chars (optional, "XXX" if not specified)
}

/// <summary>
/// Account number validation
/// </summary>
public class AccountNumberValidation
{
    public bool Valid { get; set; }
    public string Format { get; set; } = string.Empty;  // "numeric", "alphanumeric"
    public int Length { get; set; }
}

/// <summary>
/// Routing number validation (US ACH)
/// </summary>
public class RoutingNumberValidation
{
    public bool Valid { get; set; }
    public bool ChecksumValid { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string BankLocation { get; set; } = string.Empty;
}

/// <summary>
/// Request to check for duplicate bank accounts
/// </summary>
public class BankDuplicateCheckRequest
{
    public string Iban { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
    public string? ExcludeVendorId { get; set; }  // Exclude this vendor from check
}

/// <summary>
/// Result of bank duplicate check
/// </summary>
public class BankDuplicateCheckResult
{
    public bool DuplicateFound { get; set; }
    public List<BankDuplicateMatch> Matches { get; set; } = new();
}

/// <summary>
/// Duplicate bank account match
/// </summary>
public class BankDuplicateMatch
{
    public string VendorNumber { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public string Iban { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;  // "Active", "Blocked"
}
