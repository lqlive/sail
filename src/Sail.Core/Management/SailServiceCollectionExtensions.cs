using Sail.Core.Certificates;
using Sail.Core.Cors;
using Sail.Core.RateLimiter;
using Sail.Core.Https;
using Sail.Core.Authentication;
using Sail.Core.Authentication.JwtBearer;
using Sail.Core.Authentication.OpenIdConnect;
using Sail.Core.Timeout;
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

    public static IServiceCollection AddSailCors(this IServiceCollection services)
    {
        services.AddCors();
        services.AddSingleton<SailCorsPolicyProvider>();
        services.AddSingleton<ICorsPolicyProvider>(sp => sp.GetRequiredService<SailCorsPolicyProvider>());
        return services;
    }

    public static IServiceCollection AddSailRateLimiter(this IServiceCollection services)
    {
        services.TryAddSingleton<SailRateLimiterPolicyProvider>();
        services.TryAddSingleton<IRateLimiterPolicyProvider>(sp => sp.GetRequiredService<SailRateLimiterPolicyProvider>());
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

    public static IServiceCollection AddSailAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication();
        services.AddAuthorization();
        
        // Register custom authorization policy provider
        services.AddSingleton<SailAuthorizationPolicyProvider>();
        services.AddSingleton<IAuthorizationPolicyProvider>(sp => 
            sp.GetRequiredService<SailAuthorizationPolicyProvider>());
        
        services.AddSingleton<JwtBearerAuthenticationOptionsProvider>();
        services.AddSingleton<OpenIdConnectAuthenticationOptionsProvider>();
        return services;
    }

    public static IServiceCollection AddSailTimeout(this IServiceCollection services)
    {
        services.AddRequestTimeouts();
        services.AddSingleton<SailTimeoutPolicyProvider>();
        return services;
    }
}