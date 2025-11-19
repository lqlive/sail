using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Sail.Core.Authentication.JwtBearer;

public class JwtBearerAuthenticationOptionsProvider
{
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly IOptionsMonitorCache<JwtBearerOptions> _optionsCache;
    private readonly DynamicAuthorizationPolicyProvider _authorizationPolicyProvider;
    private readonly ILogger<JwtBearerAuthenticationOptionsProvider> _logger;

    public JwtBearerAuthenticationOptionsProvider(
        IAuthenticationSchemeProvider schemeProvider,
        IOptionsMonitorCache<JwtBearerOptions> optionsCache,
        DynamicAuthorizationPolicyProvider authorizationPolicyProvider,
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

        // Remove obsolete schemes and policies
        foreach (var schemeName in currentSchemes.Except(newSchemeNames))
        {
            _schemeProvider.RemoveScheme(schemeName);
            _optionsCache.TryRemove(schemeName);
            _authorizationPolicyProvider.RemovePolicy(schemeName);
            _logger.LogInformation("Removed JWT Bearer scheme and authorization policy: {SchemeName}", schemeName);
        }

        // Update schemes and ensure authorization policies
        foreach (var (name, config) in configurations)
        {
            if (!currentSchemes.Contains(name))
            {
                _schemeProvider.AddScheme(new AuthenticationScheme(
                    name,
                    name,
                    typeof(JwtBearerHandler)
                ));
                _logger.LogInformation("Added JWT Bearer scheme: {SchemeName}", name);
            }
            else
            {
                _optionsCache.TryRemove(name);
                _logger.LogInformation("Updated JWT Bearer scheme: {SchemeName}", name);
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

        _logger.LogInformation("JWT Bearer schemes updated. Total: {Count}", configurations.Count);
    }
}

