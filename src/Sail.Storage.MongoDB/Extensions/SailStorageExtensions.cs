using Microsoft.Extensions.DependencyInjection;
using Sail.Core.Stores;
using Sail.Storage.MongoDB.Stores;

namespace Sail.Storage.MongoDB.Extensions;

public static class SailStorageExtensions
{
    public static IServiceCollection AddSailStorage(this IServiceCollection services)
    {
        services.AddSingleton<SailContext>();
        services.AddSingleton<IClusterStore, ClusterStore>();
        services.AddSingleton<IRouteStore, RouteStore>();
        services.AddSingleton<ICertificateStore, CertificateStore>();
        services.AddSingleton<IMiddlewareStore, MiddlewareStore>();
        services.AddSingleton<IAuthenticationPolicyStore, AuthenticationPolicyStore>();
        services.AddSingleton<IServiceDiscoveryStore, ServiceDiscoveryStore>();
        return services;
    }
}

