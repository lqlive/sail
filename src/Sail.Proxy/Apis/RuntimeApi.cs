using Yarp.ReverseProxy;

namespace Sail.Proxy.Apis;

public static class RuntimeApi
{
    public static RouteGroupBuilder MapRuntimeV1(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/runtime");

        group.MapGet("/clusters", GetAllClusters);
        group.MapGet("/clusters/{clusterId}/destinations", GetAllDestinations);

        return group;
    }

    private static IResult GetAllClusters(IProxyStateLookup proxyStateLookup)
    {
        var clusters = proxyStateLookup.GetClusters();

        var result = clusters.Select(cluster => new
        {
            cluster.ClusterId,
            DestinationCount = cluster.Destinations.Count,
            Destinations = cluster.Destinations.Values.Select(dest => new
            {
                dest.DestinationId,
                dest.Model.Config.Address,
                dest.Model.Config.Host,
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
            cluster.ClusterId,
            dest.DestinationId,
            dest.Model.Config.Address,
            dest.Model.Config.Host,
            ActiveHealth = dest.Health.Active.ToString(),
            PassiveHealth = dest.Health.Passive.ToString()
        });

        return Results.Ok(destinations);
    }
}
