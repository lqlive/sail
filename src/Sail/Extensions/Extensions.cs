using Microsoft.Extensions.DependencyInjection.Extensions;
using Sail.Core.Options;
using Sail.Services;

namespace Sail.Extensions;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        services.AddOptions<DatabaseOptions>()
            .BindConfiguration(DatabaseOptions.Name);

        services.TryAddScoped<RouteService>();
        services.TryAddScoped<ClusterService>();
        services.TryAddScoped<CertificateService>();
        services.TryAddScoped<MiddlewareService>();
        services.TryAddScoped<AuthenticationPolicyService>();

        services.AddDynamicCors();
        services.AddDynamicRateLimiter();
    }
}