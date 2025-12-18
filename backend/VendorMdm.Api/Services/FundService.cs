using VendorMdm.Api.Data;
using VendorMdm.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace VendorMdm.Api.Services;

public class FundService : IFundService
{
    private readonly SqlDbContext _context;
    private readonly CosmosRepository _cosmosRepository;
    private readonly ILogger<FundService> _logger;

    public FundService(
        SqlDbContext context,
        CosmosRepository cosmosRepository,
        ILogger<FundService> logger)
    {
        _context = context;
        _cosmosRepository = cosmosRepository;
        _logger = logger;
    }

    public async Task<Fund> CreateFundAsync(Fund fund)
    {
        if (fund == null) throw new ArgumentNullException(nameof(fund));

        // 1. SQL Persistence
        fund.Id = Guid.NewGuid();
        fund.CreatedAt = DateTime.UtcNow;
        fund.UpdatedAt = DateTime.UtcNow;
        fund.EntityVersion = 1;
        fund.SchemaVersion = "v1.0.0";
        if (string.IsNullOrEmpty(fund.Status)) fund.Status = "Active";
        if (string.IsNullOrEmpty(fund.SourceSystem)) fund.SourceSystem = SourceSystems.GetDefaultSource(typeof(Fund));

        fund.ValidateCanonicalFields();

        _context.Funds.Add(fund);
        await _context.SaveChangesAsync();

        // 2. Artifact
        await _cosmosRepository.SaveArtifactAsync(fund.Id.ToString(), fund);

        // 3. Event
        var domainEvent = new DomainEvent
        {
            EventType = "FundCreated",
            EntityId = fund.Id.ToString(),
            Data = fund,
            Timestamp = DateTime.UtcNow,
            Source = "VendorMdm.Api",
            SchemaVersion = fund.SchemaVersion
        };
        await _cosmosRepository.LogDomainEventAsync(domainEvent);

        return fund;
    }

    public async Task<Fund> UpdateFundAsync(Fund fund)
    {
        fund.UpdatedAt = DateTime.UtcNow;
        fund.IncrementVersion();

        _context.Funds.Update(fund);
        await _context.SaveChangesAsync();

        await _cosmosRepository.SaveArtifactAsync(fund.Id.ToString(), fund);

        var domainEvent = new DomainEvent
        {
            EventType = "FundUpdated",
            EntityId = fund.Id.ToString(),
            Data = fund,
            Timestamp = DateTime.UtcNow,
            Source = "VendorMdm.Api",
            SchemaVersion = fund.SchemaVersion
        };
        await _cosmosRepository.LogDomainEventAsync(domainEvent);

        return fund;
    }

    public async Task<Fund?> GetFundByIdAsync(Guid id)
    {
        return await _context.Funds.FindAsync(id);
    }

    public async Task<List<Fund>> GetAllFundsAsync()
    {
        return await _context.Funds.ToListAsync();
    }
}
