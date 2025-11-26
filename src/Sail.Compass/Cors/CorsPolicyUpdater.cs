using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Sail.Core.Cors;

namespace Sail.Compass.Cors;

internal sealed class CorsPolicyUpdater : IDisposable
{
    private readonly ILogger<CorsPolicyUpdater> _logger;
    private readonly SailCorsPolicyProvider _corsPolicyProvider;
    private readonly CompositeDisposable _subscriptions = new();

    public CorsPolicyUpdater(
        ILogger<CorsPolicyUpdater> logger,
        SailCorsPolicyProvider corsPolicyProvider,
        IObservable<IReadOnlyList<CorsPolicyConfig>> corsPolicyStream)
    {
        _logger = logger;
        _corsPolicyProvider = corsPolicyProvider;

        var subscription = corsPolicyStream
            .Subscribe(
                async policies => await UpdateCorsPolicies(policies),
                ex => _logger.LogError(ex, "Error in CORS policy stream"),
                () => _logger.LogInformation("CORS policy stream completed"));

        _subscriptions.Add(subscription);
        
        _logger.LogInformation("CorsPolicyUpdater initialized and subscribed to policy stream");
    }

    private async Task UpdateCorsPolicies(IReadOnlyList<CorsPolicyConfig> policies)
    {
        try
        {
            _logger.LogInformation("Updating CORS policies, count: {Count}", policies.Count);
            await _corsPolicyProvider.UpdateAsync(policies, CancellationToken.None);
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

