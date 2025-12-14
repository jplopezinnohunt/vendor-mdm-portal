namespace VendorMdm.Shared.Mapping;

/// <summary>
/// Interface for external system ID mapping service.
/// Anti-corruption layer: translates between canonical IDs and external system IDs.
/// Supports SAP, Salesforce, SuccessFactors, Workday, and other integrations.
/// </summary>
public interface IExternalSystemMappingService
{
    /// <summary>
    /// Get external system ID for a canonical entity, or create new mapping if doesn't exist.
    /// </summary>
    Task<string> GetOrCreateExternalIdAsync(Guid canonicalId, string entityType, string systemName, string systemEnvironment = "Production");
    
    /// <summary>
    /// Get existing external system ID for a canonical entity.
    /// Returns null if no mapping exists.
    /// </summary>
    Task<string?> GetExternalIdAsync(Guid canonicalId, string entityType, string systemName, string systemEnvironment = "Production");
    
    /// <summary>
    /// Get canonical ID from external system ID.
    /// Returns null if no mapping exists.
    /// </summary>
    Task<Guid?> GetCanonicalIdAsync(string externalId, string entityType, string systemName, string systemEnvironment = "Production");
    
    /// <summary>
    /// Create explicit mapping between canonical and external system IDs.
    /// </summary>
    Task CreateMappingAsync(Guid canonicalId, string entityType, string externalId, string systemName, string systemEnvironment = "Production");
}

// Backward compatibility alias
public interface ISapIdMappingService : IExternalSystemMappingService { }

/// <summary>
/// SAP mapper interface for canonical Vendor entity.
/// Translates between canonical Vendor and SAP Business Partner.
/// NO SAP FIELDS in Vendor entity!
/// </summary>
public interface IVendorSapMapper
{
    /// <summary>
    /// Map canonical Vendor to SAP Business Partner structure.
    /// </summary>
    Task<SapBusinessPartner> ToSapAsync(Models.Vendor vendor);
    
    /// <summary>
    /// Map SAP Business Partner to canonical Vendor.
    /// </summary>
    Task<Models.Vendor> FromSapAsync(SapBusinessPartner sapBp);
}

/// <summary>
/// SAP Business Partner model (SAP-specific, NOT in canonical domain).
/// Used only for SAP integration, never exposed to apps.
/// </summary>
public class SapBusinessPartner
{
    /// <summary>
    /// SAP Partner number (LIFNR for vendors)
    /// </summary>
    public string PartnerNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// SAP Name 1
    /// </summary>
    public string Name1 { get; set; } = string.Empty;
    
    /// <summary>
    /// SAP Tax Number
    /// </summary>
    public string? TaxNumber { get; set; }
    
    /// <summary>
    /// SAP Email
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// SAP Partner Type (LI for vendor, KU for customer)
    /// </summary>
    public string PartnerType { get; set; } = "LI";
    
    // Add more SAP-specific fields as needed
}
