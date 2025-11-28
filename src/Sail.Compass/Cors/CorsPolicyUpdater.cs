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
                ex => Log.CorsPolicyStreamError(_logger, ex),
                () => Log.CorsPolicyStreamCompleted(_logger));

        _subscriptions.Add(subscription);
        
        Log.CorsPolicyUpdaterInitialized(_logger);
    }

    private async Task UpdateCorsPolicies(IReadOnlyList<CorsPolicyConfig> policies)
    {
        try
        {
            Log.UpdatingCorsPolicies(_logger, policies.Count);
            await _corsPolicyProvider.UpdateAsync(policies, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Log.UpdateCorsPoliciesFailed(_logger, ex);
        }
    }

    public void Dispose()
    {
        _subscriptions?.Dispose();
    }

    private static class Log
    {
        private static readonly Action<ILogger, Exception?> _corsPolicyStreamError = LoggerMessage.Define(
            LogLevel.Error,
            new EventId(1, nameof(CorsPolicyStreamError)),
            "Error in CORS policy stream");

        private static readonly Action<ILogger, Exception?> _corsPolicyStreamCompleted = LoggerMessage.Define(
            LogLevel.Information,
            new EventId(2, nameof(CorsPolicyStreamCompleted)),
            "CORS policy stream completed");

        private static readonly Action<ILogger, Exception?> _corsPolicyUpdaterInitialized = LoggerMessage.Define(
            LogLevel.Information,
            new EventId(3, nameof(CorsPolicyUpdaterInitialized)),
            "CorsPolicyUpdater initialized and subscribed to policy stream");

        private static readonly Action<ILogger, int, Exception?> _updatingCorsPolicies = LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(4, nameof(UpdatingCorsPolicies)),
            "Updating CORS policies, count: {Count}");

        private static readonly Action<ILogger, Exception?> _updateCorsPoliciesFailed = LoggerMessage.Define(
            LogLevel.Error,
            new EventId(5, nameof(UpdateCorsPoliciesFailed)),
            "Failed to update CORS policies");

        public static void CorsPolicyStreamError(ILogger logger, Exception exception)
        {
            _corsPolicyStreamError(logger, exception);
        }

        public static void CorsPolicyStreamCompleted(ILogger logger)
        {
            _corsPolicyStreamCompleted(logger, null);
        }

        public static void CorsPolicyUpdaterInitialized(ILogger logger)
        {
            _corsPolicyUpdaterInitialized(logger, null);
        }

        public static void UpdatingCorsPolicies(ILogger logger, int count)
        {
            _updatingCorsPolicies(logger, count, null);
        }

        public static void UpdateCorsPoliciesFailed(ILogger logger, Exception exception)
        {
            _updateCorsPoliciesFailed(logger, exception);
        }
    }
}

