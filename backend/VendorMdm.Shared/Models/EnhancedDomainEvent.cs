using Newtonsoft.Json;

namespace VendorMdm.Shared.Models;

/// <summary>
/// Enhanced Domain Event with correlation tracking, actor, and channel.
/// Replaces basic DomainEvent with canonical requirements.
/// </summary>
public class EnhancedDomainEvent
{
    /// <summary>
    /// Unique event identifier
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Event type (partition key in Cosmos)
    /// Examples: VendorCreated, VendorUpdated, InvitationCreated
    /// </summary>
    [JsonProperty("eventType")]
    public string EventType { get; set; } = string.Empty;
    
    /// <summary>
    /// Entity ID that this event relates to
    /// </summary>
    [JsonProperty("entityId")]
    public string EntityId { get; set; } = string.Empty;
    
    /// <summary>
    /// Correlation ID for distributed tracing
    /// Links all events in a single request/transaction
    /// MANDATORY for canonical model
    /// </summary>
    [JsonProperty("correlationId")]
    public string CorrelationId { get; set; } = string.Empty;
    
    /// <summary>
    /// Causation ID - the event that caused this event
    /// Used for event chains (Event A → Event B → Event C)
    /// Optional: only needed for event-driven workflows
    /// </summary>
    [JsonProperty("causationId")]
    public string? CausationId { get; set; }
    
    /// <summary>
    /// Actor who triggered this event (user ID, system name)
    /// MANDATORY for audit trail
    /// </summary>
    [JsonProperty("actor")]
    public string Actor { get; set; } = "System";
    
    /// <summary>
    /// Channel through which event was triggered
    /// Values: Portal, API, SAP, Batch, Scheduler
    /// MANDATORY for canonical model
    /// </summary>
    [JsonProperty("channel")]
    public string Channel { get; set; } = "Portal";
    
    /// <summary>
    /// Event timestamp (UTC)
    /// </summary>
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Entity version at time of event
    /// Enables event replay and state reconstruction
    /// </summary>
    [JsonProperty("entityVersion")]
    public int EntityVersion { get; set; } = 1;
    
    /// <summary>
    /// Event payload data
    /// </summary>
    [JsonProperty("data")]
    public object Data { get; set; } = new object();
    
    /// <summary>
    /// Sequence number for ordering events (optional)
    /// Used for event replay
    /// </summary>
    [JsonProperty("sequenceNumber")]
    public long? SequenceNumber { get; set; }
    
    /// <summary>
    /// Flag indicating if this is a replayed event
    /// </summary>
    [JsonProperty("isReplay")]
    public bool IsReplay { get; set; } = false;
}

/// <summary>
/// Valid event channels
/// </summary>
public static class EventChannels
{
    public const string Portal = "Portal";
    public const string Api = "API";
    public const string Sap = "SAP";
    public const string Batch = "Batch";
    public const string Scheduler = "Scheduler";
    public const string Migration = "Migration";
    
    public static readonly string[] All = { Portal, Api, Sap, Batch, Scheduler, Migration };
}
