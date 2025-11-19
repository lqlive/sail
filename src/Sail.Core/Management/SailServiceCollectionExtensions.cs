using Sail.Core.Certificates;
using Sail.Core.Cors;
using Sail.Core.RateLimiter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        services.TryAddSingleton<CorsPolicyProvider>();
        services.TryAddSingleton<ICorsPolicyProvider>(sp => sp.GetRequiredService<CorsPolicyProvider>());
        return services;
    }

    public static IServiceCollection AddDynamicRateLimiter(this IServiceCollection services)
    {
        services.TryAddSingleton<RateLimiterPolicyProvider>();
        services.TryAddSingleton<IRateLimiterPolicyProvider>(sp => sp.GetRequiredService<RateLimiterPolicyProvider>());
        return services;
    }
}

public static class SailApplicationBuilderExtensions
{
    public static IApplicationBuilder UseDynamicRateLimiter(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RateLimiterMiddleware>();
    }
}