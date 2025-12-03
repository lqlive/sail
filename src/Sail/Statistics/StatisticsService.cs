using Sail.Core.Stores;
using Sail.Statistics.Models;

namespace Sail.Statistics;

public class StatisticsService(
    IRouteStore routeStore,
    IClusterStore clusterStore,
    ICertificateStore certificateStore,
    IMiddlewareStore middlewareStore,
    IAuthenticationPolicyStore authPolicyStore)
{
    public async Task<ResourceCountResponse> GetRouteStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var routes = await routeStore.GetAsync(cancellationToken);
        return new ResourceCountResponse
        {
            Total = routes.Count,
            Enabled = routes.Count(r => r.Enabled ?? true)
        };
    }

    public async Task<ResourceCountResponse> GetClusterStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var clusters = await clusterStore.GetAsync(cancellationToken);
        return new ResourceCountResponse
        {
            Total = clusters.Count,
            Enabled = clusters.Count(c => c.Enabled ?? true)
        };
    }

    public async Task<ResourceCountResponse> GetCertificateStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var certificates = await certificateStore.GetAsync(cancellationToken);
        return new ResourceCountResponse
        {
            Total = certificates.Count,
            Enabled = certificates.Count
        };
    }

    public async Task<ResourceCountResponse> GetMiddlewareStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var middlewares = await middlewareStore.GetAsync(cancellationToken);
        return new ResourceCountResponse
        {
            Total = middlewares.Count,
            Enabled = middlewares.Count(m => m.Enabled)
        };
    }

    public async Task<ResourceCountResponse> GetAuthenticationPolicyStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var authPolicies = await authPolicyStore.GetAsync(cancellationToken);
        return new ResourceCountResponse
        {
            Total = authPolicies.Count,
            Enabled = authPolicies.Count(p => p.Enabled)
        };
    }

    public async Task<RecentItemsResponse> GetRecentRoutesAsync(CancellationToken cancellationToken = default)
    {
        var routes = await routeStore.GetAsync(cancellationToken);
        var items = routes
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .Select(r => new RecentItem
            {
                Id = r.Id,
                Name = r.Name,
                CreatedAt = r.CreatedAt
            })
            .ToList();

        return new RecentItemsResponse { Items = items };
    }

    public async Task<RecentItemsResponse> GetRecentClustersAsync(CancellationToken cancellationToken = default)
    {
        var clusters = await clusterStore.GetAsync(cancellationToken);
        var items = clusters
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .Select(c => new RecentItem
            {
                Id = c.Id,
                Name = c.Name,
                CreatedAt = c.CreatedAt
            })
            .ToList();

        return new RecentItemsResponse { Items = items };
    }

    public async Task<RecentItemsResponse> GetRecentCertificatesAsync(CancellationToken cancellationToken = default)
    {
        var certificates = await certificateStore.GetAsync(cancellationToken);
        var items = certificates
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .Select(c => new RecentItem
            {
                Id = c.Id,
                Name = c.Name ?? $"Certificate-{c.Id}",
                CreatedAt = c.CreatedAt
            })
            .ToList();

        return new RecentItemsResponse { Items = items };
    }
}

