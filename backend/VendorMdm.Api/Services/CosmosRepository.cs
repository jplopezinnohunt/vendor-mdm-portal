using Microsoft.Azure.Cosmos;
using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Services;

public class CosmosRepository
{
    private readonly Container _changeRequestContainer;
    private readonly Container _domainEventsContainer;
    private readonly Container _auditLogsContainer;
    private readonly Container _integrationEventsContainer;
    private readonly Container _configurationContainer;

    public CosmosRepository(CosmosClient cosmosClient)
    {
        var database = cosmosClient.GetDatabase("MdmCore");
        _changeRequestContainer = database.GetContainer("ChangeRequestData");
        _domainEventsContainer = database.GetContainer("DomainEvents");
        _auditLogsContainer = database.GetContainer("AuditLogs");
        _integrationEventsContainer = database.GetContainer("IntegrationEvents");
        _configurationContainer = database.GetContainer("Configuration");
    }

    // --- Change Requests ---
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

    // --- Domain Events ---
    public async Task LogDomainEventAsync(DomainEvent domainEvent)
    {
        await _domainEventsContainer.CreateItemAsync(domainEvent, new PartitionKey(domainEvent.EventType));
    }

    // --- Audit Logs ---
    public async Task LogAuditAsync(object auditEntry, string partitionKey)
    {
        await _auditLogsContainer.CreateItemAsync(auditEntry, new PartitionKey(partitionKey));
    }

    // --- Integration Events ---
    public async Task LogIntegrationEventAsync(object integrationEvent, string partitionKey)
    {
        await _integrationEventsContainer.CreateItemAsync(integrationEvent, new PartitionKey(partitionKey));
    }

    /// <summary>
    /// Saves a canonical artifact (snapshot) to the Functional Log.
    /// Used for audit trails and history.
    /// </summary>
    public async Task SaveArtifactAsync(string entityId, object artifact)
    {
        var container = _changeRequestContainer.Database.GetContainer("CanonicalArtifacts");
        await container.UpsertItemAsync(artifact, new PartitionKey(entityId));
    }
}
