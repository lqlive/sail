using Microsoft.Extensions.Logging;

namespace Sail.Core.Retry;

public class SailRetryPolicyProvider : IRetryPolicyProvider
{
    private readonly ILogger<SailRetryPolicyProvider> _logger;
    private volatile IReadOnlyDictionary<string, RetryPolicyConfig> _policies =
        new Dictionary<string, RetryPolicyConfig>(StringComparer.OrdinalIgnoreCase);

    public SailRetryPolicyProvider(ILogger<SailRetryPolicyProvider> logger)
    {
        _logger = logger;
    }

    public RetryPolicyConfig? GetPolicy(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return null;
        }

        if (_policies.TryGetValue(key, out var policy))
        {
            _logger.LogDebug("Retry policy found: {PolicyName}", key);
            return policy;
        }

        _logger.LogWarning("Retry policy not found: {PolicyName}", key);
        return null;
    }

    public Task UpdateAsync(IReadOnlyList<RetryPolicyConfig> configs, CancellationToken cancellationToken)
    {
        var newPolicies = new Dictionary<string, RetryPolicyConfig>(StringComparer.OrdinalIgnoreCase);

        foreach (var config in configs)
        {
            try
            {
               
                    if (newPolicies.TryAdd(config.Name, config))
                    {
                        _logger.LogInformation("Loaded retry policy: {PolicyName}, MaxRetryAttempts: {MaxRetryAttempts}",
                            config.Name, config.MaxRetryAttempts);
                    }
                    else
                    {
                        _logger.LogWarning("Duplicate retry policy name: {PolicyName}", config.Name);
                    }
             
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load retry policy: {PolicyName}", config.Name);
            }
        }

        var oldPolicies = _policies;
        var changed = !AreEquivalent(oldPolicies, newPolicies);

        if (!changed)
        {
            _logger.LogDebug("Retry policies unchanged, skipping update");
            return Task.CompletedTask;
        }

        _logger.LogInformation("Retry policies changed. Applying {Count} policies", newPolicies.Count);
        _policies = newPolicies;

        return Task.CompletedTask;
    }

    private static bool AreEquivalent(
        IReadOnlyDictionary<string, RetryPolicyConfig> old,
        IReadOnlyDictionary<string, RetryPolicyConfig> updated)
    {
        if (old.Count != updated.Count)
        {
            return false;
        }

        foreach (var kvp in old)
        {
            if (!updated.TryGetValue(kvp.Key, out var newConfig))
            {
                return false;
            }

            var oldConfig = kvp.Value;
            if (oldConfig.MaxRetryAttempts != newConfig.MaxRetryAttempts ||
                oldConfig.RetryDelayMilliseconds != newConfig.RetryDelayMilliseconds ||
                oldConfig.UseExponentialBackoff != newConfig.UseExponentialBackoff ||
                !oldConfig.RetryStatusCodes.SequenceEqual(newConfig.RetryStatusCodes))
            {
                return false;
            }
        }

        return true;
    }
}

