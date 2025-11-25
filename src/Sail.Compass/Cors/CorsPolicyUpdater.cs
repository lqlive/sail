using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sail.Core.Cors;

namespace Sail.Compass.Cors;

internal sealed class CorsPolicyUpdater : IHostedService, IDisposable
{
    private readonly ILogger<CorsPolicyUpdater> _logger;
    private readonly SailCorsPolicyProvider _corsPolicyProvider;
    private readonly IObservable<IReadOnlyList<CorsPolicyConfig>> _corsPolicyStream;
    private readonly CompositeDisposable _subscriptions = new();

    public CorsPolicyUpdater(
        ILogger<CorsPolicyUpdater> logger,
        SailCorsPolicyProvider corsPolicyProvider,
        IObservable<IReadOnlyList<CorsPolicyConfig>> corsPolicyStream)
    {
        _logger = logger;
        _corsPolicyProvider = corsPolicyProvider;
        _corsPolicyStream = corsPolicyStream;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var subscription = _corsPolicyStream
            .Subscribe(
                async policies => await UpdateCorsPolicies(policies),
                ex => _logger.LogError(ex, "Error in CORS policy stream"),
                () => _logger.LogInformation("CORS policy stream completed"));

        _subscriptions.Add(subscription);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _subscriptions?.Dispose();
        return Task.CompletedTask;
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

