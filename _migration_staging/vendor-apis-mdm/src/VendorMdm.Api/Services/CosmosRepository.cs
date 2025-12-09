using Microsoft.Azure.Cosmos;
using VendorMdm.Api.Models;

namespace VendorMdm.Api.Services;

public class CosmosRepository
{
    private readonly Container _changeRequestContainer;
    private readonly Container _domainEventsContainer;

    public CosmosRepository(CosmosClient cosmosClient)
    {
        var database = cosmosClient.GetDatabase("MdmCore");
        _changeRequestContainer = database.GetContainer("ChangeRequestData");
        _domainEventsContainer = database.GetContainer("DomainEvents");
    }

    public async Task SaveChangeRequestDataAsync(ChangeRequestData data)
    {
        await _changeRequestContainer.UpsertItemAsync(data, new PartitionKey(data.RequestId));
    }

    public async Task<ChangeRequestData?> GetChangeRequestDataAsync(string requestId)
    {
        try
        {
            ItemResponse<ChangeRequestData> response = await _changeRequestContainer.ReadItemAsync<ChangeRequestData>(requestId, new PartitionKey(requestId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task LogDomainEventAsync(DomainEvent domainEvent)
    {
        await _domainEventsContainer.CreateItemAsync(domainEvent, new PartitionKey(domainEvent.EventType));
    }
}
