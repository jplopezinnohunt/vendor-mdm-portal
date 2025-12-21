namespace VendorMdm.Shared.Models.SapSimulation;

/// <summary>
/// Request to get vendor details from SAP (BAPI_VENDOR_GETDETAIL)
/// Maps to SAP tables: LFA1 (General), LFBK (Bank), LFB1 (Company Code)
/// </summary>
public class VendorGetRequest
{
    public string VendorNumber { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
}

/// <summary>
/// Complete vendor master data from SAP
/// </summary>
public class VendorGetResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public SapVendorDetail? Vendor { get; set; }
}

/// <summary>
/// SAP Vendor Master Data (LFA1 + LFBK + LFB1)
/// </summary>
public class SapVendorDetail
{
    public string SapId { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string AccountGroup { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool Blocked { get; set; }
    public bool DeletionFlag { get; set; }
    public SapGeneralData GeneralData { get; set; } = new();
    public List<SapBankAccount> BankAccounts { get; set; } = new();
    public SapCompanyCodeData CompanyCodeData { get; set; } = new();
}

/// <summary>
/// SAP LFA1 - General vendor data
/// </summary>
public class SapGeneralData
{
    public string Title { get; set; } = string.Empty;  // Mr, Ms, Dr, etc.
    public string Name1 { get; set; } = string.Empty;  // SAP NAME1 field (35 chars)
    public string Name2 { get; set; } = string.Empty;  // SAP NAME2 field (35 chars)
    public string SearchTerm { get; set; } = string.Empty;  // SAP SORT1 field (20 chars)
    public string Street { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string Nationality { get; set; } = string.Empty;
}

/// <summary>
/// SAP LFBK - Bank account data
/// </summary>
public class SapBankAccount
{
    public string BankCountry { get; set; } = string.Empty;
    public string BankKey { get; set; } = string.Empty;
    public string BankAccount { get; set; } = string.Empty;
    public string? Iban { get; set; }
    public string Swift { get; set; } = string.Empty;
    public string AccountHolder { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string? RoutingNumber { get; set; }  // For US banks
    public string? AccountType { get; set; }    // Checking/Savings for US
}

/// <summary>
/// SAP LFB1 - Company code data
/// </summary>
public class SapCompanyCodeData
{
    public string CompanyCode { get; set; } = string.Empty;
    public string ReconciliationAccount { get; set; } = string.Empty;
    public string PaymentTerms { get; set; } = string.Empty;
    public List<string> PaymentMethods { get; set; } = new();  // T=Transfer, C=Check
    public string Currency { get; set; } = string.Empty;
    public bool PaymentBlock { get; set; }
}
