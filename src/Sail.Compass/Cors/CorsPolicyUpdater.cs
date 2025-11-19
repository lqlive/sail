using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Sail.Core.Cors;

namespace Sail.Compass.Cors;

internal sealed class CorsPolicyUpdater : IDisposable
{
    private readonly ILogger<CorsPolicyUpdater> _logger;
    private readonly CompositeDisposable _subscriptions = new();

    public CorsPolicyUpdater(
        ILogger<CorsPolicyUpdater> logger,
        CorsPolicyProvider corsPolicyProvider,
        IObservable<IReadOnlyList<CorsPolicyConfig>> corsPolicyStream)
    {
        _logger = logger;

        var subscription = corsPolicyStream
            .Subscribe(
                async policies => await UpdateCorsPolicies(corsPolicyProvider, policies),
                ex => _logger.LogError(ex, "Error in CORS policy stream"),
                () => _logger.LogInformation("CORS policy stream completed"));

        _subscriptions.Add(subscription);
    }

    private async Task UpdateCorsPolicies(
        CorsPolicyProvider corsPolicyProvider,
        IReadOnlyList<CorsPolicyConfig> policies)
    {
        try
        {
            _logger.LogInformation("Updating CORS policies, count: {Count}", policies.Count);
            await corsPolicyProvider.UpdateAsync(policies, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update CORS policies");
        }
    }

    public void Dispose()
    {
        _subscriptions?.Dispose();
    }
}

