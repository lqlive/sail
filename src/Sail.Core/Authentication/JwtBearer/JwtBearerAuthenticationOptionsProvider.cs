using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Sail.Core.Authentication.JwtBearer;

public class JwtBearerAuthenticationOptionsProvider
{
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly IOptionsMonitorCache<JwtBearerOptions> _optionsCache;
    private readonly SailAuthorizationPolicyProvider _authorizationPolicyProvider;
    private readonly ILogger<JwtBearerAuthenticationOptionsProvider> _logger;

    public JwtBearerAuthenticationOptionsProvider(
        IAuthenticationSchemeProvider schemeProvider,
        IOptionsMonitorCache<JwtBearerOptions> optionsCache,
        SailAuthorizationPolicyProvider authorizationPolicyProvider,
        ILogger<JwtBearerAuthenticationOptionsProvider> logger)
    {
        _schemeProvider = schemeProvider;
        _optionsCache = optionsCache;
        _authorizationPolicyProvider = authorizationPolicyProvider;
        _logger = logger;
    }

    public async Task UpdateAsync(
        IReadOnlyDictionary<string, JwtBearerConfiguration> configurations,
        CancellationToken cancellationToken)
    {
        var allSchemes = await _schemeProvider.GetAllSchemesAsync();
        var currentSchemes = allSchemes
            .Where(s => s.HandlerType == typeof(JwtBearerHandler))
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
                    typeof(JwtBearerHandler)
                ));
                Log.AddedScheme(_logger, name);
            }
            else
            {
                _optionsCache.TryRemove(name);
                Log.UpdatedScheme(_logger, name);
            }

            var options = new JwtBearerOptions
            {
                Authority = config.Authority,
                Audience = config.Audience,
                RequireHttpsMetadata = config.RequireHttpsMetadata,
                SaveToken = config.SaveToken,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = config.ValidateIssuer,
                    ValidateAudience = config.ValidateAudience,
                    ValidateLifetime = config.ValidateLifetime,
                    ValidateIssuerSigningKey = config.ValidateIssuerSigningKey,
                    ValidIssuers = config.ValidIssuers,
                    ValidAudiences = config.ValidAudiences,
                    ClockSkew = config.ClockSkew.HasValue
                        ? TimeSpan.FromSeconds(config.ClockSkew.Value)
                        : TimeSpan.FromMinutes(5)
                }
            };

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
            "Removed JWT Bearer scheme and authorization policy: {SchemeName}");

        private static readonly Action<ILogger, string, Exception?> _addedScheme = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(2, nameof(AddedScheme)),
            "Added JWT Bearer scheme: {SchemeName}");

        private static readonly Action<ILogger, string, Exception?> _updatedScheme = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(3, nameof(UpdatedScheme)),
            "Updated JWT Bearer scheme: {SchemeName}");

        private static readonly Action<ILogger, int, Exception?> _schemesUpdated = LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(4, nameof(SchemesUpdated)),
            "JWT Bearer schemes updated. Total: {Count}");

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
