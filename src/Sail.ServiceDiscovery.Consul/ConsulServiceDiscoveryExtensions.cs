using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ServiceDiscovery;
using Sail.ServiceDiscovery.Consul;

namespace Microsoft.Extensions.Hosting;

public static class ConsulServiceDiscoveryExtensions
{
    public static IServiceCollection AddConsulSrvServiceEndpointProvider(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddServiceDiscoveryCore();
        services.AddSingleton<IServiceEndpointProviderFactory, ConsulServiceEndpointProviderFactory>();
        return services;
    }
}