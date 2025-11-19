using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Sail.Core.Authentication;

public class DynamicAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
    private readonly ConcurrentDictionary<string, AuthorizationPolicy> _policies = new();

    public DynamicAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (_policies.TryGetValue(policyName, out var policy))
        {
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }

    public void AddOrUpdatePolicy(string name, string schemeName)
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes(schemeName)
            .Build();

        _policies.AddOrUpdate(name, policy, (_, _) => policy);
    }

    public void RemovePolicy(string name)
    {
        _policies.TryRemove(name, out _);
    }
}

