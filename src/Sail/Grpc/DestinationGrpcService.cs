using Grpc.Core;
using Sail.Api.V1;
using Sail.Core.Stores;
using Destination = Sail.Core.Entities.Destination;
using DestinationResponse = Sail.Api.V1.Destination;

namespace Sail.Grpc;

public class DestinationGrpcService(IClusterStore clusterStore)
    : DestinationService.DestinationServiceBase
{
    public override async Task StreamDestinations(DestinationRequest request,
        IServerStreamWriter<DestinationItems> responseStream, ServerCallContext context)
    {
        while (!context.CancellationToken.IsCancellationRequested)
        {
            var clusters = await clusterStore.GetAsync(CancellationToken.None);
            var destinations = clusters.SelectMany(c => c.Destinations ?? []);

            var response = MapToDiscoveryResponse(destinations);
            await responseStream.WriteAsync(response);
            await Task.Delay(TimeSpan.FromSeconds(20));
        }
    }

    private static DestinationItems MapToDiscoveryResponse(IEnumerable<Destination> destinations)
    {
        var items = destinations.Select(MapToRouteResponse);

        var response = new DestinationItems
        {
            Items = { items }
        };
        return response;
    }

    private static DestinationResponse MapToRouteResponse(Destination destination)
    {
        return new DestinationResponse
        {
            Address = destination.Address,
            Health = destination.Health,
            Host = destination.Host
        };
    }
}