using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Sail.Core.RateLimiter;

namespace Sail.Compass.RateLimiter;

internal sealed class RateLimiterPolicyUpdater : IDisposable
{
    private readonly ILogger<RateLimiterPolicyUpdater> _logger;
    private readonly CompositeDisposable _subscriptions = new();

    public RateLimiterPolicyUpdater(
        ILogger<RateLimiterPolicyUpdater> logger,
        RateLimiterPolicyProvider rateLimiterPolicyProvider,
        IObservable<IReadOnlyList<RateLimiterPolicyConfig>> rateLimiterPolicyStream)
    {
        _logger = logger;

        var subscription = rateLimiterPolicyStream
            .Subscribe(
                async policies => await UpdateRateLimiterPolicies(rateLimiterPolicyProvider, policies),
                ex => _logger.LogError(ex, "Error in rate limiter policy stream"),
                () => _logger.LogInformation("Rate limiter policy stream completed"));

        _subscriptions.Add(subscription);
    }

    private async Task UpdateRateLimiterPolicies(
        RateLimiterPolicyProvider rateLimiterPolicyProvider,
        IReadOnlyList<RateLimiterPolicyConfig> policies)
    {
        try
        {
            _logger.LogInformation("Updating rate limiter policies, count: {Count}", policies.Count);
            await rateLimiterPolicyProvider.UpdateAsync(policies, CancellationToken.None);
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

