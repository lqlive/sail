using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Sail.Core.RateLimiter;

namespace Sail.Compass.RateLimiter;

internal sealed class RateLimiterPolicyUpdater : IDisposable
{
    private readonly ILogger<RateLimiterPolicyUpdater> _logger;
    private readonly SailRateLimiterPolicyProvider _rateLimiterPolicyProvider;
    private readonly CompositeDisposable _subscriptions = new();

    public RateLimiterPolicyUpdater(
        ILogger<RateLimiterPolicyUpdater> logger,
        SailRateLimiterPolicyProvider rateLimiterPolicyProvider,
        IObservable<IReadOnlyList<RateLimiterPolicyConfig>> rateLimiterPolicyStream)
    {
        _logger = logger;
        _rateLimiterPolicyProvider = rateLimiterPolicyProvider;

        var subscription = rateLimiterPolicyStream
            .Subscribe(
                async policies => await UpdateRateLimiterPolicies(policies),
                ex => _logger.LogError(ex, "Error in rate limiter policy stream"),
                () => _logger.LogInformation("Rate limiter policy stream completed"));

        _subscriptions.Add(subscription);
        
        _logger.LogInformation("RateLimiterPolicyUpdater initialized and subscribed to policy stream");
    }

    private async Task UpdateRateLimiterPolicies(IReadOnlyList<RateLimiterPolicyConfig> policies)
    {
        try
        {
            _logger.LogInformation("Updating rate limiter policies, count: {Count}", policies.Count);
            await _rateLimiterPolicyProvider.UpdateAsync(policies, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update rate limiter policies");
        }
    }

    public void Dispose()
    {
        _subscriptions?.Dispose();
    }
}

