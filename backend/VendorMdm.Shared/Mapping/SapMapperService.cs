using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using VendorMdm.Shared.Models;

namespace VendorMdm.Shared.Mapping;

/// <summary>
/// SAP ID mapping service implementation.
/// Manages mappings between canonical entity IDs and SAP system IDs.
/// </summary>
public class SapIdMappingService : ISapIdMappingService
{
    private readonly DbContext _context; // Your DbContext
    private readonly ILogger<SapIdMappingService> _logger;
    
    public SapIdMappingService(DbContext context, ILogger<SapIdMappingService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<string> GetOrCreateSapIdAsync(Guid canonicalId, string entityType, string sapEnvironment = "D01")
    {
        var existing = await GetSapIdAsync(canonicalId, entityType, sapEnvironment);
        if (existing != null)
            return existing;
        
        // Generate new SAP ID (in real implementation, call SAP to create)
        var sapId = await GenerateNewSapIdAsync(entityType);
        
        await CreateMappingAsync(canonicalId, entityType, sapId, sapEnvironment);
        
        return sapId;
    }
    
    public async Task<string?> GetSapIdAsync(Guid canonicalId, string entityType, string sapEnvironment = "D01")
    {
        var mapping = await _context.Set<SapIdMapping>()
            .FirstOrDefaultAsync(m => 
                m.CanonicalEntityId == canonicalId &&
                m.EntityType == entityType &&
                m.SapEnvironment == sapEnvironment);
        
        return mapping?.SapId;
    }
    
    public async Task<Guid?> GetCanonicalIdAsync(string sapId, string entityType, string sapEnvironment = "D01")
    {
        var mapping = await _context.Set<SapIdMapping>()
            .FirstOrDefaultAsync(m => 
                m.SapId == sapId &&
                m.EntityType == entityType &&
                m.SapEnvironment == sapEnvironment);
        
        return mapping?.CanonicalEntityId;
    }
    
    public async Task CreateMappingAsync(Guid canonicalId, string entityType, string sapId, string sapEnvironment = "D01")
    {
        var mapping = new SapIdMapping
        {
            CanonicalEntityId = canonicalId,
            EntityType = entityType,
            SapId = sapId,
            SapEnvironment = sapEnvironment
        };
        
        _context.Set<SapIdMapping>().Add(mapping);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created SAP mapping: {EntityType} {CanonicalId} → {SapId}", 
            entityType, canonicalId, sapId);
    }
    
    private async Task<string> GenerateNewSapIdAsync(string entityType)
    {
        // In a real implementation, this would call SAP to create a new partner
        // For now, generate a placeholder
        return $"SAP_{entityType}_{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
    }
}

/// <summary>
/// Vendor SAP mapper implementation.
/// </summary>
public class VendorSapMapper : IVendorSapMapper
{
    private readonly ISapIdMappingService _sapIdService;
    private readonly ILogger<VendorSapMapper> _logger;
    
    public VendorSapMapper(ISapIdMappingService sapIdService, ILogger<VendorSapMapper> logger)
    {
        _sapIdService = sapIdService;
        _logger = logger;
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
