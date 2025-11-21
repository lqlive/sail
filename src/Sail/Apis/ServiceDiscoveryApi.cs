using Microsoft.AspNetCore.Http.HttpResults;
using Sail.Extensions;
using Sail.Models.ServiceDiscoveries;
using Sail.Services;

namespace Sail.Apis;

public static class ServiceDiscoveryApi
{
    public static RouteGroupBuilder MapServiceDiscoveryApiV1(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api/service-discoveries").HasApiVersion(1.0);

        api.MapGet("/", List);
        api.MapGet("/{id:guid}", Get);
        api.MapPost("/", Create);
        api.MapPut("/{id:guid}", Update);
        api.MapDelete("/{id:guid}", Delete);
        return api;
    }

    private static async Task<Results<Ok<ServiceDiscoveryResponse>, NotFound>> Get(ServiceDiscoveryService service,
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await service.GetAsync(id, cancellationToken);
        return TypedResults.Ok(item);
    }

    private static async Task<Results<Ok<IEnumerable<ServiceDiscoveryResponse>>, NotFound>> List(ServiceDiscoveryService service,
        string? keywords,
        CancellationToken cancellationToken)
    {
        var items = await service.ListAsync(keywords, cancellationToken);
        return TypedResults.Ok(items);
    }

    private static async Task<Results<Created, ProblemHttpResult>> Create(ServiceDiscoveryService service,
        ServiceDiscoveryRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);

        return result.Match<Results<Created, ProblemHttpResult>>(
            created => TypedResults.Created(string.Empty),
            errors => errors.HandleErrors()
        );
    }

    private static async Task<Results<Ok, ProblemHttpResult>> Update(ServiceDiscoveryService service, Guid id,
        ServiceDiscoveryRequest request, CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);

        return result.Match<Results<Ok, ProblemHttpResult>>(
            updated => TypedResults.Ok(),
            errors => errors.HandleErrors()
        );
    }

    private static async Task<Results<Ok, ProblemHttpResult>> Delete(ServiceDiscoveryService service, Guid id,
        CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);

        return result.Match<Results<Ok, ProblemHttpResult>>(
            deleted => TypedResults.Ok(),
            errors => errors.HandleErrors()
        );
    }
}

