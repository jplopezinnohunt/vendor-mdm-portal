namespace VendorMdm.Shared.Mapping;

/// <summary>
/// Interface for SAP ID mapping service.
/// Anti-corruption layer: translates between canonical IDs and SAP IDs.
/// </summary>
public interface ISapIdMappingService
{
    /// <summary>
    /// Get SAP ID for a canonical entity, or create new mapping if doesn't exist.
    /// </summary>
    Task<string> GetOrCreateSapIdAsync(Guid canonicalId, string entityType, string sapEnvironment = "D01");
    
    /// <summary>
    /// Get existing SAP ID for a canonical entity.
    /// Returns null if no mapping exists.
    /// </summary>
    Task<string?> GetSapIdAsync(Guid canonicalId, string entityType, string sapEnvironment = "D01");
    
    /// <summary>
    /// Get canonical ID from SAP ID.
    /// Returns null if no mapping exists.
    /// </summary>
    Task<Guid?> GetCanonicalIdAsync(string sapId, string entityType, string sapEnvironment = "D01");
    
    /// <summary>
    /// Create explicit mapping between canonical and SAP IDs.
    /// </summary>
    Task CreateMappingAsync(Guid canonicalId, string entityType, string sapId, string sapEnvironment = "D01");
}

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
