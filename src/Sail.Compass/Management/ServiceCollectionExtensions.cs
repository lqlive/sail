using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sail.Api.V1;
using Sail.Compass.Authentication;
using Sail.Compass.Certificates;
using Sail.Compass.ConfigProvider;
using Sail.Compass.Cors;
using Sail.Compass.Observers;
using Sail.Compass.RateLimiter;
using Sail.Compass.Retry;
using Sail.Compass.Timeout;
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
        services.AddGrpcClient<AuthenticationPolicyService.AuthenticationPolicyServiceClient>((sp, o) =>
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
        builder.Services.AddSingleton<AuthenticationPolicyObserver>();

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

        services.AddHostedService<ServerCertificateUpdater>();

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

        services.AddHostedService<CorsPolicyUpdater>();

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

        services.AddHostedService<RateLimiterPolicyUpdater>();

        return services;
    }

    public static IServiceCollection AddAuthenticationPolicyUpdater(
        this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var policyObserver = sp.GetRequiredService<AuthenticationPolicyObserver>();
            return AuthenticationPolicyStreamBuilder.Build(policyObserver);
        });

        services.AddHostedService<AuthenticationPolicyUpdater>();

        return services;
    }

    public static IServiceCollection AddTimeoutPolicyUpdater(
        this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var middlewareObserver = sp.GetRequiredService<ResourceObserver<Middleware>>();
            return TimeoutPolicyStreamBuilder.BuildTimeoutPolicyStream(middlewareObserver);
        });

        services.AddHostedService<TimeoutPolicyUpdater>();

        return services;
    }

    public static IServiceCollection AddRetryPolicyUpdater(
        this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var middlewareObserver = sp.GetRequiredService<ResourceObserver<Middleware>>();
            return RetryPolicyStreamBuilder.BuildRetryPolicyStream(middlewareObserver);
        });

        services.AddHostedService<RetryPolicyUpdater>();

        return services;
    }
}
