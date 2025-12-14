using Newtonsoft.Json;
using VendorMdm.Shared.Models;

namespace VendorMdm.Shared.Mapping;

/// <summary>
/// SAP ID mapping service implementation.
/// NOTE: This is an interface-only version. Actual implementation should be in the API project
/// where DbContext is available.
/// </summary>
public interface ISapIdMappingRepository
{
    Task<SapIdMapping?> GetMappingAsync(Guid canonicalId, string entityType, string sapEnvironment);
    Task<SapIdMapping?> GetMappingBySapIdAsync(string sapId, string entityType, string sapEnvironment);
    Task CreateMappingAsync(SapIdMapping mapping);
}

/// <summary>
/// Vendor SAP mapper implementation.
/// </summary>
public class VendorSapMapper : IVendorSapMapper
{
    private readonly ISapIdMappingService _sapIdService;
    
    public VendorSapMapper(ISapIdMappingService sapIdService)
    {
        _sapIdService = sapIdService;
    }
    
    public async Task<SapBusinessPartner> ToSapAsync(Vendor vendor)
    {
        // Get or create SAP ID for this vendor
        var sapId = await _sapIdService.GetOrCreateSapIdAsync(vendor.Id, nameof(Vendor));
        
        // Parse vendor Data JSON for additional fields
        var vendorData = string.IsNullOrWhiteSpace(vendor.Data) 
            ? new { } 
            : JsonConvert.DeserializeObject<dynamic>(vendor.Data);
        
        return new SapBusinessPartner
        {
            PartnerNumber = sapId,
            Name1 = vendor.LegalName,
            TaxNumber = vendor.TaxId,
            Email = vendor.PrimaryContactEmail,
            PartnerType = "LI" // Vendor/LIFNR
        };
    }
    
    public async Task<Vendor> FromSapAsync(SapBusinessPartner sapBp)
    {
        // Get canonical ID for this SAP partner
        var canonicalId = await _sapIdService.GetCanonicalIdAsync(sapBp.PartnerNumber, nameof(Vendor));
        
        var vendorData = new
        {
            legalName = sapBp.Name1,
            taxId = sapBp.TaxNumber,
            primaryContact = new
            {
                email = sapBp.Email
            },
            sapSync = new
            {
                lastSyncAt = DateTime.UtcNow,
                sapPartnerType = sapBp.PartnerType
            }
        };
        
        return new Vendor
        {
            Id = canonicalId ?? Guid.NewGuid(),
            LegalName = sapBp.Name1,
            TaxId = sapBp.TaxNumber,
            PrimaryContactEmail = sapBp.Email ?? string.Empty,
            Status = VendorStatus.Active,
            SourceSystem = SourceSystems.Sap,
            EntityVersion = 1,
            SchemaVersion = "v1.0.0",
            Data = JsonConvert.SerializeObject(vendorData)
        };
    }
}
