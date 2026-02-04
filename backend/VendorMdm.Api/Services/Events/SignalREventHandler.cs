using Microsoft.AspNetCore.SignalR;
using VendorMdm.Api.Hubs;
using VendorMdm.Core.Framework.Events;

namespace VendorMdm.Api.Services.Events;

/// <summary>
/// Event handler that pushes domain events to connected frontend clients via SignalR.
/// </summary>
public class SignalREventHandler :
    IEventHandler<VendorCreatedEvent>,
    IEventHandler<VendorStatusChangedEvent>
{
    private readonly IHubContext<EventHub> _hubContext;
    private readonly ILogger<SignalREventHandler> _logger;

    public SignalREventHandler(
        IHubContext<EventHub> hubContext,
        ILogger<SignalREventHandler> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task HandleAsync(VendorCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[SIGNALR] Broadcasting VendorCreated: VendorId={VendorId}, LegalName={LegalName}",
            @event.VendorId,
            @event.LegalName);

        await _hubContext.Clients.Group("all").SendAsync(
            "VendorCreated",
            new
            {
                VendorId = @event.VendorId,
                LegalName = @event.LegalName,
                AccountGroup = @event.AccountGroup,
                EventId = @event.EventId,
                Timestamp = @event.OccurredAt
            },
            cancellationToken);
    }

    public async Task HandleAsync(VendorStatusChangedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[SIGNALR] Broadcasting StatusChanged: VendorId={VendorId}, {OldStatus} → {NewStatus}",
            @event.VendorId,
            @event.OldStatus,
            @event.NewStatus);

        // Broadcast to all connected clients
        await _hubContext.SendStatusChangedAsync(
            "Vendor",
            @event.VendorId.ToString(),
            @event.OldStatus,
            @event.NewStatus);

        // Also send to vendor-specific subscribers
        await _hubContext.Clients.Group($"vendor:{@event.VendorId}").SendAsync(
            "VendorStatusChanged",
            new
            {
                VendorId = @event.VendorId,
                OldStatus = @event.OldStatus,
                NewStatus = @event.NewStatus,
                EventId = @event.EventId,
                Timestamp = @event.OccurredAt
            },
            cancellationToken);
    }
}
