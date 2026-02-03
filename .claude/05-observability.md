# Observability Standards

---

## 5.1 Health Endpoints (MANDATORY)

### Required Endpoints
```csharp
app.MapHealthChecks("/health");          // Overall health
app.MapHealthChecks("/health/ready");    // Readiness probe (Kubernetes)
app.MapHealthChecks("/health/live");     // Liveness probe (Kubernetes)
```

### Implementation
```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<SqlDbContext>("database")
    .AddCheck("ready", () => HealthCheckResult.Healthy());
```

### Health Checks
- Database connectivity
- External service availability (SAP, if applicable)
- Disk space
- Memory usage

### Response Format
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0123456",
  "entries": {
    "database": {
      "status": "Healthy",
      "duration": "00:00:00.0100000"
    }
  }
}
```

---

## 5.2 Structured Logging

### Interface
```csharp
public interface IStructuredLogger
{
    void LogInformation(string message, params (string key, object value)[] properties);
    void LogWarning(string message, params (string key, object value)[] properties);
    void LogError(Exception ex, string message, params (string key, object value)[] properties);
    void LogDebug(string message, params (string key, object value)[] properties);
    void LogCritical(Exception ex, string message, params (string key, object value)[] properties);
    IDisposable BeginScope(params (string key, object value)[] properties);
    IDisposable BeginOperation(string operationName, params (string key, object value)[] properties);
}
```

### Usage Examples
```csharp
// Simple logging
_logger.LogInformation("Vendor created",
    ("VendorId", vendor.Id),
    ("LegalName", vendor.LegalName));

// Error logging
_logger.LogError(ex, "Failed to sync vendor",
    ("VendorId", vendorId),
    ("ErrorCode", ex.HResult));

// Scope (all logs include these properties)
using (_logger.BeginScope(("UserId", userId), ("RequestId", requestId)))
{
    _logger.LogInformation("Processing started");
    // ... business logic ...
    _logger.LogInformation("Processing completed");
}

// Operation scope (auto-timing)
using (_logger.BeginOperation("ProcessPayment", ("Amount", amount)))
{
    // ... business logic ...
    // Automatically logs: "[START] ProcessPayment {Amount=100}"
    // Automatically logs: "[END] ProcessPayment {Amount=100, Duration=1234ms}"
}
```

### Contextual Properties
Always include:
- `UserId` - Who performed the action
- `VendorId` - Related vendor (if applicable)
- `InvitationId` - Related invitation (if applicable)
- `TraceId` - Distributed trace ID
- `SpanId` - Current span ID

---

## 5.3 TraceId Propagation

### Requirements
- TraceId in all log statements
- TraceId in HTTP response headers
- TraceId in error responses
- TraceId in frontend UI overlay

### Implementation
```csharp
// Middleware to add TraceId to response headers
public class TraceIdMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = Activity.Current?.TraceId.ToString()
            ?? context.TraceIdentifier;

        context.Response.Headers.Add("X-Trace-Id", traceId);
        await _next(context);
    }
}
```

### Error Response Format
```json
{
  "error": "Validation failed",
  "traceId": "00-abc123def456-789012345678-01",
  "timestamp": "2026-02-03T12:00:00Z"
}
```

---

## 5.4 OpenTelemetry Integration

### Packages Required
- `OpenTelemetry.Extensions.Hosting`
- `OpenTelemetry.Instrumentation.AspNetCore`
- `OpenTelemetry.Instrumentation.Http`
- `OpenTelemetry.Instrumentation.SqlClient`
- `Azure.Monitor.OpenTelemetry.Exporter`

### Configuration
```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(builder =>
    {
        builder
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSqlClientInstrumentation()
            .AddAzureMonitorTraceExporter(options =>
            {
                options.ConnectionString = config["ApplicationInsights:ConnectionString"];
            });
    })
    .WithMetrics(builder =>
    {
        builder
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddAzureMonitorMetricExporter(options =>
            {
                options.ConnectionString = config["ApplicationInsights:ConnectionString"];
            });
    });
```

---

## 5.5 Metrics Collection

### Business Metrics
| Metric | Type | Description |
|--------|------|-------------|
| `vendors_created_total` | Counter | Total vendors created |
| `invitations_sent_total` | Counter | Total invitations sent |
| `approvals_pending` | Gauge | Current pending approvals |
| `request_duration_seconds` | Histogram | API request duration |
| `active_requests` | Gauge | Currently active requests |
| `error_total` | Counter | Total errors |

### Custom Meter
```csharp
public class VendorMetrics
{
    private static readonly Counter<long> VendorsCreated =
        Meter.CreateCounter<long>("vendors_created_total");

    private static readonly Histogram<double> RequestDuration =
        Meter.CreateHistogram<double>("request_duration_seconds");

    public void RecordVendorCreated() => VendorsCreated.Add(1);
    public void RecordRequestDuration(double seconds) => RequestDuration.Record(seconds);
}
```

---

## 5.6 Simulation Mode

### Logging Pattern for Mocks
```csharp
// Always log when in simulation mode
_logger.LogWarning("[SIMULATION MODE] SAP sync skipped for vendor {VendorId}", vendorId);
_logger.LogWarning("[SIMULATION MODE] Email not sent to {Email}", email);
```

### Detection
```csharp
if (_configuration.GetValue<bool>("Features:UseSapSimulation"))
{
    _logger.LogWarning("[SIMULATION MODE] Using SAP mock");
    return SimulatedSapResponse();
}
```
