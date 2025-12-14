using Microsoft.EntityFrameworkCore;
using VendorMdm.Shared.Models;
using VendorMdm.Shared.Mapping;
using VendorMdm.Api.Data;

namespace VendorMdm.Api.Services;

/// <summary>
/// External system ID mapping service implementation.
/// Manages mappings between canonical entity IDs and external system IDs.
/// Implements anti-corruption layer for SAP, Salesforce, SuccessFactors, etc.
/// </summary>
public class ExternalSystemMappingService : IExternalSystemMappingService
{
    private readonly SqlDbContext _context;
    private readonly ILogger<ExternalSystemMappingService> _logger;
    
    public ExternalSystemMappingService(SqlDbContext context, ILogger<ExternalSystemMappingService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<string> GetOrCreateExternalIdAsync(
        Guid canonicalId, 
        string entityType, 
        string systemName, 
        string systemEnvironment = "Production")
    {
        var existing = await GetExternalIdAsync(canonicalId, entityType, systemName, systemEnvironment);
        if (existing != null)
            return existing;
        
        // Generate new external system ID
        var externalId = await GenerateNewExternalIdAsync(entityType, systemName);
        
        await CreateMappingAsync(canonicalId, entityType, externalId, systemName, systemEnvironment);
        
        return externalId;
    }
    
    public async Task<string?> GetExternalIdAsync(
        Guid canonicalId, 
        string entityType, 
        string systemName, 
        string systemEnvironment = "Production")
    {
        var mapping = await _context.ExternalSystemMappings
            .FirstOrDefaultAsync(m => 
                m.CanonicalEntityId == canonicalId &&
                m.EntityType == entityType &&
                m.SystemName == systemName &&
                m.SystemEnvironment == systemEnvironment);
        
        return mapping?.ExternalSystemId;
    }
    
    public async Task<Guid?> GetCanonicalIdAsync(
        string externalId, 
        string entityType, 
        string systemName, 
        string systemEnvironment = "Production")
    {
        var mapping = await _context.ExternalSystemMappings
            .FirstOrDefaultAsync(m => 
                m.ExternalSystemId == externalId &&
                m.EntityType == entityType &&
                m.SystemName == systemName &&
                m.SystemEnvironment == systemEnvironment);
        
        return mapping?.CanonicalEntityId;
    }
    
    public async Task CreateMappingAsync(
        Guid canonicalId, 
        string entityType, 
        string externalId, 
        string systemName, 
        string systemEnvironment = "Production")
    {
        var mapping = new ExternalSystemMapping
        {
            CanonicalEntityId = canonicalId,
            EntityType = entityType,
            ExternalSystemId = externalId,
            SystemName = systemName,
            SystemEnvironment = systemEnvironment,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.ExternalSystemMappings.Add(mapping);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation(
            "Created external system mapping: {EntityType} {CanonicalId} → {SystemName}:{ExternalId}", 
            entityType, canonicalId, systemName, externalId);
    }
    
    private async Task<string> GenerateNewExternalIdAsync(string entityType, string systemName)
    {
        // In a real implementation, this would call the external system to create a new ID
        // For SAP: call BAPI to get next LIFNR
        // For Salesforce: create Account and get Salesforce ID
        // For SuccessFactors: create Person and get Person ID
        
        // For now, generate a placeholder that's system-specific
        return systemName.ToUpper() switch
        {
            "SAP" => $"10{Guid.NewGuid().ToString("N").Substring(0, 8)}",  // SAP format: 10 + 8 digits
            "SALESFORCE" => $"001{Guid.NewGuid().ToString("N").Substring(0, 12)}",  // SF format
            "SUCCESSFACTORS" => $"EMP{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}",
            _ => $"{systemName}_{Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper()}"
        };
    }
}

/// <summary>
/// SAP-specific implementation of external system mapping service.
/// Provides backward compatibility with ISapIdMappingService interface.
/// </summary>
public class SapIdMappingService : ISapIdMappingService
{
    private readonly IExternalSystemMappingService _externalSystemMapping;
    
    public SapIdMappingService(IExternalSystemMappingService externalSystemMapping)
    {
        _externalSystemMapping = externalSystemMapping;
    }
    
    // ISapIdMappingService methods delegate to IExternalSystemMappingService with "SAP" as system name
    public Task<string> GetOrCreateExternalIdAsync(Guid canonicalId, string entityType, string systemName, string systemEnvironment = "Production")
        => _externalSystemMapping.GetOrCreateExternalIdAsync(canonicalId, entityType, systemName, systemEnvironment);
    
    public Task<string?> GetExternalIdAsync(Guid canonicalId, string entityType, string systemName, string systemEnvironment = "Production")
        => _externalSystemMapping.GetExternalIdAsync(canonicalId, entityType, systemName, systemEnvironment);
    
    public Task<Guid?> GetCanonicalIdAsync(string externalId, string entityType, string systemName, string systemEnvironment = "Production")
        => _externalSystemMapping.GetCanonicalIdAsync(externalId, entityType, systemName, systemEnvironment);
    
    public Task CreateMappingAsync(Guid canonicalId, string entityType, string externalId, string systemName, string systemEnvironment = "Production")
        => _externalSystemMapping.CreateMappingAsync(canonicalId, entityType, externalId, systemName, systemEnvironment);
}
