using Microsoft.AspNetCore.Http.HttpResults;
using Sail.Statistics.Models;

namespace Sail.Statistics.Http;

public static class StatisticsHttpEndpointsBuilder
{
    public static RouteGroupBuilder MapStatisticsApiV1(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api/statistics").HasApiVersion(1.0);

        api.MapGet("/resources/routes", GetRouteStatistics);
        api.MapGet("/resources/clusters", GetClusterStatistics);
        api.MapGet("/resources/certificates", GetCertificateStatistics);
        api.MapGet("/resources/middlewares", GetMiddlewareStatistics);
        api.MapGet("/resources/authentication-policies", GetAuthenticationPolicyStatistics);
        api.MapGet("/recent/routes", GetRecentRoutes);
        api.MapGet("/recent/clusters", GetRecentClusters);
        api.MapGet("/recent/certificates", GetRecentCertificates);

        return api;
    }

    private static async Task<Ok<RecentItemsResponse>> GetRecentRoutes(
        StatisticsService service,
        CancellationToken cancellationToken)
    {
        var items = await service.GetRecentRoutesAsync(cancellationToken);
        return TypedResults.Ok(items);
    }

    private static async Task<Ok<RecentItemsResponse>> GetRecentClusters(
        StatisticsService service,
        CancellationToken cancellationToken)
    {
        var items = await service.GetRecentClustersAsync(cancellationToken);
        return TypedResults.Ok(items);
    }

    private static async Task<Ok<RecentItemsResponse>> GetRecentCertificates(
        StatisticsService service,
        CancellationToken cancellationToken)
    {
        var items = await service.GetRecentCertificatesAsync(cancellationToken);
        return TypedResults.Ok(items);
    }

    private static async Task<Ok<ResourceCountResponse>> GetRouteStatistics(
        StatisticsService service,
        CancellationToken cancellationToken)
    {
        var count = await service.GetRouteStatisticsAsync(cancellationToken);
        return TypedResults.Ok(count);
    }

    private static async Task<Ok<ResourceCountResponse>> GetClusterStatistics(
        StatisticsService service,
        CancellationToken cancellationToken)
    {
        var count = await service.GetClusterStatisticsAsync(cancellationToken);
        return TypedResults.Ok(count);
    }

    private static async Task<Ok<ResourceCountResponse>> GetCertificateStatistics(
        StatisticsService service,
        CancellationToken cancellationToken)
    {
        var count = await service.GetCertificateStatisticsAsync(cancellationToken);
        return TypedResults.Ok(count);
    }

    private static async Task<Ok<ResourceCountResponse>> GetMiddlewareStatistics(
        StatisticsService service,
        CancellationToken cancellationToken)
    {
        var count = await service.GetMiddlewareStatisticsAsync(cancellationToken);
        return TypedResults.Ok(count);
    }

    private static async Task<Ok<ResourceCountResponse>> GetAuthenticationPolicyStatistics(
        StatisticsService service,
        CancellationToken cancellationToken)
    {
        var count = await service.GetAuthenticationPolicyStatisticsAsync(cancellationToken);
        return TypedResults.Ok(count);
    }
}

