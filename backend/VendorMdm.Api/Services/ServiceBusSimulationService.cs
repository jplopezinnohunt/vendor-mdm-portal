using System.Text.Json;
using VendorMdm.Shared.DomainEvents;

namespace VendorMdm.Api.Services;

public class ServiceBusSimulationService : IServiceBusService
{
    private readonly ILogger<ServiceBusSimulationService> _logger;

    public ServiceBusSimulationService(ILogger<ServiceBusSimulationService> logger)
    {
        _logger = logger;
    }

    public Task PublishEventAsync(string eventType, object data, string? queueName = null)
    {
        // Simulation: Just log that we "would have" sent the event
        _logger.LogInformation("📢 [SIMULATION] Publishing Event to Service Bus:");
        _logger.LogInformation("   Type: {EventType}", eventType);
        _logger.LogInformation("   Queue: {Queue}", queueName ?? "domain-events");
        
        try 
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            _logger.LogInformation("   Payload: {Payload}", json);
        }
        catch
        {
            _logger.LogInformation("   Payload: [Serialization Failed]");
        }

        return Task.CompletedTask;
    }

    public Task<bool> TestConnectionAsync()
    {
        _logger.LogInformation("📢 [SIMULATION] Testing Service Bus connection (always success)");
        return Task.FromResult(true);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
