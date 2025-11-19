using Microsoft.AspNetCore.Http.HttpResults;
using Sail.Extensions;
using Sail.Models.Middlewares;
using Sail.Services;

namespace Sail.Apis;

public static class MiddlewareApi
{
    public static RouteGroupBuilder MapMiddlewareApiV1(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api/middlewares").HasApiVersion(1.0);

        api.MapGet("/", List);
        api.MapGet("/{id:guid}", Get);
        api.MapPost("/", Create);
        api.MapPut("/{id:guid}", Update);
        api.MapDelete("/{id:guid}", Delete);
        return api;
    }

    private static async Task<Results<Ok<MiddlewareResponse>, NotFound>> Get(
        MiddlewareService service,
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await service.GetAsync(id, cancellationToken);
        return item != null ? TypedResults.Ok(item) : TypedResults.NotFound();
    }

    private static async Task<Ok<IEnumerable<MiddlewareResponse>>> List(
        MiddlewareService service,
        string? keywords,
        CancellationToken cancellationToken)
    {
        var items = await service.ListAsync(keywords, cancellationToken);
        return TypedResults.Ok(items);
    }

    private static async Task<Results<Created, ProblemHttpResult>> Create(
        MiddlewareService service,
        MiddlewareRequest request,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);

        return result.Match<Results<Created, ProblemHttpResult>>(
            created => TypedResults.Created($"/api/middlewares/{created}"),
            errors => errors.HandleErrors()
        );
    }

    private static async Task<Results<Ok, ProblemHttpResult>> Update(
        MiddlewareService service,
        Guid id,
        MiddlewareRequest request,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);

        return result.Match<Results<Ok, ProblemHttpResult>>(
            updated => TypedResults.Ok(),
            errors => errors.HandleErrors()
        );
    }

    private static async Task<Results<Ok, ProblemHttpResult>> Delete(
        MiddlewareService service,
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);

        return result.Match<Results<Ok, ProblemHttpResult>>(
            deleted => TypedResults.Ok(),
            errors => errors.HandleErrors()
        );
    }
}

