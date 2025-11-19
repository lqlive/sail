using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sail.Api.V1;
using Sail.Compass.Certificates;
using Sail.Compass.ConfigProvider;
using Sail.Compass.Cors;
using Sail.Compass.Observers;
using Sail.Compass.RateLimiter;
using Sail.Core.Options;
using Yarp.ReverseProxy.Configuration;

namespace Sail.Compass.Management;

public static class ServiceCollectionExtensions
{
    private static IServiceCollection AddResourceGrpcClient(this IServiceCollection services)
    {
        services.AddGrpcClient<ClusterService.ClusterServiceClient>((sp, o) =>
        {
            var receiverOptions = sp.GetRequiredService<IOptions<ReceiverOptions>>().Value;
            o.Address = new Uri("http://localhost:8000");
        });
        services.AddGrpcClient<RouteService.RouteServiceClient>((sp, o) =>
        {
            var receiverOptions = sp.GetRequiredService<IOptions<ReceiverOptions>>().Value;
            o.Address = new Uri("http://localhost:8000");
        });
        services.AddGrpcClient<CertificateService.CertificateServiceClient>((sp, o) =>
        {
            var receiverOptions = sp.GetRequiredService<IOptions<ReceiverOptions>>().Value;
            o.Address = new Uri("http://localhost:8000");
        });
        services.AddGrpcClient<MiddlewareService.MiddlewareServiceClient>((sp, o) =>
        {
            var receiverOptions = sp.GetRequiredService<IOptions<ReceiverOptions>>().Value;
            o.Address = new Uri("http://localhost:8000");
        });

        return services;
    }
    public static IReverseProxyBuilder LoadFromMessages(this IReverseProxyBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddResourceGrpcClient();

        builder.Services.AddSingleton<ResourceObserver<Cluster>, ClusterObserver>();
        builder.Services.AddSingleton<ResourceObserver<Route>, RouteObserver>();
        builder.Services.AddSingleton<ResourceObserver<Certificate>, CertificateObserver>();
        builder.Services.AddSingleton<ResourceObserver<Middleware>, MiddlewareObserver>();

        builder.Services.AddSingleton<ProxyConfigProvider>();
        builder.Services.AddSingleton<IProxyConfigProvider>(sp => 
            sp.GetRequiredService<ProxyConfigProvider>());
        
        return builder;
    }

    public static IServiceCollection AddCertificateUpdater(
        this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var certificateObserver = sp.GetRequiredService<ResourceObserver<Certificate>>();
            return CertificateStreamBuilder.BuildCertificateStream(certificateObserver);
        });

        services.AddSingleton<ServerCertificateUpdater>();

        return services;
    }

    public static IServiceCollection AddCorsPolicyUpdater(
        this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var middlewareObserver = sp.GetRequiredService<ResourceObserver<Middleware>>();
            return CorsPolicyStreamBuilder.BuildCorsPolicyStream(middlewareObserver);
        });

        services.AddSingleton<CorsPolicyUpdater>();

        return services;
    }

    public static IServiceCollection AddRateLimiterPolicyUpdater(
        this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var middlewareObserver = sp.GetRequiredService<ResourceObserver<Middleware>>();
            return RateLimiterPolicyStreamBuilder.BuildRateLimiterPolicyStream(middlewareObserver);
        });

        services.AddSingleton<RateLimiterPolicyUpdater>();

        return services;
    }

    public static void UseCompassUpdaters(this IServiceProvider serviceProvider)
    {
        _ = serviceProvider.GetRequiredService<ServerCertificateUpdater>();
        _ = serviceProvider.GetRequiredService<CorsPolicyUpdater>();
        _ = serviceProvider.GetRequiredService<RateLimiterPolicyUpdater>();
    }
}
