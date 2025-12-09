using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;

namespace VendorMdm.Api.Services;

public class ServiceBusService
{
    private readonly ServiceBusClient _client;
    private readonly string _sapEnvironmentCode;
    private readonly Dictionary<string, ServiceBusSender> _senders;

    public ServiceBusService(ServiceBusClient client, IConfiguration configuration)
    {
        _client = client;
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

    private string GetQueueNameForEvent(string eventType)
    {
        // Route different events to different queues/topics
        return eventType switch
        {
            "invitation-created" => "invitation-emails",
            "vendor-application-submitted" => "vendor-changes",
            "vendor-change-request" => "vendor-changes",
            _ => "vendor-changes" // Default queue
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
