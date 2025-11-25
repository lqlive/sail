using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sail.Core.Retry;

namespace Sail.Compass.Retry;

internal sealed class RetryPolicyUpdater : IHostedService, IDisposable
{
    private readonly ILogger<RetryPolicyUpdater> _logger;
    private readonly SailRetryPolicyProvider _retryPolicyProvider;
    private readonly IObservable<IReadOnlyList<RetryPolicyConfig>> _retryPolicyStream;
    private readonly CompositeDisposable _subscriptions = new();

    public RetryPolicyUpdater(
        ILogger<RetryPolicyUpdater> logger,
        SailRetryPolicyProvider retryPolicyProvider,
        IObservable<IReadOnlyList<RetryPolicyConfig>> retryPolicyStream)
    {
        _logger = logger;
        _retryPolicyProvider = retryPolicyProvider;
        _retryPolicyStream = retryPolicyStream;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var subscription = _retryPolicyStream
            .Subscribe(
                async policies => await UpdateRetryPolicies(policies),
                ex => _logger.LogError(ex, "Error in Retry policy stream"),
                () => _logger.LogInformation("Retry policy stream completed"));

        _subscriptions.Add(subscription);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _subscriptions?.Dispose();
        return Task.CompletedTask;
    }

    private async Task UpdateRetryPolicies(IReadOnlyList<RetryPolicyConfig> policies)
    {
        try
        {
            _logger.LogInformation("Updating Retry policies, count: {Count}", policies.Count);
            await _retryPolicyProvider.UpdateAsync(policies, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Retry policies");
        }
    }

    public void Dispose()
    {
        _subscriptions?.Dispose();
    }
}

