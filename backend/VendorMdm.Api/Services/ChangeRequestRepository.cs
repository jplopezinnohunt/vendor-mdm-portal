using VendorMdm.Api.Data;
using VendorMdm.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace VendorMdm.Api.Services;

public interface IChangeRequestRepository
{
    Task<ChangeRequest> CreateRequestAsync(ChangeRequest request, object payload);
    Task<ChangeRequest?> GetRequestAsync(Guid id);
    Task ApproveRequestAsync(Guid id);
}

public class ChangeRequestRepository : IChangeRequestRepository
{
    private readonly SqlDbContext _sqlContext;
    private readonly CosmosRepository _cosmosRepo;
    private readonly ServiceBusService _serviceBus;

    public ChangeRequestRepository(SqlDbContext sqlContext, CosmosRepository cosmosRepo, ServiceBusService serviceBus)
    {
        _sqlContext = sqlContext;
        _cosmosRepo = cosmosRepo;
        _serviceBus = serviceBus;
    }

    public async Task<ChangeRequest> CreateRequestAsync(ChangeRequest request, object payload)
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

        return request;
    }

    public async Task<ChangeRequest?> GetRequestAsync(Guid id)
    {
        // 1. Get Metadata from SQL
        var request = await _sqlContext.ChangeRequests.FindAsync(id);
        if (request == null) return null;

        // 2. Get Payload from Cosmos (Optional: Merge into a DTO if needed, but here returning SQL entity + fetching payload separately if requested)
        // For this example, we aren't merging into a DTO in the repo, but typically you would.
        // Let's assume the Controller handles the DTO composition or we add a property to the entity (not mapped).
        
        return request; 
    }

    public async Task ApproveRequestAsync(Guid id)
    {
        var request = await _sqlContext.ChangeRequests.FindAsync(id);
        if (request == null) throw new KeyNotFoundException("Request not found");

        // 1. Update SQL State
        request.Status = "Approved";
        request.UpdatedAt = DateTime.UtcNow;
        await _sqlContext.SaveChangesAsync();

        // 2. Log Domain Event
        var domainEvent = new DomainEvent
        {
            EventType = "RequestApproved",
            EntityId = id.ToString(),
            Data = new { RequestId = id, Status = "Approved" }
        };
        await _cosmosRepo.LogDomainEventAsync(domainEvent);

        // 3. Publish to Service Bus
        await _serviceBus.PublishEventAsync("RequestApproved", new { RequestId = id, SapVendorId = request.SapVendorId });
    }
}
