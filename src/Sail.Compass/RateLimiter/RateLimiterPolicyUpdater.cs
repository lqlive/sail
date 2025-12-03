using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Sail.Core.RateLimiter;

namespace Sail.Compass.RateLimiter;

internal sealed class RateLimiterPolicyUpdater : IDisposable
{
    private readonly ILogger<RateLimiterPolicyUpdater> _logger;
    private readonly SailRateLimiterPolicyProvider _rateLimiterPolicyProvider;
    private readonly CompositeDisposable _subscriptions = new();

    public RateLimiterPolicyUpdater(
        ILogger<RateLimiterPolicyUpdater> logger,
        SailRateLimiterPolicyProvider rateLimiterPolicyProvider,
        IObservable<IReadOnlyList<RateLimiterPolicyConfig>> rateLimiterPolicyStream)
    {
        _logger = logger;
        _rateLimiterPolicyProvider = rateLimiterPolicyProvider;

        var subscription = rateLimiterPolicyStream
            .Subscribe(
                async policies => await UpdateRateLimiterPolicies(policies),
                ex => Log.RateLimiterPolicyStreamError(_logger, ex),
                () => Log.RateLimiterPolicyStreamCompleted(_logger));

        _subscriptions.Add(subscription);
        
        Log.RateLimiterPolicyUpdaterInitialized(_logger);
    }

    private async Task UpdateRateLimiterPolicies(IReadOnlyList<RateLimiterPolicyConfig> policies)
    {
        try
        {
            Log.UpdatingRateLimiterPolicies(_logger, policies.Count);
            await _rateLimiterPolicyProvider.UpdateAsync(policies, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Log.UpdateRateLimiterPoliciesFailed(_logger, ex);
        }
    }

    public void Dispose()
    {
        _subscriptions?.Dispose();
    }

    private static class Log
    {
        private static readonly Action<ILogger, Exception?> _rateLimiterPolicyStreamError = LoggerMessage.Define(
            LogLevel.Error,
            new EventId(1, nameof(RateLimiterPolicyStreamError)),
            "Error in rate limiter policy stream");

        private static readonly Action<ILogger, Exception?> _rateLimiterPolicyStreamCompleted = LoggerMessage.Define(
            LogLevel.Information,
            new EventId(2, nameof(RateLimiterPolicyStreamCompleted)),
            "Rate limiter policy stream completed");

        private static readonly Action<ILogger, Exception?> _rateLimiterPolicyUpdaterInitialized = LoggerMessage.Define(
            LogLevel.Information,
            new EventId(3, nameof(RateLimiterPolicyUpdaterInitialized)),
            "RateLimiterPolicyUpdater initialized and subscribed to policy stream");

        private static readonly Action<ILogger, int, Exception?> _updatingRateLimiterPolicies = LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(4, nameof(UpdatingRateLimiterPolicies)),
            "Updating rate limiter policies, count: {Count}");

        private static readonly Action<ILogger, Exception?> _updateRateLimiterPoliciesFailed = LoggerMessage.Define(
            LogLevel.Error,
            new EventId(5, nameof(UpdateRateLimiterPoliciesFailed)),
            "Failed to update rate limiter policies");

        public static void RateLimiterPolicyStreamError(ILogger logger, Exception exception)
        {
            _rateLimiterPolicyStreamError(logger, exception);
        }

        public static void RateLimiterPolicyStreamCompleted(ILogger logger)
        {
            _rateLimiterPolicyStreamCompleted(logger, null);
        }

        public static void RateLimiterPolicyUpdaterInitialized(ILogger logger)
        {
            _rateLimiterPolicyUpdaterInitialized(logger, null);
        }

        public static void UpdatingRateLimiterPolicies(ILogger logger, int count)
        {
            _updatingRateLimiterPolicies(logger, count, null);
        }

        public static void UpdateRateLimiterPoliciesFailed(ILogger logger, Exception exception)
        {
            _updateRateLimiterPoliciesFailed(logger, exception);
        }
    }
}