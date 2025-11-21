using ErrorOr;
using MongoDB.Driver;
using Sail.Core.Entities;
using Sail.Database.MongoDB;
using Sail.Models.Routes;
using Route = Sail.Core.Entities.Route;

namespace Sail.Services;

public class RouteService(SailContext context)
{
    
    public async Task<RouteResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Route>.Filter.Where(x => x.Id == id);
        var routes = await context.Routes.FindAsync(filter, cancellationToken: cancellationToken);
        var result = await routes.SingleOrDefaultAsync(cancellationToken: cancellationToken);
        return MapToRoute(result);
    }

    
    public async Task<IEnumerable<RouteResponse>> ListAsync(string? keywords,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<Route>.Filter.Empty;
        var routes = await context.Routes.FindAsync(filter, cancellationToken: cancellationToken);
        var items = await routes.ToListAsync(cancellationToken: cancellationToken);
        return items.Select(MapToRoute);
    }

    public async Task<ErrorOr<Created>> CreateAsync(RouteRequest request, CancellationToken cancellationToken = default)
    {
        var route = CreateRouteFromRequest(request);
        await context.Routes.InsertOneAsync(route, cancellationToken: cancellationToken);
        return Result.Created;
    }

    public async Task<ErrorOr<Updated>> UpdateAsync(Guid id, RouteRequest request,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<Route>.Filter.And(Builders<Route>.Filter.Where(x => x.Id == id));

        var update = Builders<Route>.Update
            .Set(x => x.Name, request.Name)
            .Set(x => x.ClusterId, request.ClusterId)
            .Set(x => x.Match, CreateRouteMatchFromRequest(request.Match))
            .Set(x => x.AuthorizationPolicy, request.AuthorizationPolicy)
            .Set(x => x.RateLimiterPolicy, request.RateLimiterPolicy) 
            .Set(x => x.CorsPolicy, request.CorsPolicy)
            .Set(x => x.TimeoutPolicy, request.TimeoutPolicy)
            .Set(x => x.Timeout, request.Timeout)
            .Set(x => x.MaxRequestBodySize, request.MaxRequestBodySize)
            .Set(x => x.HttpsRedirect, request.HttpsRedirect)
            .Set(x=>x.Transforms,request.Transforms)
            .Set(x => x.UpdatedAt, DateTimeOffset.UtcNow);

        await context.Routes.FindOneAndUpdateAsync(filter, update, cancellationToken: cancellationToken);
        return Result.Updated;
    }

    public async Task<ErrorOr<Deleted>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Route>.Filter.And(Builders<Route>.Filter.Where(x => x.Id == id));
        await context.Routes.DeleteOneAsync(filter, cancellationToken);
        return Result.Deleted;
    }

    private Route CreateRouteFromRequest(RouteRequest request)
    {
        var route = new Route
        {
            Name = request.Name,
            ClusterId = request.ClusterId,
            Match = CreateRouteMatchFromRequest(request.Match),
            Order = request.Order,
            AuthorizationPolicy = request.AuthorizationPolicy,
            RateLimiterPolicy = request.RateLimiterPolicy,
            CorsPolicy = request.CorsPolicy,
            TimeoutPolicy = request.TimeoutPolicy,
            Timeout = request.Timeout,
            MaxRequestBodySize = request.MaxRequestBodySize,
            HttpsRedirect = request.HttpsRedirect,
            Transforms = request.Transforms
        };
        return route;
    }

    private RouteMatch CreateRouteMatchFromRequest(RouteMatchRequest match)
    {
        return new RouteMatch
        {
            Path = match.Path,
            Hosts = match.Hosts ?? [],
            Methods = match.Methods ?? [],
            QueryParameters = match.QueryParameters?.Select(CreateQueryParameterFromRequest).ToList() ?? [],
            Headers = match.Headers?.Select(CreateRouteHeaderFromRequest).ToList() ?? []
        };
    }

    private RouteQueryParameter CreateQueryParameterFromRequest(QueryParameterRequest queryParameter)
    {
        return new RouteQueryParameter
        {
            Name = queryParameter.Name,
            Values = queryParameter.Values,
            Mode = queryParameter.Mode,
            IsCaseSensitive = queryParameter.IsCaseSensitive
        };
    }

    private RouteHeader CreateRouteHeaderFromRequest(RouteHeaderRequest header)
    {
        return new RouteHeader
        {
            Name = header.Name,
            Values = header.Values,
            Mode = header.Mode,
            IsCaseSensitive = header.IsCaseSensitive
        };
    }

    private RouteResponse MapToRoute(Route route)
    {
        return new RouteResponse
        {
            Id = route.Id,
            ClusterId = route.ClusterId,
            Name = route.Name,
            Match = new RouteMatchResponse
            {
                Path = route.Match.Path,
                Hosts = route.Match.Hosts,
                Methods = route.Match.Methods,
                Headers = route.Match.Headers.Select(h => new RouteHeaderResponse
                {
                    Name = h.Name,
                    Mode = h.Mode,
                    Values = h.Values ?? [],
                    IsCaseSensitive = h.IsCaseSensitive

                }),
                QueryParameters = route.Match.QueryParameters.Select(q => new RouteQueryParameterResponse
                {
                    Name = q.Name,
                    Mode = q.Mode,
                    Values = q.Values ?? [],
                    IsCaseSensitive = q.IsCaseSensitive
                })
            },
            Order = route.Order,
            AuthorizationPolicy = route.AuthorizationPolicy,
            RateLimiterPolicy = route.RateLimiterPolicy,
            CorsPolicy = route.CorsPolicy,
            TimeoutPolicy = route.TimeoutPolicy,
            Timeout = route.Timeout,
            MaxRequestBodySize = route.MaxRequestBodySize,
            HttpsRedirect = route.HttpsRedirect,
            Transforms = route.Transforms,
            CreatedAt = route.CreatedAt,
            UpdatedAt = route.UpdatedAt
        };
    }
}