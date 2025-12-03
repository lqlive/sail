using ErrorOr;
using Sail.Core.Entities;
using Sail.Core.Stores;
using Sail.Route.Models;
using Sail.Route.Errors;
using Sail.Route.Validators;
using RouteEntity = Sail.Core.Entities.Route;

namespace Sail.Route;

public class RouteService(
    IRouteStore routeStore, 
    IClusterStore clusterStore,
    RoutePolicyValidator policyValidator)
{
    public async Task<RouteResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var route = await routeStore.GetByIdAsync(id, cancellationToken);
        return route != null ? MapToRoute(route) : null;
    }

    public async Task<IEnumerable<RouteResponse>> ListAsync(string? keywords,
        CancellationToken cancellationToken = default)
    {
        var routes = await routeStore.GetAsync(cancellationToken);
        return routes.Select(MapToRoute);
    }

    public async Task<ErrorOr<Created>> CreateAsync(RouteRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.ClusterId.HasValue)
        {
            return RouteErrors.ClusterRequired;
        }

        var cluster = await clusterStore.GetByIdAsync(request.ClusterId.Value, cancellationToken);
        if (cluster is null)
        {
            return RouteErrors.ClusterNotFound;
        }

        var validationErrors = await policyValidator.ValidatePoliciesAsync(request, cancellationToken);
        if (validationErrors.Count > 0)
        {
            return validationErrors;
        }

        var route = CreateRouteFromRequest(request);
        await routeStore.CreateAsync(route, cancellationToken);
        return Result.Created;
    }

    public async Task<ErrorOr<Updated>> UpdateAsync(Guid id, RouteRequest request,
        CancellationToken cancellationToken = default)
    {
        var route = await routeStore.GetByIdAsync(id, cancellationToken);
        if (route is null)
        {
            return Error.NotFound(description: "Route not found");
        }

        if (request.ClusterId.HasValue)
        {
            var cluster = await clusterStore.GetByIdAsync(request.ClusterId.Value, cancellationToken);
            if (cluster is null)
            {
                return RouteErrors.ClusterNotFound;
            }
        }

        var validationErrors = await policyValidator.ValidatePoliciesAsync(request, cancellationToken);
        if (validationErrors.Count > 0)
        {
            return validationErrors;
        }

        route.Name = request.Name;
        route.Enabled = request.Enabled;
        route.ClusterId = request.ClusterId;
        route.Match = CreateRouteMatchFromRequest(request.Match);
        route.AuthorizationPolicy = request.AuthorizationPolicy;
        route.RateLimiterPolicy = request.RateLimiterPolicy;
        route.CorsPolicy = request.CorsPolicy;
        route.TimeoutPolicy = request.TimeoutPolicy;
        route.RetryPolicy = request.RetryPolicy;
        route.Timeout = request.Timeout;
        route.MaxRequestBodySize = request.MaxRequestBodySize;
        route.HttpsRedirect = request.HttpsRedirect;
        route.Transforms = request.Transforms;
        route.UpdatedAt = DateTimeOffset.UtcNow;

        await routeStore.UpdateAsync(route, cancellationToken);
        return Result.Updated;
    }

    public async Task<ErrorOr<Deleted>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await routeStore.DeleteAsync(id, cancellationToken);
        return Result.Deleted;
    }

    private RouteEntity CreateRouteFromRequest(RouteRequest request)
    {
        var route = new RouteEntity
        {
            Name = request.Name,
            Enabled = request.Enabled,
            ClusterId = request.ClusterId,
            Match = CreateRouteMatchFromRequest(request.Match),
            Order = request.Order,
            AuthorizationPolicy = request.AuthorizationPolicy,
            RateLimiterPolicy = request.RateLimiterPolicy,
            CorsPolicy = request.CorsPolicy,
            TimeoutPolicy = request.TimeoutPolicy,
            RetryPolicy = request.RetryPolicy,
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

    private RouteResponse MapToRoute(RouteEntity route)
    {
        return new RouteResponse
        {
            Id = route.Id,
            ClusterId = route.ClusterId,
            Name = route.Name,
            Enabled = route.Enabled ?? true,
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
            RetryPolicy = route.RetryPolicy,
            Timeout = route.Timeout,
            MaxRequestBodySize = route.MaxRequestBodySize,
            HttpsRedirect = route.HttpsRedirect,
            Transforms = route.Transforms,
            CreatedAt = route.CreatedAt,
            UpdatedAt = route.UpdatedAt
        };
    }
}
