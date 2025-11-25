using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sail.Core.Timeout;

namespace Sail.Compass.Timeout;

internal sealed class TimeoutPolicyUpdater : IHostedService, IDisposable
{
    private readonly ILogger<TimeoutPolicyUpdater> _logger;
    private readonly SailTimeoutPolicyProvider _timeoutPolicyProvider;
    private readonly IObservable<IReadOnlyList<TimeoutPolicyConfig>> _timeoutPolicyStream;
    private readonly CompositeDisposable _subscriptions = new();

    public TimeoutPolicyUpdater(
        ILogger<TimeoutPolicyUpdater> logger,
        SailTimeoutPolicyProvider timeoutPolicyProvider,
        IObservable<IReadOnlyList<TimeoutPolicyConfig>> timeoutPolicyStream)
    {
        _logger = logger;
        _timeoutPolicyProvider = timeoutPolicyProvider;
        _timeoutPolicyStream = timeoutPolicyStream;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var subscription = _timeoutPolicyStream
            .Subscribe(
                async policies => await UpdateTimeoutPolicies(policies),
                ex => _logger.LogError(ex, "Error in Timeout policy stream"),
                () => _logger.LogInformation("Timeout policy stream completed"));

        _subscriptions.Add(subscription);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _subscriptions?.Dispose();
        return Task.CompletedTask;
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

