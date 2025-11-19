using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Sail.Core.Certificates;

namespace Sail.Compass.Certificates;

internal sealed class ServerCertificateUpdater : IDisposable
{
    private readonly ILogger<ServerCertificateUpdater> _logger;
    private readonly CompositeDisposable _subscriptions = new();

    public ServerCertificateUpdater(
        ILogger<ServerCertificateUpdater> logger,
        IServerCertificateSelector certificateSelector,
        IObservable<IReadOnlyList<CertificateConfig>> certificateStream)
    {
        _logger = logger;

        var subscription = certificateStream
            .Subscribe(
                async certificates => await UpdateCertificates(certificateSelector, certificates),
                ex => _logger.LogError(ex, "Error in certificate stream"),
                () => _logger.LogInformation("Certificate stream completed"));

        _subscriptions.Add(subscription);
    }

    private async Task UpdateCertificates(
        IServerCertificateSelector certificateSelector,
        IReadOnlyList<CertificateConfig> certificates)
    {
        try
        {
            _logger.LogInformation("Updating certificates, count: {Count}", certificates.Count);
            await certificateSelector.UpdateAsync(certificates, CancellationToken.None);
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

