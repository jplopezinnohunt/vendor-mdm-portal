namespace VendorMdm.Shared.Models.SapSimulation;

/// <summary>
/// Request to create new vendor in SAP (BAPI_VENDOR_CREATE1)
/// </summary>
public class VendorCreateRequest
{
    public string AccountGroup { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
    public SapGeneralData GeneralData { get; set; } = new();
    public List<SapBankAccount> BankAccounts { get; set; } = new();
    public SapCompanyCodeData CompanyCodeData { get; set; } = new();
}

/// <summary>
/// Response from vendor creation
/// </summary>
public class VendorCreateResponse
{
    public bool Success { get; set; }
    public string VendorNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public SapReturnMessage SapReturn { get; set; } = new();
}

/// <summary>
/// Request to update existing vendor in SAP (BAPI_VENDOR_CHANGE)
/// </summary>
public class VendorUpdateRequest
{
    public string VendorNumber { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
    public VendorChanges Changes { get; set; } = new();
}

/// <summary>
/// Changes to apply to vendor
/// </summary>
public class VendorChanges
{
    public SapGeneralData? GeneralData { get; set; }
    public List<BankAccountChange>? BankAccounts { get; set; }
    public SapCompanyCodeData? CompanyCodeData { get; set; }
}

/// <summary>
/// Bank account change with operation type
/// </summary>
public class BankAccountChange
{
    public string Operation { get; set; } = "UPDATE";  // "ADD", "UPDATE", "DELETE"
    public SapBankAccount BankAccount { get; set; } = new();
}

/// <summary>
/// Response from vendor update
/// </summary>
public class VendorUpdateResponse
{
    public bool Success { get; set; }
    public string VendorNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public SapReturnMessage SapReturn { get; set; } = new();
}

/// <summary>
/// SAP BAPI return message structure
/// </summary>
public class SapReturnMessage
{
    public string Type { get; set; } = string.Empty;  // S=Success, E=Error, W=Warning, I=Info
    public string Id { get; set; } = string.Empty;     // Message class (e.g., "VK")
    public string Number { get; set; } = string.Empty; // Message number
    public string Message { get; set; } = string.Empty;
    public string LogNo { get; set; } = string.Empty;
    public string LogMsgNo { get; set; } = string.Empty;
    public string MessageV1 { get; set; } = string.Empty;
    public string MessageV2 { get; set; } = string.Empty;
    public string MessageV3 { get; set; } = string.Empty;
    public string MessageV4 { get; set; } = string.Empty;
}
