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

    public RetryPipelineWrapper? GetPolicy(string key)
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
        var newPolicies = new Dictionary<string, RetryPipelineWrapper>(StringComparer.OrdinalIgnoreCase);

        foreach (var config in configs)
        {
            try
            {
                var pipeline = BuildResiliencePipeline(config);
                var policy = new RetryPipelineWrapper(config, pipeline);

                if (newPolicies.TryAdd(config.Name, policy))
                {
                    _logger.LogInformation("Loaded retry policy: {PolicyName}, MaxRetryAttempts: {MaxRetryAttempts}, BackoffType: {BackoffType}",
                        config.Name, config.MaxRetryAttempts, config.UseExponentialBackoff ? "Exponential" : "Linear");
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

        _logger.LogInformation("Retry policies changed. Applying {Count} policies", newPolicies.Count);
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
                _logger.LogDebug("Retry attempt {AttemptNumber} after {Delay}ms",
                    args.AttemptNumber + 1, args.RetryDelay.TotalMilliseconds);

                if (args.Context.Properties.TryGetValue(RetryKeys.OnRetryCallback, out var callback))
                {
                    callback?.Invoke();
                }

                return ValueTask.CompletedTask;
            }
        };

        return new ResiliencePipelineBuilder().AddRetry(retryStrategyOptions).Build();
    }
}