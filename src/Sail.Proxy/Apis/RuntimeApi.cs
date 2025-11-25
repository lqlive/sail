using Yarp.ReverseProxy;

namespace Sail.Proxy.Apis;

public static class RuntimeApi
{
    public static RouteGroupBuilder MapRuntimeV1(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/runtime");

        group.MapGet("/clusters", GetAllClusters)
            .WithName("GetAllClusterRuntimeStates")
            .WithDescription("Get runtime states of all clusters");

        group.MapGet("/clusters/{clusterId}/destinations", GetAllDestinations)
            .WithName("GetClusterDestinationRuntimeStates")
            .WithDescription("Get runtime states of all destinations for a specific cluster");

        return group;
    }

    private static IResult GetAllClusters(IProxyStateLookup proxyStateLookup)
    {
        var clusters = proxyStateLookup.GetClusters();

        var result = clusters.Select(cluster => new
        {
            ClusterId = cluster.ClusterId,
            DestinationCount = cluster.Destinations.Count,
            Destinations = cluster.Destinations.Values.Select(dest => new
            {
                DestinationId = dest.DestinationId,
                Address = dest.Model.Config.Address,
                Host = dest.Model.Config.Host,
                ActiveHealth = dest.Health.Active.ToString(),
                PassiveHealth = dest.Health.Passive.ToString()
            })
        });

        return Results.Ok(result);
    }

    private static IResult GetAllDestinations(
        IProxyStateLookup proxyStateLookup,
        string clusterId)
    {
        if (!proxyStateLookup.TryGetCluster(clusterId, out var cluster))
        {
            return Results.NotFound(new
            {
                Error = "Cluster not found in YARP runtime",
                ClusterId = clusterId
            });
        }

        var destinations = cluster.Destinations.Values.Select(dest => new
        {
            ClusterId = cluster.ClusterId,
            DestinationId = dest.DestinationId,
            Address = dest.Model.Config.Address,
            Host = dest.Model.Config.Host,
            ActiveHealth = dest.Health.Active.ToString(),
            PassiveHealth = dest.Health.Passive.ToString()
        });

        return Results.Ok(destinations);
    }
}
