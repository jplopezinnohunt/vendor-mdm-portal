using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace VendorMdm.Core.Framework.Observability;

/// <summary>
/// Interface for distributed tracing operations.
/// </summary>
public interface IDistributedTracing
{
    /// <summary>
    /// Start a new activity (span) for an operation.
    /// </summary>
    Activity? StartActivity(string operationName, ActivityKind kind = ActivityKind.Internal);

    /// <summary>
    /// Get the current trace ID.
    /// </summary>
    string? GetCurrentTraceId();

    /// <summary>
    /// Get the current span ID.
    /// </summary>
    string? GetCurrentSpanId();

    /// <summary>
    /// Add a tag to the current activity.
    /// </summary>
    void AddTag(string key, object value);

    /// <summary>
    /// Add an event to the current activity.
    /// </summary>
    void AddEvent(string name, params (string key, object value)[] attributes);

    /// <summary>
    /// Record an exception in the current activity.
    /// </summary>
    void RecordException(Exception exception);
}

/// <summary>
/// Implementation of IDistributedTracing using OpenTelemetry.
/// </summary>
public class DistributedTracingService : IDistributedTracing
{
    private readonly ActivitySource _activitySource;

    public DistributedTracingService(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentNullException(nameof(serviceName));

        _activitySource = new ActivitySource(serviceName, "1.0.0");
    }

    public Activity? StartActivity(string operationName, ActivityKind kind = ActivityKind.Internal)
    {
        return _activitySource.StartActivity(operationName, kind);
    }

    public string? GetCurrentTraceId()
    {
        return Activity.Current?.TraceId.ToString();
    }

    public string? GetCurrentSpanId()
    {
        return Activity.Current?.SpanId.ToString();
    }

    public void AddTag(string key, object value)
    {
        Activity.Current?.SetTag(key, value);
    }

    public void AddEvent(string name, params (string key, object value)[] attributes)
    {
        if (Activity.Current == null) return;

        var tags = new ActivityTagsCollection();
        foreach (var (key, value) in attributes)
        {
            tags.Add(key, value);
        }

        Activity.Current.AddEvent(new ActivityEvent(name, tags: tags));
    }

    public void RecordException(Exception exception)
    {
        if (Activity.Current == null) return;

        Activity.Current.SetStatus(ActivityStatusCode.Error, exception.Message);
        Activity.Current.RecordException(exception);
    }
}

/// <summary>
/// Configuration options for distributed tracing.
/// </summary>
public class DistributedTracingOptions
{
    public required string ServiceName { get; set; }
    public required string ServiceVersion { get; set; }
    public string? ApplicationInsightsConnectionString { get; set; }
    public bool EnableConsoleExporter { get; set; } = false;
}
