using FluentValidation;
using Sail.Core.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Sail.Extensions;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        services.AddOptions<DatabaseOptions>()
            .BindConfiguration(DatabaseOptions.Name);

        services.TryAddScoped<Route.RouteService>();
        services.TryAddScoped<Route.Validators.RoutePolicyValidator>();
        services.TryAddScoped<Cluster.ClusterService>();
        services.TryAddScoped<Certificate.CertificateService>();
        services.TryAddScoped<Middleware.MiddlewareService>();
        services.TryAddScoped<AuthenticationPolicy.AuthenticationPolicyService>();
        services.TryAddScoped<Statistics.StatisticsService>();

        services.AddValidatorsFromAssemblyContaining<Program>();

        services.AddSailCors();
        services.AddSailRateLimiter();
        services.AddSailRetry();
    }
}