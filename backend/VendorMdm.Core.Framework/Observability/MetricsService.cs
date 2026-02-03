using System.Diagnostics.Metrics;
using System.Collections.Generic;
using System.Linq;

namespace VendorMdm.Core.Framework.Observability;

/// <summary>
/// Interface for metrics collection.
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// Increment a counter metric.
    /// </summary>
    void IncrementCounter(string name, long value = 1, params (string key, object value)[] tags);

    /// <summary>
    /// Record a histogram value.
    /// </summary>
    void RecordHistogram(string name, double value, params (string key, object value)[] tags);

    /// <summary>
    /// Set a gauge value.
    /// </summary>
    void SetGauge(string name, long value, params (string key, object value)[] tags);
}

/// <summary>
/// Implementation of IMetricsService using OpenTelemetry.
/// </summary>
public class MetricsService : IMetricsService
{
    private readonly Meter _meter;
    private readonly Dictionary<string, Counter<long>> _counters = new();
    private readonly Dictionary<string, Histogram<double>> _histograms = new();
    private readonly Dictionary<string, ObservableGauge<long>> _gauges = new();
    private readonly Dictionary<string, long> _gaugeValues = new();

    public MetricsService(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentNullException(nameof(serviceName));

        _meter = new Meter(serviceName, "1.0.0");
    }

    public void IncrementCounter(string name, long value = 1, params (string key, object value)[] tags)
    {
        if (!_counters.ContainsKey(name))
        {
            _counters[name] = _meter.CreateCounter<long>(name);
        }

        var tagArray = tags.Select(t => new KeyValuePair<string, object?>(t.key, t.value)).ToArray();
        _counters[name].Add(value, tagArray);
    }

    public void RecordHistogram(string name, double value, params (string key, object value)[] tags)
    {
        if (!_histograms.ContainsKey(name))
        {
            _histograms[name] = _meter.CreateHistogram<double>(name);
        }

        var tagArray = tags.Select(t => new KeyValuePair<string, object?>(t.key, t.value)).ToArray();
        _histograms[name].Record(value, tagArray);
    }

    public void SetGauge(string name, long value, params (string key, object value)[] tags)
    {
        var gaugeKey = $"{name}_{string.Join("_", tags.Select(t => $"{t.key}={t.value}"))}";
        _gaugeValues[gaugeKey] = value;

        if (!_gauges.ContainsKey(name))
        {
            _gauges[name] = _meter.CreateObservableGauge(name, () =>
            {
                var measurements = new List<Measurement<long>>();
                foreach (var kvp in _gaugeValues.Where(kv => kv.Key.StartsWith(name)))
                {
                    var tagArray = tags.Select(t => new KeyValuePair<string, object?>(t.key, t.value)).ToArray();
                    measurements.Add(new Measurement<long>(kvp.Value, tagArray));
                }
                return measurements;
            });
        }
    }
}

/// <summary>
/// Common business metrics for MDM applications.
/// </summary>
public static class CoreMetrics
{
    // HTTP Metrics
    public const string HttpRequestCount = "http.request.count";
    public const string HttpRequestDuration = "http.request.duration";
    public const string HttpActiveRequests = "http.active_requests";
    public const string HttpErrorCount = "http.error.count";

    // Business Metrics
    public const string VendorsCreated = "vendors.created";
    public const string InvitationsSent = "invitations.sent";
    public const string ApprovalsPending = "approvals.pending";
    public const string DocumentsUploaded = "documents.uploaded";

    // Database Metrics
    public const string DatabaseQueryDuration = "database.query.duration";
    public const string DatabaseConnectionCount = "database.connection.count";

    // External Service Metrics
    public const string SapCallDuration = "sap.call.duration";
    public const string EmailSentCount = "email.sent.count";
}
