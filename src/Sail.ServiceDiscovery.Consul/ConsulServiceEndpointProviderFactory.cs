using Consul;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.ServiceDiscovery;

namespace Sail.ServiceDiscovery.Consul;

internal sealed class ConsulServiceEndpointProviderFactory(
    IOptionsMonitor<ConsulServiceEndpointProviderOptions> options,
    ILogger<ConsulServiceEndpointProvider> logger,
    TimeProvider timeProvider,
    IConsulClient client) : IServiceEndpointProviderFactory
{
    public bool TryCreateProvider(ServiceEndpointQuery query, out IServiceEndpointProvider? provider)
    {
        provider = new ConsulServiceEndpointProvider(query, query.ServiceName, options, logger, client, timeProvider);
        return true;
    }
}