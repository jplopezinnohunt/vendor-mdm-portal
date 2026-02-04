using Microsoft.EntityFrameworkCore;
using VendorMdm.Api.Data;
using VendorMdm.Core.Framework.Primitives;
using VendorMdm.Shared.Constants;
using VendorMdm.Shared.Models; // SQL and Cosmos entities

namespace VendorMdm.Api.Services;

public interface IChangeRequestRepository
{
    Task<Result<ChangeRequest>> CreateRequestAsync(ChangeRequest request, object payload);
    Task<Result<ChangeRequest>> GetRequestAsync(Guid id);
    Task<Result> ApproveRequestAsync(Guid id);

    // Feature: Overlay Logic (SAP + Pending Changes)
    Task<Result<object>> GetEffectiveVendorStateAsync(string sapVendorId);
}

public class ChangeRequestRepository : IChangeRequestRepository
{
    private readonly SqlDbContext _sqlContext;
    private readonly CosmosRepository _cosmosRepo;
    private readonly IServiceBusService _serviceBus;

    public ChangeRequestRepository(SqlDbContext sqlContext, CosmosRepository cosmosRepo, IServiceBusService serviceBus)
    {
        _sqlContext = sqlContext;
        _cosmosRepo = cosmosRepo;
        _serviceBus = serviceBus;
    }

    public async Task<Result<ChangeRequest>> CreateRequestAsync(ChangeRequest request, object payload)
    {
        // 1. Save Metadata to SQL
        _sqlContext.ChangeRequests.Add(request);
        await _sqlContext.SaveChangesAsync();

        // 2. Save Payload to Cosmos
        var cosmosData = new ChangeRequestData
        {
            Id = request.Id.ToString(),
            RequestId = request.Id.ToString(),
            Payload = payload,
            NewValue = payload
        };
        await _cosmosRepo.SaveChangeRequestDataAsync(cosmosData);

        return Result.Ok(request);
    }

    public async Task<Result<ChangeRequest>> GetRequestAsync(Guid id)
    {
        // 1. Get Metadata from SQL
        var request = await _sqlContext.ChangeRequests.FindAsync(id);
        if (request == null)
            return Result.Fail<ChangeRequest>($"Change request with ID '{id}' not found.");

        // 2. Get Payload from Cosmos (Optional: Merge into a DTO if needed, but here returning SQL entity + fetching payload separately if requested)
        // For this example, we aren't merging into a DTO in the repo, but typically you would.
        // Let's assume the Controller handles the DTO composition or we add a property to the entity (not mapped).

        return Result.Ok(request);
    }

    public async Task<Result> ApproveRequestAsync(Guid id)
    {
        var request = await _sqlContext.ChangeRequests.FindAsync(id);
        if (request == null)
            return Result.Fail($"Change request with ID '{id}' not found.");

        // Validate state transition using state machine
        if (!ChangeRequestStatus.CanApprove(request.Status))
            return Result.Fail($"Cannot approve change request in status '{request.Status}'. Allowed from: Submitted, UnderReview.");

        if (!ChangeRequestStatus.IsValidTransition(request.Status, ChangeRequestStatus.Approved))
            return Result.Fail($"Invalid transition from '{request.Status}' to 'Approved'.");

        // 1. Update SQL State
        request.Status = ChangeRequestStatus.Approved;
        request.UpdatedAt = DateTime.UtcNow;
        await _sqlContext.SaveChangesAsync();

        // 2. Log Domain Event
        var domainEvent = new DomainEvent
        {
            EventType = "RequestApproved",
            EntityId = id.ToString(),
            Data = new { RequestId = id, Status = "Approved" },
            SchemaVersion = "v1.0.0"
        };
        await _cosmosRepo.LogDomainEventAsync(domainEvent);

        // 3. Publish to Service Bus
        await _serviceBus.PublishEventAsync("RequestApproved", new { RequestId = id, SapVendorId = request.SapVendorId });

        return Result.Ok();
    }

    /// <summary>
    /// Gets the "Effective" state of a vendor by overlaying pending changes on top of SAP master data.
    /// Scenario: User changed name to "NewName" (Pending). SAP has "OldName".
    /// Result: "NewName" is shown.
    /// </summary>
    public async Task<Result<object>> GetEffectiveVendorStateAsync(string sapVendorId)
    {
        // 1. Fetch Master Data from SAP (Mocked for now)
        // In real impl, this would call ISapService
        var sapMasterData = new
        {
            VendorId = sapVendorId,
            LegalName = "Acme Corp (SAP Legacy)",
            Status = "Active",
            Region = "US"
        };

        // 2. Find LATEST Pending Change Request for this SAP Vendor
        var pendingRequest = await _sqlContext.ChangeRequests
            .Where(r => r.SapVendorId == sapVendorId && r.Status == ChangeRequestStatus.Submitted)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        if (pendingRequest == null)
        {
            // No pending changes - return SAP data as is
            return Result.Ok<object>(sapMasterData);
        }

        // 3. Fetch the flexible payload from Cosmos
        // Note: ChangeRequestData.Payload is typically JObject/JsonElement
        var changeData = await _cosmosRepo.GetChangeRequestDataAsync(pendingRequest.Id.ToString());

        if (changeData?.Payload == null)
        {
            return Result.Ok<object>(sapMasterData);
        }

        // 4. Overlay Logic (Simple Merge)
        // In a real implementation with JObject, use Merge(MergeSettings).
        // For this demo, we return a composite object to show the concept.

        return Result.Ok<object>(new
        {
            Meta = new { IsOverlay = true, PendingRequestId = pendingRequest.Id },
            Base = sapMasterData,
            Overlay = changeData.Payload,
            // "Effective" would be the Merged view
            Effective = MergeObjects(sapMasterData, changeData.Payload)
        });
    }

    private object MergeObjects(object original, object delta)
    {
        // Placeholder for deep merge logic (e.g. using Newtonsoft.Json.Linq.JObject.Merge)
        // Returning delta for now to simulate "Last Write Wins" on the whole object
        return delta;
    }
}
