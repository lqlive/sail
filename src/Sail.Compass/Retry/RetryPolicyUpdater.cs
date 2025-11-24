using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Sail.Core.Retry;

namespace Sail.Compass.Retry;

internal sealed class RetryPolicyUpdater : IDisposable
{
    private readonly ILogger<RetryPolicyUpdater> _logger;
    private readonly CompositeDisposable _subscriptions = new();

    public RetryPolicyUpdater(
        ILogger<RetryPolicyUpdater> logger,
        SailRetryPolicyProvider retryPolicyProvider,
        IObservable<IReadOnlyList<RetryPolicyConfig>> retryPolicyStream)
    {
        _logger = logger;

        var subscription = retryPolicyStream
            .Subscribe(
                async policies => await UpdateRetryPolicies(retryPolicyProvider, policies),
                ex => _logger.LogError(ex, "Error in Retry policy stream"),
                () => _logger.LogInformation("Retry policy stream completed"));

        _subscriptions.Add(subscription);
    }

    private async Task UpdateRetryPolicies(
        SailRetryPolicyProvider retryPolicyProvider,
        IReadOnlyList<RetryPolicyConfig> policies)
    {
        try
        {
            _logger.LogInformation("Updating Retry policies, count: {Count}", policies.Count);
            await retryPolicyProvider.UpdateAsync(policies, CancellationToken.None);
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

