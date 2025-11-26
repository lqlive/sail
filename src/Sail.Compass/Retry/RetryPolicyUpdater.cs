using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Sail.Core.Retry;

namespace Sail.Compass.Retry;

internal sealed class RetryPolicyUpdater : IDisposable
{
    private readonly ILogger<RetryPolicyUpdater> _logger;
    private readonly SailRetryPolicyProvider _retryPolicyProvider;
    private readonly CompositeDisposable _subscriptions = new();

    public RetryPolicyUpdater(
        ILogger<RetryPolicyUpdater> logger,
        SailRetryPolicyProvider retryPolicyProvider,
        IObservable<IReadOnlyList<RetryPolicyConfig>> retryPolicyStream)
    {
        _logger = logger;
        _retryPolicyProvider = retryPolicyProvider;

        var subscription = retryPolicyStream
            .Subscribe(
                async policies => await UpdateRetryPolicies(policies),
                ex => _logger.LogError(ex, "Error in Retry policy stream"),
                () => _logger.LogInformation("Retry policy stream completed"));

        _subscriptions.Add(subscription);
        
        _logger.LogInformation("RetryPolicyUpdater initialized and subscribed to policy stream");
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

