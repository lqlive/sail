using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MongoDB.Driver;
using Sail.Api.V1;
using Sail.Core.Stores;
using Sail.Database.MongoDB;
using Sail.Database.MongoDB.Extensions;
using Route = Sail.Core.Entities.Route;
using RouteResponse = Sail.Api.V1.Route;

namespace Sail.Grpc;

public class RouteGrpcService(MongoDBContext dbContext, IRouteStore routeStore) : RouteService.RouteServiceBase
{
    public override async Task<ListRouteResponse> List(Empty request, ServerCallContext context)
    {
        var clusters = await routeStore.GetAsync(CancellationToken.None);
        var response = MapToRouteItemsResponse(clusters);
        return response;
    }

    public override async Task Watch(Empty request, IServerStreamWriter<WatchRouteResponse> responseStream,
        ServerCallContext context)
    {
        var options = new ChangeStreamOptions
        {
            FullDocument = ChangeStreamFullDocumentOption.Required,
            FullDocumentBeforeChange = ChangeStreamFullDocumentBeforeChangeOption.Required
        };

        while (!context.CancellationToken.IsCancellationRequested)
        {
            var watch = await dbContext.Routes.WatchAsync(options);

            await foreach (var changeStreamDocument in watch.ToAsyncEnumerable())
            {
                var document = changeStreamDocument.FullDocument;

                if (changeStreamDocument.OperationType == ChangeStreamOperationType.Delete)
                {
                    document = changeStreamDocument.FullDocumentBeforeChange;
                }

                var eventType = changeStreamDocument.OperationType switch
                {
                    ChangeStreamOperationType.Insert => EventType.Create,
                    ChangeStreamOperationType.Update => EventType.Update,
                    ChangeStreamOperationType.Delete => EventType.Delete,
                    _ => EventType.Unknown
                };
                var route = MapToRouteResponse(document);

                var response = new WatchRouteResponse
                {
                    Route = route,
                    EventType = eventType
                };
                await responseStream.WriteAsync(response);
            }
        }
    }

    private static ListRouteResponse MapToRouteItemsResponse(List<Route> routes)
    {
        var items = routes.Select(MapToRouteResponse);

        var response = new ListRouteResponse
        {
            Items = { items }
        };
        return response;
    }

    private static RouteResponse MapToRouteResponse(Route route)
    {
        return new RouteResponse
        {
            RouteId = route.Id.ToString(),
            Match = new RouteMatch
            {
                Path = route.Match.Path,
                Hosts = { route.Match.Hosts },
                Methods = { route.Match.Methods },
                Headers =
                {
                    route.Match.Headers.Select(h => new RouteMatch.Types.RouteHeader
                    {
                        Name = h.Name,
                        Mode = (RouteMatch.Types.RouteHeader.Types.HeaderMatchMode)h.Mode,
                        Values = { h.Values ?? [] },
                        IsCaseSensitive = h.IsCaseSensitive
                    })
                },
                QueryParameters =
                {
                    route.Match.QueryParameters.Select(q => new RouteMatch.Types.RouteQueryParameter
                    {
                        Name = q.Name,
                        Mode = (RouteMatch.Types.RouteQueryParameter.Types.QueryParameterMatchMode)q.Mode,
                        Values = { q.Values ?? [] },
                        IsCaseSensitive = q.IsCaseSensitive
                    })
                }
            },
            Order = route.Order,
            ClusterId = route.ClusterId?.ToString(),
            CorsPolicy = route.CorsPolicy,
            Timeout = route.Timeout?.ToString(),
            TimeoutPolicy = route.TimeoutPolicy,
            AuthorizationPolicy = route.AuthorizationPolicy,
            MaxRequestBodySize = route.MaxRequestBodySize,
            Transforms = { route.Transforms?.Select(MapToRouteTransform) ?? [] },
            RateLimiterPolicy = route.RateLimiterPolicy,
            HttpsRedirect = route.HttpsRedirect ?? false
        };
    }

    private static RouteTransform MapToRouteTransform(Dictionary<string, string> transform)
    {
        var result = new RouteTransform();
        foreach (var item in transform)
        {
            result.Properties[item.Key] = item.Value;
        }

        return result;
    }
}