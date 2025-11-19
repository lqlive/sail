using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Yarp.ReverseProxy.Model;

namespace Sail.Core.Cors;

public class CorsPolicyProvider : ICorsPolicyProvider
{
    private readonly ILogger<CorsPolicyProvider> _logger;
    private volatile IReadOnlyDictionary<string, CorsPolicy> _policies =
        new Dictionary<string, CorsPolicy>(StringComparer.OrdinalIgnoreCase);

    public CorsPolicyProvider(ILogger<CorsPolicyProvider> logger)
    {
        _logger = logger;
    }

    public IReadOnlyDictionary<string, CorsPolicy> Policies => _policies;

    public Task<CorsPolicy?> GetPolicyAsync(HttpContext context, string? policyName)
    {
        if (string.IsNullOrEmpty(policyName))
        {
            var reverseProxyFeature = context.Features.Get<IReverseProxyFeature>();
            if (reverseProxyFeature?.Route.Config.Metadata?.TryGetValue("CorsPolicy", out var metadataPolicyName) == true)
            {
                policyName = metadataPolicyName;
                _logger.LogDebug("Using CORS policy from route metadata: {PolicyName}", policyName);
            }
        }

        if (string.IsNullOrEmpty(policyName))
        {
            return Task.FromResult<CorsPolicy?>(null);
        }

        if (_policies.TryGetValue(policyName, out var policy))
        {
            _logger.LogDebug("CORS policy found: {PolicyName}", policyName);
            return Task.FromResult<CorsPolicy?>(policy);
        }

        _logger.LogWarning("CORS policy not found: {PolicyName}", policyName);
        return Task.FromResult<CorsPolicy?>(null);
    }

    public Task UpdateAsync(IReadOnlyList<CorsPolicyConfig> configs, CancellationToken cancellationToken)
    {
        var newPolicies = new Dictionary<string, CorsPolicy>(StringComparer.OrdinalIgnoreCase);

        foreach (var config in configs)
        {
            try
            {
                var corsPolicy = BuildCorsPolicy(config);
                if (newPolicies.TryAdd(config.Name, corsPolicy))
                {
                    _logger.LogInformation("Loaded CORS policy: {PolicyName}", config.Name);
                }
                else
                {
                    _logger.LogWarning("Duplicate CORS policy name: {PolicyName}", config.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to build CORS policy: {PolicyName}", config.Name);
            }
        }

        var oldPolicies = _policies;
        var changed = !AreEquivalent(oldPolicies, newPolicies);

        if (!changed)
        {
            _logger.LogDebug("CORS policies unchanged, skipping update");
            return Task.CompletedTask;
        }

        _logger.LogInformation("CORS policies changed. Applying {Count} policies", newPolicies.Count);
        _policies = newPolicies;

        return Task.CompletedTask;
    }

    private static bool AreEquivalent(
        IReadOnlyDictionary<string, CorsPolicy> old,
        IReadOnlyDictionary<string, CorsPolicy> updated)
    {
        if (old.Count != updated.Count)
        {
            return false;
        }

        foreach (var kvp in old)
        {
            if (!updated.ContainsKey(kvp.Key))
            {
                return false;
            }
        }

        return true;
    }

    private static CorsPolicy BuildCorsPolicy(CorsPolicyConfig corsConfig)
    {
        var builder = new CorsPolicyBuilder();

        ConfigureOrigins(builder, corsConfig.AllowOrigins);
        ConfigureMethods(builder, corsConfig.AllowMethods);
        ConfigureHeaders(builder, corsConfig.AllowHeaders);
        ConfigureExposeHeaders(builder, corsConfig.ExposeHeaders);

        if (corsConfig.AllowCredentials)
        {
            builder.AllowCredentials();
        }

        if (corsConfig.MaxAge.HasValue)
        {
            builder.SetPreflightMaxAge(TimeSpan.FromSeconds(corsConfig.MaxAge.Value));
        }

        return builder.Build();
    }

    private static void ConfigureOrigins(CorsPolicyBuilder builder, List<string>? origins)
    {
        if (origins is null or { Count: 0 } || origins.Contains("*", StringComparer.OrdinalIgnoreCase))
        {
            builder.AllowAnyOrigin();
        }
        else
        {
            builder.WithOrigins([.. origins]);
        }
    }

    private static void ConfigureMethods(CorsPolicyBuilder builder, List<string>? methods)
    {
        if (methods is null or { Count: 0 } || methods.Contains("*", StringComparer.OrdinalIgnoreCase))
        {
            builder.AllowAnyMethod();
        }
        else
        {
            builder.WithMethods([.. methods]);
        }
    }

    private static void ConfigureHeaders(CorsPolicyBuilder builder, List<string>? headers)
    {
        if (headers is null or { Count: 0 } || headers.Contains("*", StringComparer.OrdinalIgnoreCase))
        {
            builder.AllowAnyHeader();
        }
        else
        {
            builder.WithHeaders([.. headers]);
        }
    }

    private static void ConfigureExposeHeaders(CorsPolicyBuilder builder, List<string>? exposeHeaders)
    {
        if (exposeHeaders is { Count: > 0 })
        {
            builder.WithExposedHeaders([.. exposeHeaders]);
        }
    }
}
