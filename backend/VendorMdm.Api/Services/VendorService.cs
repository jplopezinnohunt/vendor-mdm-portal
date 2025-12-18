using VendorMdm.Api.Data;
using VendorMdm.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace VendorMdm.Api.Services;

public class VendorService : IVendorService
{
    private readonly SqlDbContext _context;
    private readonly CosmosRepository _cosmosRepository;
    private readonly ILogger<VendorService> _logger;

    public VendorService(
        SqlDbContext context,
        CosmosRepository cosmosRepository,
        ILogger<VendorService> logger)
    {
        _context = context;
        _cosmosRepository = cosmosRepository;
        _logger = logger;
    }

    public async Task<Vendor> CreateVendorAsync(Vendor vendor)
    {
        if (vendor == null) throw new ArgumentNullException(nameof(vendor));

        // 1. SQL Persistence
        vendor.Id = Guid.NewGuid();
        vendor.CreatedAt = DateTime.UtcNow;
        vendor.UpdatedAt = DateTime.UtcNow;
        vendor.EntityVersion = 1;
        vendor.SchemaVersion = "v1.0.0";
        if (string.IsNullOrEmpty(vendor.Status)) vendor.Status = VendorStatus.Active;
            if (string.IsNullOrEmpty(vendor.SourceSystem) || vendor.SourceSystem == SourceSystems.Portal) vendor.SourceSystem = SourceSystems.GetDefaultSource(typeof(Vendor));

        // Validate required fields (optional double-check)
        if (string.IsNullOrEmpty(vendor.LegalName)) throw new ArgumentException("Legal Name is required");
        if (string.IsNullOrEmpty(vendor.PrimaryContactEmail)) throw new ArgumentException("Primary Contact Email is required");

        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Vendor created in SQL: {Id}", vendor.Id);

        // 2. Functional Log (Cosmos Artifact)
        try 
        {
            await _cosmosRepository.SaveArtifactAsync(vendor.Id.ToString(), vendor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save Vendor artifact to Cosmos: {Id}", vendor.Id);
        }

        // 3. Outbound Port (Event Bus)
        try
        {
            var domainEvent = new DomainEvent
            {
                EventType = "VendorCreated",
                EntityId = vendor.Id.ToString(),
                Data = vendor,
                Timestamp = DateTime.UtcNow,
                Source = "VendorMdm.Api",
                SchemaVersion = vendor.SchemaVersion
            };
            await _cosmosRepository.LogDomainEventAsync(domainEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to emit VendorCreated event: {Id}", vendor.Id);
        }

        return vendor;
    }

    public async Task<Vendor> UpdateVendorAsync(Vendor vendor)
    {
        // 1. SQL
        vendor.UpdatedAt = DateTime.UtcNow;
        vendor.IncrementVersion();

        _context.Vendors.Update(vendor);
        await _context.SaveChangesAsync();

        // 2. Artifact
        await _cosmosRepository.SaveArtifactAsync(vendor.Id.ToString(), vendor);

        // 3. Event
        var domainEvent = new DomainEvent
        {
            EventType = "VendorUpdated",
            EntityId = vendor.Id.ToString(),
            Data = vendor,
            Timestamp = DateTime.UtcNow,
            Source = "VendorMdm.Api",
            SchemaVersion = vendor.SchemaVersion
        };
        await _cosmosRepository.LogDomainEventAsync(domainEvent);

        return vendor;
    }

    public async Task<Vendor?> GetVendorByIdAsync(Guid id)
    {
        return await _context.Vendors.FindAsync(id);
    }

    public async Task<Vendor?> GetVendorByEmailAsync(string email)
    {
        return await _context.Vendors.FirstOrDefaultAsync(v => v.PrimaryContactEmail == email);
    }

    public async Task<List<Vendor>> GetAllVendorsAsync()
    {
        return await _context.Vendors.ToListAsync();
    }
}
