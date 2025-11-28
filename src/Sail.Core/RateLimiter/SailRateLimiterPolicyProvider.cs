using Microsoft.Extensions.Logging;

namespace Sail.Core.RateLimiter;

public class SailRateLimiterPolicyProvider : IRateLimiterPolicyProvider
{
    private readonly ILogger<SailRateLimiterPolicyProvider> _logger;
    private volatile IReadOnlyDictionary<string, RateLimiterPolicyConfig> _policies =
        new Dictionary<string, RateLimiterPolicyConfig>(StringComparer.OrdinalIgnoreCase);

    public SailRateLimiterPolicyProvider(ILogger<SailRateLimiterPolicyProvider> logger)
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
            Log.PolicyFound(_logger, key);
            return policy;
        }

        Log.PolicyNotFound(_logger, key);
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
                        Log.LoadedPolicy(_logger, config.Name, config.PermitLimit, config.Window);
                    }
                    else
                    {
                        Log.DuplicatePolicyName(_logger, config.Name);
                    }
                }
                else
                {
                    Log.InvalidConfiguration(_logger, config.Name);
                }
            }
            catch (Exception ex)
            {
                Log.FailedToLoadPolicy(_logger, config.Name, ex);
            }
        }

        var oldPolicies = _policies;
        var changed = !AreEquivalent(oldPolicies, newPolicies);

        if (!changed)
        {
            Log.PoliciesUnchanged(_logger);
            return Task.CompletedTask;
        }

        Log.PoliciesChanged(_logger, newPolicies.Count);
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

    private static class Log
    {
        private static readonly Action<ILogger, string, Exception?> _policyFound = LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(1, nameof(PolicyFound)),
            "Rate limiter policy found: {PolicyName}");

        private static readonly Action<ILogger, string, Exception?> _policyNotFound = LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(2, nameof(PolicyNotFound)),
            "Rate limiter policy not found: {PolicyName}");

        private static readonly Action<ILogger, string, int, int, Exception?> _loadedPolicy = LoggerMessage.Define<string, int, int>(
            LogLevel.Information,
            new EventId(3, nameof(LoadedPolicy)),
            "Loaded rate limiter policy: {PolicyName}, PermitLimit: {PermitLimit}, Window: {Window}s");

        private static readonly Action<ILogger, string, Exception?> _duplicatePolicyName = LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(4, nameof(DuplicatePolicyName)),
            "Duplicate rate limiter policy name: {PolicyName}");

        private static readonly Action<ILogger, string, Exception?> _invalidConfiguration = LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(5, nameof(InvalidConfiguration)),
            "Invalid rate limiter policy configuration: {PolicyName}");

        private static readonly Action<ILogger, string, Exception?> _failedToLoadPolicy = LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(6, nameof(FailedToLoadPolicy)),
            "Failed to load rate limiter policy: {PolicyName}");

        private static readonly Action<ILogger, Exception?> _policiesUnchanged = LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(7, nameof(PoliciesUnchanged)),
            "Rate limiter policies unchanged, skipping update");

        private static readonly Action<ILogger, int, Exception?> _policiesChanged = LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(8, nameof(PoliciesChanged)),
            "Rate limiter policies changed. Applying {Count} policies");

        public static void PolicyFound(ILogger logger, string policyName)
        {
            _policyFound(logger, policyName, null);
        }

        public static void PolicyNotFound(ILogger logger, string policyName)
        {
            _policyNotFound(logger, policyName, null);
        }

        public static void LoadedPolicy(ILogger logger, string policyName, int permitLimit, int window)
        {
            _loadedPolicy(logger, policyName, permitLimit, window, null);
        }

        public static void DuplicatePolicyName(ILogger logger, string policyName)
        {
            _duplicatePolicyName(logger, policyName, null);
        }

        public static void InvalidConfiguration(ILogger logger, string policyName)
        {
            _invalidConfiguration(logger, policyName, null);
        }

        public static void FailedToLoadPolicy(ILogger logger, string policyName, Exception exception)
        {
            _failedToLoadPolicy(logger, policyName, exception);
        }

        public static void PoliciesUnchanged(ILogger logger)
        {
            _policiesUnchanged(logger, null);
        }

        public static void PoliciesChanged(ILogger logger, int count)
        {
            _policiesChanged(logger, count, null);
        }
    }
}
