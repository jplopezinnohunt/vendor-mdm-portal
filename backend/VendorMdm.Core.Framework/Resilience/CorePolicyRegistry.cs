using Polly;
using Polly.Extensions.Http;
using System.Data.SqlClient;

namespace VendorMdm.Core.Framework.Resilience;

/// <summary>
/// Core policy registry with pre-configured Polly policies.
/// All MDM applications should use these policies for consistency.
/// </summary>
public static class CorePolicyRegistry
{
    // Policy Names
    public const string HttpRetry = "HttpRetry";
    public const string HttpCircuitBreaker = "HttpCircuitBreaker";
    public const string DatabaseRetry = "DatabaseRetry";
    public const string ServiceBusRetry = "ServiceBusRetry";
    public const string DefaultTimeout = "DefaultTimeout";

    /// <summary>
    /// HTTP Retry Policy: 3 retries with exponential backoff.
    /// Use for transient HTTP failures (network blips, timeouts).
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> HttpRetryPolicy =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => (int)msg.StatusCode == 429) // Too Many Requests
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    // Log retry attempt
                    Console.WriteLine($"[Retry {retryCount}] Waiting {timespan.TotalSeconds}s before next attempt");
                });

    /// <summary>
    /// HTTP Circuit Breaker Policy: Opens after 5 consecutive failures, stays open for 30s.
    /// Use to prevent cascading failures when external service is down.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> HttpCircuitBreakerPolicy =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, duration) =>
                {
                    Console.WriteLine($"[Circuit Breaker] OPEN for {duration.TotalSeconds}s");
                },
                onReset: () =>
                {
                    Console.WriteLine($"[Circuit Breaker] CLOSED - Service recovered");
                },
                onHalfOpen: () =>
                {
                    Console.WriteLine($"[Circuit Breaker] HALF-OPEN - Testing service");
                });

    /// <summary>
    /// Combined HTTP Policy: Retry + Circuit Breaker.
    /// Use for all external HTTP calls (SAP, Email, etc.).
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> HttpResilientPolicy =>
        Policy.WrapAsync(HttpCircuitBreakerPolicy, HttpRetryPolicy);

    /// <summary>
    /// Database Retry Policy: 3 retries with exponential backoff.
    /// Use for transient database failures (deadlocks, timeouts).
    /// </summary>
    public static IAsyncPolicy DatabaseRetryPolicy =>
        Policy
            .Handle<SqlException>(ex => IsTransient(ex))
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timespan, retryCount, context) =>
                {
                    Console.WriteLine($"[DB Retry {retryCount}] {exception.Message}");
                });

    /// <summary>
    /// Service Bus Retry Policy: 5 retries with exponential backoff.
    /// Use for Service Bus publish failures.
    /// </summary>
    public static IAsyncPolicy ServiceBusRetryPolicy =>
        Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timespan, retryCount, context) =>
                {
                    Console.WriteLine($"[ServiceBus Retry {retryCount}] {exception.Message}");
                });

    /// <summary>
    /// Default Timeout Policy: 30 seconds.
    /// Use to prevent hanging operations.
    /// </summary>
    public static IAsyncPolicy DefaultTimeoutPolicy =>
        Policy.TimeoutAsync(TimeSpan.FromSeconds(30));

    /// <summary>
    /// Checks if a SQL exception is transient (retriable).
    /// </summary>
    private static bool IsTransient(SqlException ex)
    {
        // Transient error codes
        int[] transientErrorCodes = new[]
        {
            -2,    // Timeout
            -1,    // Connection broken
            1205,  // Deadlock
            1222,  // Lock request timeout
            49918, // Cannot process request
            49919, // Cannot process create/update request
            49920, // Cannot process request - too many operations
            4060,  // Cannot open database
            40197, // Service error
            40501, // Service busy
            40613, // Database unavailable
            49919, // Cannot process request
            49920  // Cannot process request
        };

        return transientErrorCodes.Contains(ex.Number);
    }
}
