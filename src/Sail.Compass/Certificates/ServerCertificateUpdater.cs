using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sail.Core.Certificates;

namespace Sail.Compass.Certificates;

internal sealed class ServerCertificateUpdater : IHostedService, IDisposable
{
    private readonly ILogger<ServerCertificateUpdater> _logger;
    private readonly IServerCertificateSelector _certificateSelector;
    private readonly IObservable<IReadOnlyList<CertificateConfig>> _certificateStream;
    private readonly CompositeDisposable _subscriptions = new();

    public ServerCertificateUpdater(
        ILogger<ServerCertificateUpdater> logger,
        IServerCertificateSelector certificateSelector,
        IObservable<IReadOnlyList<CertificateConfig>> certificateStream)
    {
        _logger = logger;
        _certificateSelector = certificateSelector;
        _certificateStream = certificateStream;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var subscription = _certificateStream
            .Subscribe(
                async certificates => await UpdateCertificates(certificates),
                ex => _logger.LogError(ex, "Error in certificate stream"),
                () => _logger.LogInformation("Certificate stream completed"));

        _subscriptions.Add(subscription);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _subscriptions?.Dispose();
        return Task.CompletedTask;
    }

    private async Task UpdateCertificates(IReadOnlyList<CertificateConfig> certificates)
    {
        try
        {
            _logger.LogInformation("Updating certificates, count: {Count}", certificates.Count);
            await _certificateSelector.UpdateAsync(certificates, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update certificates");
        }
    }

    public void Dispose()
    {
        _subscriptions?.Dispose();
    }
}

