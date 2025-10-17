using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Sail.Compass.Hosting;

public static class ServiceCollectionHostedServiceAdapterExtensions
{
    public static IServiceCollection RegisterHostedService<TService>(this IServiceCollection services)
        where TService : IHostedService
    {
        if (services.All(serviceDescriptor => serviceDescriptor.ServiceType != typeof(HostedServiceAdapter<TService>)))
        {
            services = services.AddHostedService<HostedServiceAdapter<TService>>();
        }

        return services;
    }
}