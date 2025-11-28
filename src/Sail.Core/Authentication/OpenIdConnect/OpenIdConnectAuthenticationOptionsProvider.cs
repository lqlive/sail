using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

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

        foreach (var schemeName in currentSchemes.Except(newSchemeNames))
        {
            _schemeProvider.RemoveScheme(schemeName);
            _optionsCache.TryRemove(schemeName);
            _authorizationPolicyProvider.RemovePolicy(schemeName);
            Log.RemovedScheme(_logger, schemeName);
        }

        foreach (var (name, config) in configurations)
        {
            if (!currentSchemes.Contains(name))
            {
                _schemeProvider.AddScheme(new AuthenticationScheme(
                    name,
                    name,
                    typeof(OpenIdConnectHandler)
                ));
                Log.AddedScheme(_logger, name);
            }
            else
            {
                _optionsCache.TryRemove(name);
                Log.UpdatedScheme(_logger, name);
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

        Log.SchemesUpdated(_logger, configurations.Count);
    }

    private static class Log
    {
        private static readonly Action<ILogger, string, Exception?> _removedScheme = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1, nameof(RemovedScheme)),
            "Removed OpenIdConnect scheme and authorization policy: {SchemeName}");

        private static readonly Action<ILogger, string, Exception?> _addedScheme = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(2, nameof(AddedScheme)),
            "Added OpenIdConnect scheme: {SchemeName}");

        private static readonly Action<ILogger, string, Exception?> _updatedScheme = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(3, nameof(UpdatedScheme)),
            "Updated OpenIdConnect scheme: {SchemeName}");

        private static readonly Action<ILogger, int, Exception?> _schemesUpdated = LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(4, nameof(SchemesUpdated)),
            "OpenIdConnect schemes updated. Total: {Count}");

        public static void RemovedScheme(ILogger logger, string schemeName)
        {
            _removedScheme(logger, schemeName, null);
        }

        public static void AddedScheme(ILogger logger, string schemeName)
        {
            _addedScheme(logger, schemeName, null);
        }

        public static void UpdatedScheme(ILogger logger, string schemeName)
        {
            _updatedScheme(logger, schemeName, null);
        }

        public static void SchemesUpdated(ILogger logger, int count)
        {
            _schemesUpdated(logger, count, null);
        }
    }
}
