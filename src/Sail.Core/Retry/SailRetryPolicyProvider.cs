using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Sail.Core.Retry;

public class SailRetryPolicyProvider : IRetryPolicyProvider
{
    private readonly ILogger<SailRetryPolicyProvider> _logger;
    private volatile IReadOnlyDictionary<string, RetryPipelineWrapper> _policies =
        new Dictionary<string, RetryPipelineWrapper>(StringComparer.OrdinalIgnoreCase);

    public SailRetryPolicyProvider(ILogger<SailRetryPolicyProvider> logger)
    {
        _logger = logger;
    }

    public Task UpdateAsync(IReadOnlyList<RetryPolicyConfig> configs, CancellationToken cancellationToken)
    {
        var newPolicies = new Dictionary<string, RetryPipelineWrapper>(StringComparer.OrdinalIgnoreCase);

        foreach (var config in configs)
        {
            try
            {
                var pipeline = BuildResiliencePipeline(config);
                var policy = new RetryPipelineWrapper(config, pipeline);

                if (newPolicies.TryAdd(config.Name, policy))
                {
                    Log.LoadedPolicy(_logger, config.Name, config.MaxRetryAttempts, config.UseExponentialBackoff);
                }
                else
                {
                    Log.DuplicatePolicyName(_logger, config.Name);
                }
            }
            catch (Exception ex)
            {
                Log.FailedToLoadPolicy(_logger, config.Name, ex);
            }
        }

        Log.PoliciesChanged(_logger, newPolicies.Count);
        _policies = newPolicies;

        return Task.CompletedTask;
    }

    private ResiliencePipeline BuildResiliencePipeline(RetryPolicyConfig config)
    {
        var retryStrategyOptions = new RetryStrategyOptions
        {
            MaxRetryAttempts = config.MaxRetryAttempts,
            BackoffType = config.UseExponentialBackoff ? DelayBackoffType.Exponential : DelayBackoffType.Linear,
            Delay = TimeSpan.FromMilliseconds(config.RetryDelayMilliseconds),
            ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
            OnRetry = args =>
            {
                Log.RetryAttempt(_logger, args.AttemptNumber + 1, args.RetryDelay.TotalMilliseconds);

                if (args.Context.Properties.TryGetValue(RetryKeys.OnRetryCallback, out var callback))
                {
                    callback?.Invoke();
                }

                return ValueTask.CompletedTask;
            }
        };

        return new ResiliencePipelineBuilder().AddRetry(retryStrategyOptions).Build();
    }
    public RetryPipelineWrapper? GetPolicy(string key)
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

    private static class Log
    {
        private static readonly Action<ILogger, string, Exception?> _policyFound = LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(1, nameof(PolicyFound)),
            "Retry policy found: {PolicyName}");

        private static readonly Action<ILogger, string, Exception?> _policyNotFound = LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(2, nameof(PolicyNotFound)),
            "Retry policy not found: {PolicyName}");

        private static readonly Action<ILogger, string, int, bool, Exception?> _loadedPolicy = LoggerMessage.Define<string, int, bool>(
            LogLevel.Information,
            new EventId(3, nameof(LoadedPolicy)),
            "Loaded retry policy: {PolicyName}, MaxRetryAttempts: {MaxRetryAttempts}, BackoffType: {BackoffType}");

        private static readonly Action<ILogger, string, Exception?> _duplicatePolicyName = LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(4, nameof(DuplicatePolicyName)),
            "Duplicate retry policy name: {PolicyName}");

        private static readonly Action<ILogger, string, Exception?> _failedToLoadPolicy = LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(5, nameof(FailedToLoadPolicy)),
            "Failed to load retry policy: {PolicyName}");

        private static readonly Action<ILogger, int, Exception?> _policiesChanged = LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(6, nameof(PoliciesChanged)),
            "Retry policies changed. Applying {Count} policies");

        private static readonly Action<ILogger, int, double, Exception?> _retryAttempt = LoggerMessage.Define<int, double>(
            LogLevel.Debug,
            new EventId(7, nameof(RetryAttempt)),
            "Retry attempt {AttemptNumber} after {Delay}ms");

        public static void PolicyFound(ILogger logger, string policyName)
        {
            _policyFound(logger, policyName, null);
        }

        public static void PolicyNotFound(ILogger logger, string policyName)
        {
            _policyNotFound(logger, policyName, null);
        }

        public static void LoadedPolicy(ILogger logger, string policyName, int maxRetryAttempts, bool useExponentialBackoff)
        {
            _loadedPolicy(logger, policyName, maxRetryAttempts, useExponentialBackoff, null);
        }

        public static void DuplicatePolicyName(ILogger logger, string policyName)
        {
            _duplicatePolicyName(logger, policyName, null);
        }

        public static void FailedToLoadPolicy(ILogger logger, string policyName, Exception exception)
        {
            _failedToLoadPolicy(logger, policyName, exception);
        }

        public static void PoliciesChanged(ILogger logger, int count)
        {
            _policiesChanged(logger, count, null);
        }

        public static void RetryAttempt(ILogger logger, int attemptNumber, double delay)
        {
            _retryAttempt(logger, attemptNumber, delay, null);
        }
    }
}
