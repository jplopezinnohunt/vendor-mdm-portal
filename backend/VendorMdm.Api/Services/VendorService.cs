using VendorMdm.Api.Data;
using VendorMdm.Api.Services.Events;
using VendorMdm.Core.Framework.Events;
using VendorMdm.Core.Framework.Primitives;
using VendorMdm.Shared.Models;
using VendorMdm.Shared.Models.Sanctions;
using Microsoft.EntityFrameworkCore;

namespace VendorMdm.Api.Services;

public class VendorService : IVendorService
{
    private readonly SqlDbContext _context;
    private readonly CosmosRepository _cosmosRepository;
    private readonly ILogger<VendorService> _logger;
    private readonly ISanctionsScreeningService _sanctionsService;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public VendorService(
        SqlDbContext context,
        CosmosRepository cosmosRepository,
        ILogger<VendorService> logger,
        ISanctionsScreeningService sanctionsService,
        IDomainEventDispatcher eventDispatcher)
    {
        _context = context;
        _cosmosRepository = cosmosRepository;
        _logger = logger;
        _sanctionsService = sanctionsService;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Result<Vendor>> CreateVendorAsync(Vendor vendor, bool forceCreation = false)
    {
        // Validate required input
        if (vendor == null)
            return Result.Fail<Vendor>("Vendor is required");

        if (string.IsNullOrEmpty(vendor.LegalName))
            return Result.Fail<Vendor>("Legal Name is required");

        if (string.IsNullOrEmpty(vendor.PrimaryContactEmail))
            return Result.Fail<Vendor>("Primary Contact Email is required");

        // 1. SQL Persistence
        vendor.Id = Guid.NewGuid();
        vendor.CreatedAt = DateTime.UtcNow;
        vendor.UpdatedAt = DateTime.UtcNow;
        vendor.EntityVersion = 1;
        vendor.SchemaVersion = "v1.0.0";
        if (string.IsNullOrEmpty(vendor.Status)) vendor.Status = VendorStatus.Active;
        if (string.IsNullOrEmpty(vendor.SourceSystem) || vendor.SourceSystem == SourceSystems.Portal) vendor.SourceSystem = SourceSystems.GetDefaultSource(typeof(Vendor));

        // --- SECURITY & VALIDATION CHECKS ---

        // 1. Duplicate Check (Email or Tax ID)
        var existingVendor = await _context.Vendors
            .FirstOrDefaultAsync(v => v.PrimaryContactEmail == vendor.PrimaryContactEmail ||
                                     (v.TaxId != null && v.TaxId == vendor.TaxId));

        if (existingVendor != null)
        {
            if (forceCreation)
            {
                 _logger.LogWarning("OVERRULED Duplicate Block. Existing ID: {ExistingId}. Creating new vendor {NewId} anyway.", existingVendor.Id, vendor.Id);
            }
            else
            {
                _logger.LogWarning("Blocked duplicate vendor creation. Existing ID: {ExistingId}", existingVendor.Id);
                return Result.Fail<Vendor>($"Vendor already exists with Email '{vendor.PrimaryContactEmail}' or Tax ID '{vendor.TaxId}'");
            }
        }

        // 2. Sanctions Screening
        var screeningRequest = new VendorMdm.Shared.Models.Sanctions.ScreeningRequest
        {
            VendorId = vendor.Id.ToString(),
            EntityName = vendor.LegalName ?? "",
            EntityType = "Organization",
            Address = new VendorMdm.Shared.Models.Sanctions.AddressInfo { Country = "US" }
        };

        var screeningResult = await _sanctionsService.ScreenEntityAsync(screeningRequest);
        if (screeningResult.OverallRisk == VendorMdm.Shared.Models.Sanctions.RiskLevel.High ||
            screeningResult.OverallRisk == VendorMdm.Shared.Models.Sanctions.RiskLevel.Critical)
        {
            if (forceCreation)
            {
                _logger.LogWarning("OVERRULED Sanctions Block for {LegalName}. Risk Level: {RiskLevel}. ForceCreation was TRUE.", vendor.LegalName, screeningResult.OverallRisk);
            }
            else
            {
                _logger.LogWarning("Blocked vendor creation due to Sanctions Risk: {RiskLevel}", screeningResult.OverallRisk);
                return Result.Fail<Vendor>("High-risk match found in Sanctions Screening. Vendor cannot be created.");
            }
        }

        // ------------------------------------

        // Create domain event for the new vendor
        // Note: AccountGroup is determined by business logic in the Concept layer, default to empty here
        var vendorCreatedEvent = new VendorCreatedEvent(
            vendor.Id,
            vendor.LegalName ?? "",
            "");

        // Add vendor and event to outbox in same transaction (guaranteed delivery)
        _context.Vendors.Add(vendor);
        _context.AddToOutbox(vendorCreatedEvent);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Vendor created in SQL: {Id}", vendor.Id);

        // Dispatch event to in-process handlers (SignalR, etc.)
        await _eventDispatcher.DispatchAsync(vendorCreatedEvent);

        // 2. Functional Log (Cosmos Artifact)
        try
        {
            await _cosmosRepository.SaveArtifactAsync(vendor.Id.ToString(), vendor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save Vendor artifact to Cosmos: {Id}", vendor.Id);
        }

        // 3. Outbound Port (Event Bus) - Legacy Cosmos event log
        try
        {
            var cosmosDomainEvent = new VendorMdm.Shared.Models.DomainEvent
            {
                EventType = "VendorCreated",
                EntityId = vendor.Id.ToString(),
                Data = vendor,
                Timestamp = DateTime.UtcNow,
                Source = "VendorMdm.Api",
                SchemaVersion = vendor.SchemaVersion
            };
            await _cosmosRepository.LogDomainEventAsync(cosmosDomainEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to emit VendorCreated event: {Id}", vendor.Id);
        }

        return Result.Ok(vendor);
    }

    public async Task<Result<Vendor>> UpdateVendorAsync(Vendor vendor)
    {
        // Get the original to detect status changes
        var original = await _context.Vendors
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == vendor.Id);

        var oldStatus = original?.Status;

        // 1. SQL
        vendor.UpdatedAt = DateTime.UtcNow;
        vendor.IncrementVersion();

        _context.Vendors.Update(vendor);

        // Check for status change and create event
        var eventsToDispatch = new List<object>();
        if (oldStatus != null && oldStatus != vendor.Status)
        {
            var statusChangedEvent = new VendorStatusChangedEvent(
                vendor.Id,
                oldStatus,
                vendor.Status ?? "");
            _context.AddToOutbox(statusChangedEvent);
            eventsToDispatch.Add(statusChangedEvent);
        }

        await _context.SaveChangesAsync();

        // Dispatch events to in-process handlers (SignalR, etc.)
        if (eventsToDispatch.Count > 0)
        {
            await _eventDispatcher.DispatchAsync(eventsToDispatch);
        }

        // 2. Artifact
        await _cosmosRepository.SaveArtifactAsync(vendor.Id.ToString(), vendor);

        // 3. Event - Legacy Cosmos event log
        var cosmosDomainEvent = new VendorMdm.Shared.Models.DomainEvent
        {
            EventType = "VendorUpdated",
            EntityId = vendor.Id.ToString(),
            Data = vendor,
            Timestamp = DateTime.UtcNow,
            Source = "VendorMdm.Api",
            SchemaVersion = vendor.SchemaVersion
        };
        await _cosmosRepository.LogDomainEventAsync(cosmosDomainEvent);

        return Result.Ok(vendor);
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
