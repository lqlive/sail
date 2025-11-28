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
                ex => Log.CertificateStreamError(_logger, ex),
                () => Log.CertificateStreamCompleted(_logger));

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
            Log.UpdatingCertificates(_logger, certificates.Count);
            await _certificateSelector.UpdateAsync(certificates, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Log.UpdateCertificatesFailed(_logger, ex);
        }
    }

    public void Dispose()
    {
        _subscriptions?.Dispose();
    }

    private static class Log
    {
        private static readonly Action<ILogger, Exception?> _certificateStreamError = LoggerMessage.Define(
            LogLevel.Error,
            new EventId(1, nameof(CertificateStreamError)),
            "Error in certificate stream");

        private static readonly Action<ILogger, Exception?> _certificateStreamCompleted = LoggerMessage.Define(
            LogLevel.Information,
            new EventId(2, nameof(CertificateStreamCompleted)),
            "Certificate stream completed");

        private static readonly Action<ILogger, int, Exception?> _updatingCertificates = LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(3, nameof(UpdatingCertificates)),
            "Updating certificates, count: {Count}");

        private static readonly Action<ILogger, Exception?> _updateCertificatesFailed = LoggerMessage.Define(
            LogLevel.Error,
            new EventId(4, nameof(UpdateCertificatesFailed)),
            "Failed to update certificates");

        public static void CertificateStreamError(ILogger logger, Exception exception)
        {
            _certificateStreamError(logger, exception);
        }

        public static void CertificateStreamCompleted(ILogger logger)
        {
            _certificateStreamCompleted(logger, null);
        }

        public static void UpdatingCertificates(ILogger logger, int count)
        {
            _updatingCertificates(logger, count, null);
        }

        public static void UpdateCertificatesFailed(ILogger logger, Exception exception)
        {
            _updateCertificatesFailed(logger, exception);
        }
    }
}

