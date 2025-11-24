using Microsoft.Extensions.DependencyInjection;
using Sail.Core.Options;

namespace Sail.Core.Management;

public static class SailApplicationExtensions
{
    public static SailApplication AddSailCore(this IServiceCollection services)
    {
        var app = new SailApplication(services);
        services.AddConfiguration();
        return app;
    }

    private static IServiceCollection AddConfiguration(this IServiceCollection services)
    {
        services.AddOptions<DatabaseOptions>().BindConfiguration(DatabaseOptions.Name);
        services.AddOptions<CertificateOptions>().BindConfiguration(CertificateOptions.Name);
        services.AddOptions<ReceiverOptions>().BindConfiguration(ReceiverOptions.Name);
        return services;
    }
}