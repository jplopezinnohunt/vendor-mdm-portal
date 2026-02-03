using VendorMdm.Core.Framework.Primitives;

namespace VendorMdm.Core.Framework.HealthChecks;

/// <summary>
/// Core health check service for all MDM applications.
/// Provides health checks for database, blob storage, service bus, and external services.
/// </summary>
public interface IHealthCheckService
{
    /// <summary>
    /// Checks database connectivity and health.
    /// </summary>
    Task<Result<HealthCheckResult>> CheckDatabaseAsync();

    /// <summary>
    /// Checks blob storage connectivity and health.
    /// </summary>
    Task<Result<HealthCheckResult>> CheckBlobStorageAsync();

    /// <summary>
    /// Checks service bus connectivity and health.
    /// </summary>
    Task<Result<HealthCheckResult>> CheckServiceBusAsync();

    /// <summary>
    /// Checks external service connectivity and health.
    /// </summary>
    Task<Result<HealthCheckResult>> CheckExternalServiceAsync(string serviceName, string endpoint);

    /// <summary>
    /// Runs all health checks and returns overall status.
    /// </summary>
    Task<Result<OverallHealthStatus>> CheckAllAsync();
}

/// <summary>
/// Health check result for a single component.
/// </summary>
public record HealthCheckResult
{
    public required string ComponentName { get; init; }
    public required HealthStatus Status { get; init; }
    public required string? Description { get; init; }
    public required TimeSpan ResponseTime { get; init; }
    public required DateTime CheckedAt { get; init; }
    public Dictionary<string, object>? Data { get; init; }
}

/// <summary>
/// Overall health status for all components.
/// </summary>
public record OverallHealthStatus
{
    public required HealthStatus Status { get; init; }
    public required List<HealthCheckResult> ComponentResults { get; init; }
    public required DateTime CheckedAt { get; init; }
    public required TimeSpan TotalResponseTime { get; init; }
}

/// <summary>
/// Health status enum.
/// </summary>
public enum HealthStatus
{
    Healthy,
    Degraded,
    Unhealthy
}
