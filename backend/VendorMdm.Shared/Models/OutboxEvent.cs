using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendorMdm.Shared.Models;

/// <summary>
/// Outbox pattern entity for guaranteed event delivery.
/// Events are stored in this table as part of the same transaction as the entity change,
/// then processed asynchronously by a background worker.
/// </summary>
[Table("OutboxEvents")]
public class OutboxEvent
{
    /// <summary>
    /// Unique identifier for the outbox entry.
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Type of the domain event (e.g., "VendorCreatedEvent", "VendorStatusChangedEvent").
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// JSON serialized event payload.
    /// </summary>
    [Required]
    public string Payload { get; set; } = "{}";

    /// <summary>
    /// Processing status of the event.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = OutboxEventStatus.Pending;

    /// <summary>
    /// Correlation ID for distributed tracing.
    /// </summary>
    [MaxLength(100)]
    public string? CorrelationId { get; set; }

    /// <summary>
    /// When the event was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the event was successfully processed.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Number of processing attempts.
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Last error message if processing failed.
    /// </summary>
    [MaxLength(2000)]
    public string? LastError { get; set; }

    /// <summary>
    /// When to next attempt processing (for exponential backoff).
    /// </summary>
    public DateTime? NextRetryAt { get; set; }

    /// <summary>
    /// Optional: Target destination (queue name, topic, etc.)
    /// </summary>
    [MaxLength(200)]
    public string? Destination { get; set; }
}

/// <summary>
/// Valid status values for outbox events.
/// </summary>
public static class OutboxEventStatus
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
    public const string DeadLettered = "DeadLettered";

    public static readonly string[] All = { Pending, Processing, Completed, Failed, DeadLettered };
}
