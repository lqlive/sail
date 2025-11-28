using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Sail.Core.Timeout;

namespace Sail.Compass.Timeout;

internal sealed class TimeoutPolicyUpdater : IDisposable
{
    private readonly ILogger<TimeoutPolicyUpdater> _logger;
    private readonly SailTimeoutPolicyProvider _timeoutPolicyProvider;
    private readonly CompositeDisposable _subscriptions = new();

    public TimeoutPolicyUpdater(
        ILogger<TimeoutPolicyUpdater> logger,
        SailTimeoutPolicyProvider timeoutPolicyProvider,
        IObservable<IReadOnlyList<TimeoutPolicyConfig>> timeoutPolicyStream)
    {
        _logger = logger;
        _timeoutPolicyProvider = timeoutPolicyProvider;

        var subscription = timeoutPolicyStream
            .Subscribe(
                async policies => await UpdateTimeoutPolicies(policies),
                ex => Log.TimeoutPolicyStreamError(_logger, ex),
                () => Log.TimeoutPolicyStreamCompleted(_logger));

        _subscriptions.Add(subscription);
        
        Log.TimeoutPolicyUpdaterInitialized(_logger);
    }

    private async Task UpdateTimeoutPolicies(IReadOnlyList<TimeoutPolicyConfig> policies)
    {
        try
        {
            Log.UpdatingTimeoutPolicies(_logger, policies.Count);
            await _timeoutPolicyProvider.UpdateAsync(policies, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Log.UpdateTimeoutPoliciesFailed(_logger, ex);
        }
    }

    public void Dispose()
    {
        _subscriptions?.Dispose();
    }

    private static class Log
    {
        private static readonly Action<ILogger, Exception?> _timeoutPolicyStreamError = LoggerMessage.Define(
            LogLevel.Error,
            new EventId(1, nameof(TimeoutPolicyStreamError)),
            "Error in Timeout policy stream");

        private static readonly Action<ILogger, Exception?> _timeoutPolicyStreamCompleted = LoggerMessage.Define(
            LogLevel.Information,
            new EventId(2, nameof(TimeoutPolicyStreamCompleted)),
            "Timeout policy stream completed");

        private static readonly Action<ILogger, Exception?> _timeoutPolicyUpdaterInitialized = LoggerMessage.Define(
            LogLevel.Information,
            new EventId(3, nameof(TimeoutPolicyUpdaterInitialized)),
            "TimeoutPolicyUpdater initialized and subscribed to policy stream");

        private static readonly Action<ILogger, int, Exception?> _updatingTimeoutPolicies = LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(4, nameof(UpdatingTimeoutPolicies)),
            "Updating Timeout policies, count: {Count}");

        private static readonly Action<ILogger, Exception?> _updateTimeoutPoliciesFailed = LoggerMessage.Define(
            LogLevel.Error,
            new EventId(5, nameof(UpdateTimeoutPoliciesFailed)),
            "Failed to update Timeout policies");

        public static void TimeoutPolicyStreamError(ILogger logger, Exception exception)
        {
            _timeoutPolicyStreamError(logger, exception);
        }

        public static void TimeoutPolicyStreamCompleted(ILogger logger)
        {
            _timeoutPolicyStreamCompleted(logger, null);
        }

        public static void TimeoutPolicyUpdaterInitialized(ILogger logger)
        {
            _timeoutPolicyUpdaterInitialized(logger, null);
        }

        public static void UpdatingTimeoutPolicies(ILogger logger, int count)
        {
            _updatingTimeoutPolicies(logger, count, null);
        }

        public static void UpdateTimeoutPoliciesFailed(ILogger logger, Exception exception)
        {
            _updateTimeoutPoliciesFailed(logger, exception);
        }
    }
}

