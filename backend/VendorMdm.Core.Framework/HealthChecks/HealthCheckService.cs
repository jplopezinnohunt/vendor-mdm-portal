using Microsoft.Extensions.Diagnostics.HealthChecks;
using VendorMdm.Core.Framework.Primitives;
using VendorMdm.Core.Framework.Logging;
using System.Diagnostics;

namespace VendorMdm.Core.Framework.HealthChecks;

/// <summary>
/// Implementation of IHealthCheckService using ASP.NET Core Health Checks.
/// </summary>
public class CoreHealthCheckService : IHealthCheckService
{
    private readonly Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService _aspNetHealthCheckService;
    private readonly IStructuredLogger _logger;

    public CoreHealthCheckService(
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService aspNetHealthCheckService,
        IStructuredLogger logger)
    {
        _aspNetHealthCheckService = aspNetHealthCheckService ?? throw new ArgumentNullException(nameof(aspNetHealthCheckService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<HealthCheckResult>> CheckDatabaseAsync()
    {
        return await CheckComponentAsync("database");
    }

    public async Task<Result<HealthCheckResult>> CheckBlobStorageAsync()
    {
        return await CheckComponentAsync("blob-storage");
    }

    public async Task<Result<HealthCheckResult>> CheckServiceBusAsync()
    {
        return await CheckComponentAsync("service-bus");
    }

    public async Task<Result<HealthCheckResult>> CheckExternalServiceAsync(string serviceName, string endpoint)
    {
        using var _ = _logger.BeginOperation("CheckExternalService", ("ServiceName", serviceName), ("Endpoint", endpoint));

        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Perform HTTP health check
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await httpClient.GetAsync(endpoint);
            
            stopwatch.Stop();

            var status = response.IsSuccessStatusCode ? HealthStatus.Healthy : HealthStatus.Unhealthy;
            var description = response.IsSuccessStatusCode 
                ? "Service is healthy" 
                : $"Service returned {response.StatusCode}";

            var result = new HealthCheckResult
            {
                ComponentName = serviceName,
                Status = status,
                Description = description,
                ResponseTime = stopwatch.Elapsed,
                CheckedAt = DateTime.UtcNow,
                Data = new Dictionary<string, object>
                {
                    ["Endpoint"] = endpoint,
                    ["StatusCode"] = (int)response.StatusCode
                }
            };

            _logger.LogInformation("External service check completed", 
                ("ServiceName", serviceName), 
                ("Status", status.ToString()));

            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External service check failed", ("ServiceName", serviceName));
            
            return Result.Ok(new HealthCheckResult
            {
                ComponentName = serviceName,
                Status = HealthStatus.Unhealthy,
                Description = $"Check failed: {ex.Message}",
                ResponseTime = TimeSpan.Zero,
                CheckedAt = DateTime.UtcNow
            });
        }
    }

    public async Task<Result<OverallHealthStatus>> CheckAllAsync()
    {
        using var _ = _logger.BeginOperation("CheckAllHealth");

        try
        {
            var overallStopwatch = Stopwatch.StartNew();
            var results = new List<HealthCheckResult>();

            // Check database
            var dbResult = await CheckDatabaseAsync();
            if (dbResult.IsSuccess)
                results.Add(dbResult.Value);

            // Check blob storage
            var blobResult = await CheckBlobStorageAsync();
            if (blobResult.IsSuccess)
                results.Add(blobResult.Value);

            // Check service bus
            var sbResult = await CheckServiceBusAsync();
            if (sbResult.IsSuccess)
                results.Add(sbResult.Value);

            overallStopwatch.Stop();

            // Determine overall status
            var overallStatus = results.All(r => r.Status == HealthStatus.Healthy)
                ? HealthStatus.Healthy
                : results.Any(r => r.Status == HealthStatus.Unhealthy)
                    ? HealthStatus.Unhealthy
                    : HealthStatus.Degraded;

            var healthStatus = new OverallHealthStatus
            {
                Status = overallStatus,
                ComponentResults = results,
                CheckedAt = DateTime.UtcNow,
                TotalResponseTime = overallStopwatch.Elapsed
            };

            _logger.LogInformation("Overall health check completed", 
                ("Status", overallStatus.ToString()), 
                ("ComponentCount", results.Count));

            return Result.Ok(healthStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Overall health check failed");
            return Result.Fail<OverallHealthStatus>("Health check failed");
        }
    }

    private async Task<Result<HealthCheckResult>> CheckComponentAsync(string componentName)
    {
        using var _ = _logger.BeginOperation("CheckComponent", ("Component", componentName));

        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Use ASP.NET Core Health Check service
            var healthReport = await _aspNetHealthCheckService.CheckHealthAsync(
                registration => registration.Name == componentName);
            
            stopwatch.Stop();

            var entry = healthReport.Entries.FirstOrDefault(e => e.Key == componentName);
            
            if (!healthReport.Entries.ContainsKey(componentName))
            {
                _logger.LogWarning("Component not found in health checks", ("Component", componentName));
                return Result.Fail<HealthCheckResult>($"Component '{componentName}' not found");
            }

            var status = entry.Value.Status switch
            {
                Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy => HealthStatus.Healthy,
                Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded => HealthStatus.Degraded,
                _ => HealthStatus.Unhealthy
            };

            var result = new HealthCheckResult
            {
                ComponentName = componentName,
                Status = status,
                Description = entry.Value.Description ?? "No description",
                ResponseTime = entry.Value.Duration,
                CheckedAt = DateTime.UtcNow,
                Data = entry.Value.Data?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };

            _logger.LogInformation("Component check completed", 
                ("Component", componentName), 
                ("Status", status.ToString()));

            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Component check failed", ("Component", componentName));
            
            return Result.Ok(new HealthCheckResult
            {
                ComponentName = componentName,
                Status = HealthStatus.Unhealthy,
                Description = $"Check failed: {ex.Message}",
                ResponseTime = TimeSpan.Zero,
                CheckedAt = DateTime.UtcNow
            });
        }
    }
}
