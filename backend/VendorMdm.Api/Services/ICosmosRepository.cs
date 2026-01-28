using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Services;

public interface ICosmosRepository
{
    Task SaveChangeRequestDataAsync(ChangeRequestData data);
    Task<ChangeRequestData?> GetChangeRequestDataAsync(string requestId);
    Task LogDomainEventAsync(DomainEvent domainEvent);
    Task LogAuditAsync(object auditEntry, string partitionKey);
    Task LogIntegrationEventAsync(object integrationEvent, string partitionKey);
    Task SaveArtifactAsync(string entityId, object artifact);
}
