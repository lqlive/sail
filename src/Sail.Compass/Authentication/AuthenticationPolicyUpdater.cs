using System.Reactive.Disposables;
using Microsoft.Extensions.Logging;
using Sail.Core.Authentication;
using Sail.Core.Authentication.JwtBearer;
using Sail.Core.Authentication.OpenIdConnect;
using Sail.Core.Entities;

namespace Sail.Compass.Authentication;

internal sealed class AuthenticationPolicyUpdater : IDisposable
{
    private readonly ILogger<AuthenticationPolicyUpdater> _logger;
    private readonly JwtBearerAuthenticationOptionsProvider _jwtBearerProvider;
    private readonly OpenIdConnectAuthenticationOptionsProvider _oidcProvider;
    private readonly CompositeDisposable _subscriptions = new();

    public AuthenticationPolicyUpdater(
        ILogger<AuthenticationPolicyUpdater> logger,
        JwtBearerAuthenticationOptionsProvider jwtBearerProvider,
        OpenIdConnectAuthenticationOptionsProvider oidcProvider,
        IObservable<IReadOnlyList<AuthenticationPolicyConfig>> policyStream)
    {
        _logger = logger;
        _jwtBearerProvider = jwtBearerProvider;
        _oidcProvider = oidcProvider;

        var subscription = policyStream
            .Subscribe(
                async policies => await UpdateAuthenticationPolicies(policies),
                ex => Log.AuthenticationPolicyStreamError(_logger, ex),
                () => Log.AuthenticationPolicyStreamCompleted(_logger));

        _subscriptions.Add(subscription);
        
        Log.AuthenticationPolicyUpdaterInitialized(_logger);
    }

    private async Task UpdateAuthenticationPolicies(IReadOnlyList<AuthenticationPolicyConfig> policies)
    {
        try
        {
            Log.UpdatingAuthenticationPolicies(_logger, policies.Count);

            var jwtBearerConfigs = policies
                .Where(p => p.Type == AuthenticationSchemeType.JwtBearer && p.JwtBearer != null)
                .ToDictionary(p => p.Name, p => p.JwtBearer!);

            var oidcConfigs = policies
                .Where(p => p.Type == AuthenticationSchemeType.OpenIdConnect && p.OpenIdConnect != null)
                .ToDictionary(p => p.Name, p => p.OpenIdConnect!);

            await _jwtBearerProvider.UpdateAsync(jwtBearerConfigs, CancellationToken.None);
            await _oidcProvider.UpdateAsync(oidcConfigs, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Log.UpdateAuthenticationPoliciesFailed(_logger, ex);
        }
    }

    public void Dispose()
    {
        _subscriptions?.Dispose();
    }

    private static class Log
    {
        private static readonly Action<ILogger, Exception?> _authenticationPolicyStreamError = LoggerMessage.Define(
            LogLevel.Error,
            new EventId(1, nameof(AuthenticationPolicyStreamError)),
            "Error in authentication policy stream");

        private static readonly Action<ILogger, Exception?> _authenticationPolicyStreamCompleted = LoggerMessage.Define(
            LogLevel.Information,
            new EventId(2, nameof(AuthenticationPolicyStreamCompleted)),
            "Authentication policy stream completed");

        private static readonly Action<ILogger, Exception?> _authenticationPolicyUpdaterInitialized = LoggerMessage.Define(
            LogLevel.Information,
            new EventId(3, nameof(AuthenticationPolicyUpdaterInitialized)),
            "AuthenticationPolicyUpdater initialized and subscribed to policy stream");

        private static readonly Action<ILogger, int, Exception?> _updatingAuthenticationPolicies = LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(4, nameof(UpdatingAuthenticationPolicies)),
            "Updating authentication policies, count: {Count}");

        private static readonly Action<ILogger, Exception?> _updateAuthenticationPoliciesFailed = LoggerMessage.Define(
            LogLevel.Error,
            new EventId(5, nameof(UpdateAuthenticationPoliciesFailed)),
            "Failed to update authentication policies");

        public static void AuthenticationPolicyStreamError(ILogger logger, Exception exception)
        {
            _authenticationPolicyStreamError(logger, exception);
        }

        public static void AuthenticationPolicyStreamCompleted(ILogger logger)
        {
            _authenticationPolicyStreamCompleted(logger, null);
        }

        public static void AuthenticationPolicyUpdaterInitialized(ILogger logger)
        {
            _authenticationPolicyUpdaterInitialized(logger, null);
        }

        public static void UpdatingAuthenticationPolicies(ILogger logger, int count)
        {
            _updatingAuthenticationPolicies(logger, count, null);
        }

        public static void UpdateAuthenticationPoliciesFailed(ILogger logger, Exception exception)
        {
            _updateAuthenticationPoliciesFailed(logger, exception);
        }
    }
}

