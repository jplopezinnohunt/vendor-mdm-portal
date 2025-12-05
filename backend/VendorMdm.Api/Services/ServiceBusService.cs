using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;

namespace VendorMdm.Api.Services;

public class ServiceBusService
{
    private readonly ServiceBusSender _sender;
    private readonly string _sapEnvironmentCode;

    public ServiceBusService(ServiceBusClient client, IConfiguration configuration)
    {
        _sender = client.CreateSender("vendor-changes");
        _sapEnvironmentCode = configuration["SapEnvironmentCode"] ?? "D01";
    }

    public async Task PublishEventAsync(string eventType, object data)
    {
        var messageBody = JsonConvert.SerializeObject(new { EventType = eventType, Data = data });
        var message = new ServiceBusMessage(messageBody);

        // Add Context for Routing
        message.ApplicationProperties.Add("sapEnvironmentCode", _sapEnvironmentCode);
        message.ApplicationProperties.Add("eventType", eventType);

        await _sender.SendMessageAsync(message);
    }
}
