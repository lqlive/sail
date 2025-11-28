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
                ex => Log.RetryPolicyStreamError(_logger, ex),
                () => Log.RetryPolicyStreamCompleted(_logger));

        _subscriptions.Add(subscription);
        
        Log.RetryPolicyUpdaterInitialized(_logger);
    }

    private async Task UpdateRetryPolicies(IReadOnlyList<RetryPolicyConfig> policies)
    {
        try
        {
            Log.UpdatingRetryPolicies(_logger, policies.Count);
            await _retryPolicyProvider.UpdateAsync(policies, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Log.UpdateRetryPoliciesFailed(_logger, ex);
        }
    }

    public void Dispose()
    {
        _subscriptions?.Dispose();
    }

    private static class Log
    {
        private static readonly Action<ILogger, Exception?> _retryPolicyStreamError = LoggerMessage.Define(
            LogLevel.Error,
            new EventId(1, nameof(RetryPolicyStreamError)),
            "Error in Retry policy stream");

        private static readonly Action<ILogger, Exception?> _retryPolicyStreamCompleted = LoggerMessage.Define(
            LogLevel.Information,
            new EventId(2, nameof(RetryPolicyStreamCompleted)),
            "Retry policy stream completed");

        private static readonly Action<ILogger, Exception?> _retryPolicyUpdaterInitialized = LoggerMessage.Define(
            LogLevel.Information,
            new EventId(3, nameof(RetryPolicyUpdaterInitialized)),
            "RetryPolicyUpdater initialized and subscribed to policy stream");

        private static readonly Action<ILogger, int, Exception?> _updatingRetryPolicies = LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(4, nameof(UpdatingRetryPolicies)),
            "Updating Retry policies, count: {Count}");

        private static readonly Action<ILogger, Exception?> _updateRetryPoliciesFailed = LoggerMessage.Define(
            LogLevel.Error,
            new EventId(5, nameof(UpdateRetryPoliciesFailed)),
            "Failed to update Retry policies");

        public static void RetryPolicyStreamError(ILogger logger, Exception exception)
        {
            _retryPolicyStreamError(logger, exception);
        }

        public static void RetryPolicyStreamCompleted(ILogger logger)
        {
            _retryPolicyStreamCompleted(logger, null);
        }

        public static void RetryPolicyUpdaterInitialized(ILogger logger)
        {
            _retryPolicyUpdaterInitialized(logger, null);
        }

        public static void UpdatingRetryPolicies(ILogger logger, int count)
        {
            _updatingRetryPolicies(logger, count, null);
        }

        public static void UpdateRetryPoliciesFailed(ILogger logger, Exception exception)
        {
            _updateRetryPoliciesFailed(logger, exception);
        }
    }
}

