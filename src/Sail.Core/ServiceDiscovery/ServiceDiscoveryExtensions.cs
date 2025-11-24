using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.ServiceDiscovery;

namespace Sail.Core.ServiceDiscovery;

public static class ServiceDiscoveryExtensions
{
    public static IReverseProxyBuilder AddServiceDiscoveryDestinationResolver(this IReverseProxyBuilder builder)
    {
        builder.Services.AddSingleton<IDestinationResolver, ServiceDiscoveryDestinationResolver>();
        return builder;
    }
}

