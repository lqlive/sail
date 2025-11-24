using Consul;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.ServiceDiscovery;
using Sail.ServiceDiscovery.Consul.Internal;

namespace Sail.ServiceDiscovery.Consul;

internal sealed class ConsulServiceEndpointProvider(
    ServiceEndpointQuery query,
    string hostName,
    IOptionsMonitor<ConsulServiceEndpointProviderOptions> options,
    ILogger<ConsulServiceEndpointProvider> logger,
    IConsulClient client,
    TimeProvider timeProvider) :
    ConsulServiceEndpointProviderBase(query, logger, timeProvider), IHostNameFeature
{
    protected override double RetryBackOffFactor { get; } = options.CurrentValue.RetryBackOffFactor;
    protected override TimeSpan MinRetryPeriod { get; } = options.CurrentValue.MinRetryPeriod;
    protected override TimeSpan MaxRetryPeriod { get; } = options.CurrentValue.MaxRetryPeriod;
    protected override TimeSpan DefaultRefreshPeriod { get; } = options.CurrentValue.DefaultRefreshPeriod;

    public string HostName { get; } = hostName;

    protected override async Task ResolveAsyncCore()
    {
        var endpoints = new List<ServiceEndpoint>();
        var ttl = DefaultRefreshPeriod;
        Log.AddressQuery(logger, ServiceName, hostName);
        var result = await client.Health.Service(hostName).ConfigureAwait(false);
        foreach (var service in result.Response)
        {
            var address = $"{service.Service.Address}:{service.Service.Port}";
            if (ServiceNameParts.TryCreateEndPoint(address, out var endpoint))
            {
                var serviceEndpoint = ServiceEndpoint.Create(endpoint);
                serviceEndpoint.Features.Set<IServiceEndpointProvider>(this);
                endpoints.Add(serviceEndpoint);
            }
        }

        SetResult(endpoints, ttl);
    }
}