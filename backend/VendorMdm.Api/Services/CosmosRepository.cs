using Microsoft.Azure.Cosmos;
using VendorMdm.Shared.Models;

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

    /// <summary>
    /// Saves a canonical artifact (snapshot) to the Functional Log.
    /// Used for audit trails and history.
    /// </summary>
    public async Task SaveArtifactAsync(string entityId, object artifact)
    {
        var container = _changeRequestContainer.Database.GetContainer("CanonicalArtifacts");
        // Note: Using dynamic container retrieval or we should add a field for it
        // Ideally add _canonicalArtifactsContainer field in constructor.
        // For now, lazily get it or assume it exists. To be safe, let's use a field.
        await container.UpsertItemAsync(artifact, new PartitionKey(entityId));
    }
}
