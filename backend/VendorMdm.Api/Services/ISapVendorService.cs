using VendorMdm.Shared.Models.SapSimulation;

namespace VendorMdm.Api.Services;

/// <summary>
/// SAP Vendor Service Interface
/// Abstracts SAP integration to allow simulation and real RFC implementations
/// Pattern: Interface-based dependency injection for easy swapping
/// </summary>
public interface ISapVendorService
{
    /// <summary>
    /// Search for existing vendors (duplicate detection)
    /// Uses Levenshtein distance for fuzzy matching
    /// UNESCO pattern: Searches both Cosmos DB (pending) and SAP (existing)
    /// </summary>
    Task<VendorSearchResponse> SearchVendorsAsync(VendorSearchRequest request);
    
    /// <summary>
    /// Get complete vendor master data from SAP
    /// Maps to BAPI_VENDOR_GETDETAIL
    /// Returns data from LFA1, LFBK, LFB1 tables
    /// </summary>
    Task<VendorGetResponse> GetVendorAsync(string vendorNumber, string companyCode);
    
    /// <summary>
    /// Create new vendor in SAP
    /// Maps to BAPI_VENDOR_CREATE1
    /// </summary>
    Task<VendorCreateResponse> CreateVendorAsync(VendorCreateRequest request);
    
    /// <summary>
    /// Update existing vendor in SAP
    /// Maps to BAPI_VENDOR_CHANGE
    /// </summary>
    Task<VendorUpdateResponse> UpdateVendorAsync(string vendorNumber, VendorUpdateRequest request);
    
    /// <summary>
    /// Validate vendor name against SAP business rules
    /// Rules:
    /// - Max 35 characters per field
    /// - Only A-Z, 0-9, space, hyphen, period, comma
    /// - No leading/trailing spaces
    /// - No consecutive spaces
    /// - Cannot be purely numeric
    /// </summary>
    Task<NameValidationResult> ValidateNameAsync(NameValidationRequest request);
    
    /// <summary>
    /// Validate bank account details
    /// Country-specific validation rules:
    /// - SEPA countries: IBAN + BIC required
    /// - US: Routing number + Account number required
    /// - Others: SWIFT + Account number
    /// </summary>
    Task<BankValidationResult> ValidateBankAsync(BankValidationRequest request);
    
    /// <summary>
    /// Check for duplicate bank accounts in SAP
    /// Prevents same IBAN being registered to multiple vendors
    /// </summary>
    Task<BankDuplicateCheckResult> CheckBankDuplicateAsync(BankDuplicateCheckRequest request);
    
    /// <summary>
    /// Get bank field configuration for a country from SAP
    /// Returns UI configuration (which fields to show/hide, which are mandatory)
    /// Does NOT include validation logic (use ibankit for that)
    /// Maps to SAP table T012K or custom Z-function
    /// </summary>
    /// <param name="countryCode">2-letter country code (e.g., "FR", "DE")</param>
    /// <param name="companyCode">Company code (e.g., "UNES")</param>
    /// <returns>Field configuration from SAP business rules</returns>
    Task<BankCountryConfigResponse> GetBankCountryConfigurationAsync(string countryCode, string companyCode);
}
