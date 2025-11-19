using Microsoft.Extensions.Logging;

namespace Sail.Core.RateLimiter;

public class RateLimiterPolicyProvider : IRateLimiterPolicyProvider
{
    private readonly ILogger<RateLimiterPolicyProvider> _logger;
    private volatile IReadOnlyDictionary<string, RateLimiterPolicyConfig> _policies =
        new Dictionary<string, RateLimiterPolicyConfig>(StringComparer.OrdinalIgnoreCase);

    public RateLimiterPolicyProvider(ILogger<RateLimiterPolicyProvider> logger)
    {
        _logger = logger;
    }

    public IReadOnlyDictionary<string, RateLimiterPolicyConfig> Policies => _policies;

    public RateLimiterPolicyConfig? GetPolicy(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return null;
        }

        if (_policies.TryGetValue(key, out var policy))
        {
            _logger.LogDebug("Rate limiter policy found: {PolicyName}", key);
            return policy;
        }

        _logger.LogWarning("Rate limiter policy not found: {PolicyName}", key);
        return null;
    }

    public Task UpdateAsync(IReadOnlyList<RateLimiterPolicyConfig> configs, CancellationToken cancellationToken)
    {
        var newPolicies = new Dictionary<string, RateLimiterPolicyConfig>(StringComparer.OrdinalIgnoreCase);

        foreach (var config in configs)
        {
            try
            {
                if (ValidateConfig(config))
                {
                    if (newPolicies.TryAdd(config.Name, config))
                    {
                        _logger.LogInformation("Loaded rate limiter policy: {PolicyName}, PermitLimit: {PermitLimit}, Window: {Window}s",
                            config.Name, config.PermitLimit, config.Window);
                    }
                    else
                    {
                        _logger.LogWarning("Duplicate rate limiter policy name: {PolicyName}", config.Name);
                    }
                }
                else
                {
                    _logger.LogWarning("Invalid rate limiter policy configuration: {PolicyName}", config.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load rate limiter policy: {PolicyName}", config.Name);
            }
        }

        var oldPolicies = _policies;
        var changed = !AreEquivalent(oldPolicies, newPolicies);

        if (!changed)
        {
            _logger.LogDebug("Rate limiter policies unchanged, skipping update");
            return Task.CompletedTask;
        }

        _logger.LogInformation("Rate limiter policies changed. Applying {Count} policies", newPolicies.Count);
        _policies = newPolicies;

        return Task.CompletedTask;
    }

    private static bool AreEquivalent(
        IReadOnlyDictionary<string, RateLimiterPolicyConfig> old,
        IReadOnlyDictionary<string, RateLimiterPolicyConfig> updated)
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
            if (oldConfig.PermitLimit != newConfig.PermitLimit ||
                oldConfig.Window != newConfig.Window ||
                oldConfig.QueueLimit != newConfig.QueueLimit)
            {
                return false;
            }
        }

        return true;
    }

    private static bool ValidateConfig(RateLimiterPolicyConfig config)
    {
        return config.PermitLimit > 0 &&
               config.Window > 0 &&
               config.QueueLimit >= 0;
    }
}