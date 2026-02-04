using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace VendorMdm.Api.Hubs;

/// <summary>
/// SignalR Hub for real-time event notifications to frontend clients.
/// Supports group-based notifications for targeted updates.
/// </summary>
[Authorize]
public class EventHub : Hub
{
    private readonly ILogger<EventHub> _logger;

    public EventHub(ILogger<EventHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.Identity?.Name ?? "anonymous";
        _logger.LogInformation(
            "Client connected: ConnectionId={ConnectionId}, User={UserId}",
            Context.ConnectionId,
            userId);

        // Add user to their personal group for targeted notifications
        if (!string.IsNullOrEmpty(userId) && userId != "anonymous")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
        }

        // Add to general broadcast group
        await Groups.AddToGroupAsync(Context.ConnectionId, "all");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.Identity?.Name ?? "anonymous";
        _logger.LogInformation(
            "Client disconnected: ConnectionId={ConnectionId}, User={UserId}, Exception={Exception}",
            Context.ConnectionId,
            userId,
            exception?.Message ?? "None");

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a specific group to receive targeted notifications.
    /// </summary>
    /// <param name="groupName">The group name (e.g., "vendor:guid", "approvers")</param>
    public async Task JoinGroup(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            _logger.LogWarning("Attempted to join empty group");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation(
            "Client {ConnectionId} joined group {GroupName}",
            Context.ConnectionId,
            groupName);
    }

    /// <summary>
    /// Leave a specific group.
    /// </summary>
    /// <param name="groupName">The group name to leave</param>
    public async Task LeaveGroup(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            return;
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation(
            "Client {ConnectionId} left group {GroupName}",
            Context.ConnectionId,
            groupName);
    }

    /// <summary>
    /// Subscribe to vendor-specific updates.
    /// </summary>
    /// <param name="vendorId">The vendor ID to subscribe to</param>
    public async Task SubscribeToVendor(string vendorId)
    {
        if (!Guid.TryParse(vendorId, out _))
        {
            _logger.LogWarning("Invalid vendor ID: {VendorId}", vendorId);
            return;
        }

        await JoinGroup($"vendor:{vendorId}");
    }

    /// <summary>
    /// Unsubscribe from vendor-specific updates.
    /// </summary>
    /// <param name="vendorId">The vendor ID to unsubscribe from</param>
    public async Task UnsubscribeFromVendor(string vendorId)
    {
        await LeaveGroup($"vendor:{vendorId}");
    }
}

/// <summary>
/// Extension methods for sending typed messages through the EventHub.
/// </summary>
public static class EventHubExtensions
{
    /// <summary>
    /// Send a status change notification to all connected clients.
    /// </summary>
    public static async Task SendStatusChangedAsync(
        this IHubContext<EventHub> hubContext,
        string entityType,
        string entityId,
        string oldStatus,
        string newStatus)
    {
        await hubContext.Clients.Group("all").SendAsync("StatusChanged", new
        {
            EntityType = entityType,
            EntityId = entityId,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Send a notification to a specific user.
    /// </summary>
    public static async Task SendNotificationAsync(
        this IHubContext<EventHub> hubContext,
        string userId,
        string title,
        string message,
        string? link = null)
    {
        await hubContext.Clients.Group($"user:{userId}").SendAsync("Notification", new
        {
            Title = title,
            Message = message,
            Link = link,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Send a task assignment notification to a specific user.
    /// </summary>
    public static async Task SendTaskAssignedAsync(
        this IHubContext<EventHub> hubContext,
        string userId,
        string taskType,
        string entityId,
        string description)
    {
        await hubContext.Clients.Group($"user:{userId}").SendAsync("TaskAssigned", new
        {
            TaskType = taskType,
            EntityId = entityId,
            Description = description,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Send SAP sync result to the requestor.
    /// </summary>
    public static async Task SendSapSyncResultAsync(
        this IHubContext<EventHub> hubContext,
        string userId,
        string vendorId,
        bool success,
        string? sapVendorNumber,
        string? errorMessage = null)
    {
        await hubContext.Clients.Group($"user:{userId}").SendAsync("SapSyncResult", new
        {
            VendorId = vendorId,
            Success = success,
            SapVendorNumber = sapVendorNumber,
            ErrorMessage = errorMessage,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Send vendor update to all subscribers of that vendor.
    /// </summary>
    public static async Task SendVendorUpdatedAsync(
        this IHubContext<EventHub> hubContext,
        string vendorId,
        object vendorData)
    {
        await hubContext.Clients.Group($"vendor:{vendorId}").SendAsync("VendorUpdated", new
        {
            VendorId = vendorId,
            Data = vendorData,
            Timestamp = DateTime.UtcNow
        });
    }
}
