using Consul.AspNetCore;

namespace Sail.Proxy.Extensions;

public static class ServiceDiscoveryExtensions
{
    public static IServiceCollection AddSailServiceDiscovery(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var serviceDiscoverySection = configuration.GetSection("ServiceDiscovery");

        if (!serviceDiscoverySection.Exists())
        {
            return services;
        }

        var consulSection = serviceDiscoverySection.GetSection("Consul");
        var dnsSection = serviceDiscoverySection.GetSection("Dns");

        if (consulSection.Exists())
        {
            var address = consulSection["Address"];
            var token = consulSection["Token"];
            var datacenter = consulSection["Datacenter"];
            var refreshInterval = consulSection.GetValue("RefreshIntervalSeconds", 60);

            services.AddServiceDiscovery(sdOptions =>
            {
                sdOptions.RefreshPeriod = TimeSpan.FromSeconds(refreshInterval);
            })
            .AddConsulSrvServiceEndpointProvider();

            services.AddConsul(consulOptions =>
            {
                consulOptions.Address = new Uri(address!);
                
                if (!string.IsNullOrEmpty(token))
                    consulOptions.Token = token;
                
                if (!string.IsNullOrEmpty(datacenter))
                    consulOptions.Datacenter = datacenter;
            });
        }

        if (dnsSection.Exists())
        {
            var refreshInterval = dnsSection.GetValue("RefreshIntervalSeconds", 300);

            services.AddServiceDiscovery(sdOptions =>
            {
                sdOptions.RefreshPeriod = TimeSpan.FromSeconds(refreshInterval);
            })
            .AddDnsSrvServiceEndpointProvider();
        }

        return services;
    }
}