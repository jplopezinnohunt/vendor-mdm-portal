using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VendorMdm.Api.Data;
using VendorMdm.Core.Framework.Events;
using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Services.Events;

/// <summary>
/// Background service that processes outbox events for guaranteed delivery.
/// Implements the Outbox Pattern for reliable event publishing.
/// </summary>
public class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(5);
    private readonly int _batchSize = 100;
    private readonly int _maxRetries = 5;

    public OutboxProcessor(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[OUTBOX] OutboxProcessor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingEventsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OUTBOX] Error in OutboxProcessor loop");
            }

            await Task.Delay(_processingInterval, stoppingToken);
        }

        _logger.LogInformation("[OUTBOX] OutboxProcessor stopped");
    }

    private async Task ProcessPendingEventsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SqlDbContext>();
        var serviceBus = scope.ServiceProvider.GetRequiredService<IServiceBusService>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

        // Get pending events that are ready for processing
        var pendingEvents = await dbContext.OutboxEvents
            .Where(e => e.Status == OutboxEventStatus.Pending ||
                       (e.Status == OutboxEventStatus.Failed &&
                        e.RetryCount < _maxRetries &&
                        e.NextRetryAt <= DateTime.UtcNow))
            .OrderBy(e => e.CreatedAt)
            .Take(_batchSize)
            .ToListAsync(cancellationToken);

        if (pendingEvents.Count == 0)
        {
            return;
        }

        _logger.LogInformation("[OUTBOX] Processing {Count} pending events", pendingEvents.Count);

        foreach (var outboxEvent in pendingEvents)
        {
            await ProcessSingleEventAsync(outboxEvent, dbContext, serviceBus, dispatcher, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessSingleEventAsync(
        OutboxEvent outboxEvent,
        SqlDbContext dbContext,
        IServiceBusService serviceBus,
        IDomainEventDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            outboxEvent.Status = OutboxEventStatus.Processing;
            await dbContext.SaveChangesAsync(cancellationToken);

            // Publish to Service Bus for external consumers
            await serviceBus.PublishEventAsync(
                outboxEvent.EventType,
                outboxEvent.Payload,
                outboxEvent.Destination);

            // Mark as completed
            outboxEvent.Status = OutboxEventStatus.Completed;
            outboxEvent.ProcessedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "[OUTBOX] Successfully processed event {EventId}: {EventType}",
                outboxEvent.Id,
                outboxEvent.EventType);
        }
        catch (Exception ex)
        {
            outboxEvent.RetryCount++;
            outboxEvent.LastError = ex.Message.Length > 2000
                ? ex.Message.Substring(0, 2000)
                : ex.Message;

            if (outboxEvent.RetryCount >= _maxRetries)
            {
                outboxEvent.Status = OutboxEventStatus.DeadLettered;
                _logger.LogError(
                    ex,
                    "[OUTBOX] Event {EventId} dead-lettered after {RetryCount} retries: {EventType}",
                    outboxEvent.Id,
                    outboxEvent.RetryCount,
                    outboxEvent.EventType);
            }
            else
            {
                outboxEvent.Status = OutboxEventStatus.Failed;
                // Exponential backoff: 5s, 25s, 125s, 625s, 3125s
                var backoffSeconds = Math.Pow(5, outboxEvent.RetryCount);
                outboxEvent.NextRetryAt = DateTime.UtcNow.AddSeconds(backoffSeconds);

                _logger.LogWarning(
                    ex,
                    "[OUTBOX] Event {EventId} failed (attempt {RetryCount}/{MaxRetries}), next retry at {NextRetry}: {EventType}",
                    outboxEvent.Id,
                    outboxEvent.RetryCount,
                    _maxRetries,
                    outboxEvent.NextRetryAt,
                    outboxEvent.EventType);
            }
        }
    }
}

/// <summary>
/// Extension methods for adding events to the outbox.
/// </summary>
public static class OutboxExtensions
{
    /// <summary>
    /// Adds a domain event to the outbox for guaranteed delivery.
    /// Call this within the same transaction as your entity changes.
    /// </summary>
    public static void AddToOutbox(
        this SqlDbContext dbContext,
        object domainEvent,
        string? correlationId = null,
        string? destination = null)
    {
        var outboxEvent = new OutboxEvent
        {
            EventType = domainEvent.GetType().Name,
            Payload = JsonSerializer.Serialize(domainEvent),
            CorrelationId = correlationId,
            Destination = destination,
            Status = OutboxEventStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.OutboxEvents.Add(outboxEvent);
    }

    /// <summary>
    /// Adds multiple domain events to the outbox.
    /// </summary>
    public static void AddToOutbox(
        this SqlDbContext dbContext,
        IEnumerable<object> domainEvents,
        string? correlationId = null,
        string? destination = null)
    {
        foreach (var domainEvent in domainEvents)
        {
            dbContext.AddToOutbox(domainEvent, correlationId, destination);
        }
    }
}
