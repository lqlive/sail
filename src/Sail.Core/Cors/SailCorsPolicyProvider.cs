using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Sail.Core.Cors;

public class SailCorsPolicyProvider : ICorsPolicyProvider
{
    private readonly ILogger<SailCorsPolicyProvider> _logger;
    private volatile IReadOnlyDictionary<string, CorsPolicy> _policies =
        new Dictionary<string, CorsPolicy>(StringComparer.OrdinalIgnoreCase);

    public SailCorsPolicyProvider(ILogger<SailCorsPolicyProvider> logger)
    {
        _logger = logger;
    }

    public IReadOnlyDictionary<string, CorsPolicy> Policies => _policies;

    public Task<CorsPolicy?> GetPolicyAsync(HttpContext context, string? policyName)
    {
        if (string.IsNullOrEmpty(policyName))
        {
            return Task.FromResult<CorsPolicy?>(null);
        }

        if (_policies.TryGetValue(policyName, out var policy))
        {
            Log.PolicyFound(_logger, policyName);
            return Task.FromResult<CorsPolicy?>(policy);
        }

        Log.PolicyNotFound(_logger, policyName);
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
                    Log.LoadedPolicy(_logger, config.Name);
                }
                else
                {
                    Log.DuplicatePolicyName(_logger, config.Name);
                }
            }
            catch (Exception ex)
            {
                Log.FailedToBuildPolicy(_logger, config.Name, ex);
            }
        }

        var oldPolicies = _policies;
        var changed = !AreEquivalent(oldPolicies, newPolicies);

        if (!changed)
        {
            Log.PoliciesUnchanged(_logger);
            return Task.CompletedTask;
        }

        Log.PoliciesChanged(_logger, newPolicies.Count);
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

    private static class Log
    {
        private static readonly Action<ILogger, string, Exception?> _policyFound = LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(1, nameof(PolicyFound)),
            "CORS policy found: {PolicyName}");

        private static readonly Action<ILogger, string, Exception?> _policyNotFound = LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(2, nameof(PolicyNotFound)),
            "CORS policy not found: {PolicyName}");

        private static readonly Action<ILogger, string, Exception?> _loadedPolicy = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(3, nameof(LoadedPolicy)),
            "Loaded CORS policy: {PolicyName}");

        private static readonly Action<ILogger, string, Exception?> _duplicatePolicyName = LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(4, nameof(DuplicatePolicyName)),
            "Duplicate CORS policy name: {PolicyName}");

        private static readonly Action<ILogger, string, Exception?> _failedToBuildPolicy = LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(5, nameof(FailedToBuildPolicy)),
            "Failed to build CORS policy: {PolicyName}");

        private static readonly Action<ILogger, Exception?> _policiesUnchanged = LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(6, nameof(PoliciesUnchanged)),
            "CORS policies unchanged, skipping update");

        private static readonly Action<ILogger, int, Exception?> _policiesChanged = LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(7, nameof(PoliciesChanged)),
            "CORS policies changed. Applying {Count} policies");

        public static void PolicyFound(ILogger logger, string policyName)
        {
            _policyFound(logger, policyName, null);
        }

        public static void PolicyNotFound(ILogger logger, string policyName)
        {
            _policyNotFound(logger, policyName, null);
        }

        public static void LoadedPolicy(ILogger logger, string policyName)
        {
            _loadedPolicy(logger, policyName, null);
        }

        public static void DuplicatePolicyName(ILogger logger, string policyName)
        {
            _duplicatePolicyName(logger, policyName, null);
        }

        public static void FailedToBuildPolicy(ILogger logger, string policyName, Exception exception)
        {
            _failedToBuildPolicy(logger, policyName, exception);
        }

        public static void PoliciesUnchanged(ILogger logger)
        {
            _policiesUnchanged(logger, null);
        }

        public static void PoliciesChanged(ILogger logger, int count)
        {
            _policiesChanged(logger, count, null);
        }
    }
}
