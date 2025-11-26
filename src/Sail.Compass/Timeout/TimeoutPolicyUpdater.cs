using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Sail.Core.Timeout;

namespace Sail.Compass.Timeout;

internal sealed class TimeoutPolicyUpdater : IDisposable
{
    private readonly ILogger<TimeoutPolicyUpdater> _logger;
    private readonly SailTimeoutPolicyProvider _timeoutPolicyProvider;
    private readonly CompositeDisposable _subscriptions = new();

    public TimeoutPolicyUpdater(
        ILogger<TimeoutPolicyUpdater> logger,
        SailTimeoutPolicyProvider timeoutPolicyProvider,
        IObservable<IReadOnlyList<TimeoutPolicyConfig>> timeoutPolicyStream)
    {
        _logger = logger;
        _timeoutPolicyProvider = timeoutPolicyProvider;

        var subscription = timeoutPolicyStream
            .Subscribe(
                async policies => await UpdateTimeoutPolicies(policies),
                ex => _logger.LogError(ex, "Error in Timeout policy stream"),
                () => _logger.LogInformation("Timeout policy stream completed"));

        _subscriptions.Add(subscription);
        
        _logger.LogInformation("TimeoutPolicyUpdater initialized and subscribed to policy stream");
    }

    private async Task UpdateTimeoutPolicies(IReadOnlyList<TimeoutPolicyConfig> policies)
    {
        try
        {
            _logger.LogInformation("Updating Timeout policies, count: {Count}", policies.Count);
            await _timeoutPolicyProvider.UpdateAsync(policies, CancellationToken.None);
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

