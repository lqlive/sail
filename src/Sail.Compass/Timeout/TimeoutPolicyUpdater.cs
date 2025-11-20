using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Sail.Core.Timeout;

namespace Sail.Compass.Timeout;

internal sealed class TimeoutPolicyUpdater : IDisposable
{
    private readonly ILogger<TimeoutPolicyUpdater> _logger;
    private readonly CompositeDisposable _subscriptions = new();

    public TimeoutPolicyUpdater(
        ILogger<TimeoutPolicyUpdater> logger,
        TimeoutPolicyProvider timeoutPolicyProvider,
        IObservable<IReadOnlyList<TimeoutPolicyConfig>> timeoutPolicyStream)
    {
        _logger = logger;

        var subscription = timeoutPolicyStream
            .Subscribe(
                async policies => await UpdateTimeoutPolicies(timeoutPolicyProvider, policies),
                ex => _logger.LogError(ex, "Error in Timeout policy stream"),
                () => _logger.LogInformation("Timeout policy stream completed"));

        _subscriptions.Add(subscription);
    }

    private async Task UpdateTimeoutPolicies(
        TimeoutPolicyProvider timeoutPolicyProvider,
        IReadOnlyList<TimeoutPolicyConfig> policies)
    {
        try
        {
            _logger.LogInformation("Updating Timeout policies, count: {Count}", policies.Count);
            await timeoutPolicyProvider.UpdateAsync(policies, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Timeout policies");
        }
    }

    public void Dispose()
    {
        _subscriptions?.Dispose();
    }
}

