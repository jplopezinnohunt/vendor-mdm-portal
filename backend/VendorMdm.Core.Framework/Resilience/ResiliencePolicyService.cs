using Polly;

namespace VendorMdm.Core.Framework.Resilience;

/// <summary>
/// Implementation of IResiliencePolicy that provides access to core policies.
/// </summary>
public class ResiliencePolicyService : IResiliencePolicy
{
    private readonly Dictionary<string, object> _policies = new();

    public ResiliencePolicyService()
    {
        // Register core policies (store as object to avoid type issues)
        _policies[CorePolicyRegistry.HttpRetry] = CorePolicyRegistry.HttpRetryPolicy;
        _policies[CorePolicyRegistry.HttpCircuitBreaker] = CorePolicyRegistry.HttpCircuitBreakerPolicy;
        _policies[CorePolicyRegistry.DatabaseRetry] = CorePolicyRegistry.DatabaseRetryPolicy;
        _policies[CorePolicyRegistry.ServiceBusRetry] = CorePolicyRegistry.ServiceBusRetryPolicy;
        _policies[CorePolicyRegistry.DefaultTimeout] = CorePolicyRegistry.DefaultTimeoutPolicy;
    }

    public IAsyncPolicy<T> GetPolicy<T>(string policyName)
    {
        if (!_policies.TryGetValue(policyName, out var policy))
        {
            throw new ArgumentException($"Policy '{policyName}' not found", nameof(policyName));
        }

        // Cast to typed policy
        if (policy is IAsyncPolicy<T> typedPolicy)
        {
            return typedPolicy;
        }

        throw new InvalidOperationException($"Policy '{policyName}' is not of type IAsyncPolicy<{typeof(T).Name}>");
    }

    public IAsyncPolicy GetPolicy(string policyName)
    {
        if (!_policies.TryGetValue(policyName, out var policy))
        {
            throw new ArgumentException($"Policy '{policyName}' not found", nameof(policyName));
        }

        return (IAsyncPolicy)policy;
    }

    public async Task<T> ExecuteAsync<T>(string policyName, Func<Task<T>> action)
    {
        var policy = GetPolicy<T>(policyName);
        return await policy.ExecuteAsync(action);
    }

    public async Task ExecuteAsync(string policyName, Func<Task> action)
    {
        var policy = GetPolicy(policyName);
        await policy.ExecuteAsync(action);
    }
}
