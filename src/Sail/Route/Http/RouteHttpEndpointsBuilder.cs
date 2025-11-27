using Sail.Extensions;
using Sail.Route.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Sail.Route.Http;

public static class RouteHttpEndpointsBuilder
{
    public static RouteGroupBuilder MapRouteApiV1(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api/routes").HasApiVersion(1.0);

        api.MapGet("/", List);
        api.MapGet("/{id:guid}", Get);
        api.MapPost("/", Create)
            .AddRequestValidation<RouteRequest>();
        api.MapPut("/{id:guid}", Update);
        api.MapDelete("/{id:guid}", Delete);
        return api;
    }

    private static async Task<Results<Ok<RouteResponse>, NotFound>> Get(RouteService service,
        Guid id,
        CancellationToken cancellationToken)
    {
        var items = await service.GetAsync(id, cancellationToken);
        return TypedResults.Ok(items);
    }
    private static async Task<Results<Ok<IEnumerable<RouteResponse>>, NotFound>> List(RouteService service,
        string? keywords,
        CancellationToken cancellationToken)
    {
        var items = await service.ListAsync(keywords, cancellationToken);
        return TypedResults.Ok(items);
    }

    private static async Task<Results<Created, ProblemHttpResult>> Create(RouteService service, RouteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);

        return result.Match<Results<Created, ProblemHttpResult>>(
            created => TypedResults.Created(string.Empty),
            errors => errors.HandleErrors()
        );
    }

    private static async Task<Results<Ok, ProblemHttpResult>> Update(RouteService service, Guid id,
        RouteRequest request, CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);

        return result.Match<Results<Ok, ProblemHttpResult>>(
            updated => TypedResults.Ok(),
            errors => errors.HandleErrors()
        );
    }

    private static async Task<Results<Ok, ProblemHttpResult>> Delete(RouteService service, Guid id,
        CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);

        return result.Match<Results<Ok, ProblemHttpResult>>(
            deleted => TypedResults.Ok(),
            errors => errors.HandleErrors()
        );
    }
}
