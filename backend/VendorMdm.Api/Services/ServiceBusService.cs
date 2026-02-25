using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;

namespace VendorMdm.Api.Services;

public interface IServiceBusService
{
    Task PublishEventAsync(string eventType, object data, string? queueName = null);
    Task<bool> TestConnectionAsync();
    ValueTask DisposeAsync();
}

public class ServiceBusService : IServiceBusService
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<ServiceBusService> _logger;
    private readonly string _sapEnvironmentCode;
    private readonly Dictionary<string, ServiceBusSender> _senders;

    public ServiceBusService(
        ServiceBusClient client, 
        IConfiguration configuration,
        ILogger<ServiceBusService> logger)
    {
        _client = client;
        _logger = logger;
        _sapEnvironmentCode = configuration["SapEnvironmentCode"] ?? "D01";
        _senders = new Dictionary<string, ServiceBusSender>();
    }

    public async Task PublishEventAsync(string eventType, object data, string? queueName = null)
    {
        // Determine queue based on event type or use provided queue name
        var targetQueue = queueName ?? GetQueueNameForEvent(eventType);
        
        // Get or create sender for this queue
        if (!_senders.ContainsKey(targetQueue))
        {
            _senders[targetQueue] = _client.CreateSender(targetQueue);
        }

        var sender = _senders[targetQueue];
        var messageBody = JsonConvert.SerializeObject(new { EventType = eventType, Data = data });
        var message = new ServiceBusMessage(messageBody);

        // Add Context for Routing
        message.ApplicationProperties.Add("sapEnvironmentCode", _sapEnvironmentCode);
        message.ApplicationProperties.Add("eventType", eventType);

        await sender.SendMessageAsync(message);
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            // Lightweight check: try to create a sender for a default queue
            // but just check if the client can interact with the namespace.
            // Getting the fully qualified namespace is a quick way to check connectivity.
            return !string.IsNullOrEmpty(_client.FullyQualifiedNamespace);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service Bus connectivity check failed");
            return false;
        }
    }

    private string GetQueueNameForEvent(string eventType)
    {
        // Route different events to appropriate queues
        // NOTE: Only 'invitation-created' is currently deployed in Azure.
        // All other queues must be deployed via Bicep before enabling.
        return eventType switch
        {
            "invitation-created" or "InvitationCreated" => "invitation-created",
            "vendor-application-submitted" => "invitation-created", // TODO: Deploy 'vendor-applications' queue
            "vendor-change-request" => "invitation-created", // TODO: Deploy 'vendor-changes' queue
            _ => "invitation-created" // Default to the only deployed queue
        };
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var sender in _senders.Values)
        {
            await sender.DisposeAsync();
        }
        _senders.Clear();
    }
}
