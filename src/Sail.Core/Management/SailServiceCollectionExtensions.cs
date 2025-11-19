using Sail.Core.Certificates;
using Sail.Core.Cors;
using Sail.Core.RateLimiter;
using Sail.Core.Https;
using Sail.Core.Authentication;
using Sail.Core.Authentication.JwtBearer;
using Sail.Core.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace Microsoft.Extensions.DependencyInjection;

public static class SailServiceCollectionExtensions
{
    public static IServiceCollection AddServerCertificateSelector(this IServiceCollection services)
    {
        services.TryAddSingleton<IServerCertificateSelector, ServerCertificateSelector>();
        return services;
    }

    public static IServiceCollection AddDynamicCors(this IServiceCollection services)
    {
        services.AddCors();
        services.AddSingleton<CorsPolicyProvider>();
        services.AddSingleton<ICorsPolicyProvider>(sp => sp.GetRequiredService<CorsPolicyProvider>());
        return services;
    }

    public static IServiceCollection AddDynamicRateLimiter(this IServiceCollection services)
    {
        services.TryAddSingleton<RateLimiterPolicyProvider>();
        services.TryAddSingleton<IRateLimiterPolicyProvider>(sp => sp.GetRequiredService<RateLimiterPolicyProvider>());
        return services;
    }

    public static IServiceCollection AddRouteHttpsRedirection(this IServiceCollection services, Action<HttpsRedirectionOptions>? configureOptions = null)
    {
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.AddOptions<HttpsRedirectionOptions>();
        }
        
        return services;
    }

    public static IServiceCollection AddDynamicAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication();
        services.AddAuthorization();
        
        // Register custom authorization policy provider
        services.AddSingleton<DynamicAuthorizationPolicyProvider>();
        services.AddSingleton<IAuthorizationPolicyProvider>(sp => 
            sp.GetRequiredService<DynamicAuthorizationPolicyProvider>());
        
        services.AddSingleton<JwtBearerAuthenticationOptionsProvider>();
        services.AddSingleton<OpenIdConnectAuthenticationOptionsProvider>();
        return services;
    }
}