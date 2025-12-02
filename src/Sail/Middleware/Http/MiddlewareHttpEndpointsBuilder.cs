using Microsoft.AspNetCore.Http.HttpResults;
using Sail.Extensions;
using Sail.Middleware.Models;

namespace Sail.Middleware.Http;

public static class MiddlewareHttpEndpointsBuilder
{
    public static RouteGroupBuilder MapMiddlewareApiV1(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api/middlewares").HasApiVersion(1.0);

        api.MapGet("/", List);
        api.MapGet("/{id:guid}", Get);
        api.MapPost("/", Create)
            .AddRequestValidation<MiddlewareRequest>();
        api.MapPut("/{id:guid}", Update)
            .AddRequestValidation<MiddlewareRequest>();
        api.MapDelete("/{id:guid}", Delete);
        return api;
    }

    private static async Task<Results<Ok<MiddlewareResponse>, NotFound>> Get(
        Middleware.MiddlewareService service,
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await service.GetAsync(id, cancellationToken);
        return item != null ? TypedResults.Ok(item) : TypedResults.NotFound();
    }

    private static async Task<Ok<IEnumerable<MiddlewareResponse>>> List(
        Middleware.MiddlewareService service,
        string? keywords,
        CancellationToken cancellationToken)
    {
        var items = await service.ListAsync(keywords, cancellationToken);
        return TypedResults.Ok(items);
    }

    private static async Task<Results<Created, ProblemHttpResult>> Create(
        Middleware.MiddlewareService service,
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
        Middleware.MiddlewareService service,
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
        Middleware.MiddlewareService service,
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