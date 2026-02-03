using Polly;

namespace VendorMdm.Core.Framework.Resilience;

/// <summary>
/// Core resilience policy service.
/// Provides access to pre-configured Polly policies for circuit breaker, retry, timeout, etc.
/// </summary>
public interface IResiliencePolicy
{
    /// <summary>
    /// Gets a typed policy by name.
    /// </summary>
    IAsyncPolicy<T> GetPolicy<T>(string policyName);

    /// <summary>
    /// Gets a non-typed policy by name.
    /// </summary>
    IAsyncPolicy GetPolicy(string policyName);

    /// <summary>
    /// Executes an action with the specified policy.
    /// </summary>
    Task<T> ExecuteAsync<T>(string policyName, Func<Task<T>> action);

    /// <summary>
    /// Executes an action with the specified policy.
    /// </summary>
    Task ExecuteAsync(string policyName, Func<Task> action);
}
