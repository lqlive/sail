using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sail.Core.Authentication;

namespace Sail.Core.Authentication.OpenIdConnect;

public class OpenIdConnectAuthenticationOptionsProvider
{
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly IOptionsMonitorCache<OpenIdConnectOptions> _optionsCache;
    private readonly SailAuthorizationPolicyProvider _authorizationPolicyProvider;
    private readonly ILogger<OpenIdConnectAuthenticationOptionsProvider> _logger;

    public OpenIdConnectAuthenticationOptionsProvider(
        IAuthenticationSchemeProvider schemeProvider,
        IOptionsMonitorCache<OpenIdConnectOptions> optionsCache,
        SailAuthorizationPolicyProvider authorizationPolicyProvider,
        ILogger<OpenIdConnectAuthenticationOptionsProvider> logger)
    {
        _schemeProvider = schemeProvider;
        _optionsCache = optionsCache;
        _authorizationPolicyProvider = authorizationPolicyProvider;
        _logger = logger;
    }

    public async Task UpdateAsync(
        IReadOnlyDictionary<string, OpenIdConnectConfiguration> configurations,
        CancellationToken cancellationToken)
    {
        var allSchemes = await _schemeProvider.GetAllSchemesAsync();
        var currentSchemes = allSchemes
            .Where(s => s.HandlerType == typeof(OpenIdConnectHandler))
            .Select(s => s.Name)
            .ToHashSet();

        var newSchemeNames = configurations.Keys.ToHashSet();

        // Remove obsolete schemes and policies
        foreach (var schemeName in currentSchemes.Except(newSchemeNames))
        {
            _schemeProvider.RemoveScheme(schemeName);
            _optionsCache.TryRemove(schemeName);
            _authorizationPolicyProvider.RemovePolicy(schemeName);
            _logger.LogInformation("Removed OpenIdConnect scheme and authorization policy: {SchemeName}", schemeName);
        }

        // Update schemes and ensure authorization policies
        foreach (var (name, config) in configurations)
        {
            if (!currentSchemes.Contains(name))
            {
                _schemeProvider.AddScheme(new AuthenticationScheme(
                    name,
                    name,
                    typeof(OpenIdConnectHandler)
                ));
                _logger.LogInformation("Added OpenIdConnect scheme: {SchemeName}", name);
            }
            else
            {
                _optionsCache.TryRemove(name);
                _logger.LogInformation("Updated OpenIdConnect scheme: {SchemeName}", name);
            }

            var options = new OpenIdConnectOptions
            {
                Authority = config.Authority,
                ClientId = config.ClientId,
                ClientSecret = config.ClientSecret,
                ResponseType = config.ResponseType ?? "code",
                RequireHttpsMetadata = config.RequireHttpsMetadata,
                SaveTokens = config.SaveTokens,
                GetClaimsFromUserInfoEndpoint = config.GetClaimsFromUserInfoEndpoint
            };

            if (config.Scope != null)
            {
                options.Scope.Clear();
                foreach (var scope in config.Scope)
                {
                    options.Scope.Add(scope);
                }
            }

            if (config.ClockSkew.HasValue)
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.FromSeconds(config.ClockSkew.Value)
                };
            }

            _optionsCache.TryAdd(name, options);

            _authorizationPolicyProvider.AddOrUpdatePolicy(name, name);

        }

        _logger.LogInformation("OpenIdConnect schemes updated. Total: {Count}", configurations.Count);
    }
}

