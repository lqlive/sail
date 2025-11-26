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
                ex => _logger.LogError(ex, "Error in authentication policy stream"),
                () => _logger.LogInformation("Authentication policy stream completed"));

        _subscriptions.Add(subscription);
        
        _logger.LogInformation("AuthenticationPolicyUpdater initialized and subscribed to policy stream");
    }

    private async Task UpdateAuthenticationPolicies(IReadOnlyList<AuthenticationPolicyConfig> policies)
    {
        try
        {
            _logger.LogInformation("Updating authentication policies, count: {Count}", policies.Count);

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
            _logger.LogError(ex, "Failed to update authentication policies");
        }
    }

    public void Dispose()
    {
        _subscriptions?.Dispose();
    }
}

