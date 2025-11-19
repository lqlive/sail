using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Sail.Models.AuthenticationPolicies;
using Sail.Services;

namespace Sail.Apis;

public static class AuthenticationPolicyApi
{
    public static RouteGroupBuilder MapAuthenticationPolicyApi(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/authentication-policies")
            .HasApiVersion(1.0)
            .WithTags("Authentication Policies");

        group.MapGet("/", GetAuthenticationPolicies);
        group.MapGet("/{id:guid}", GetAuthenticationPolicy);
        group.MapPost("/", CreateAuthenticationPolicy);
        group.MapPut("/{id:guid}", UpdateAuthenticationPolicy);
        group.MapDelete("/{id:guid}", DeleteAuthenticationPolicy);

        return group;
    }

    private static async Task<IResult> GetAuthenticationPolicies(
        [FromServices] AuthenticationPolicyService service,
        CancellationToken cancellationToken)
    {
        var policies = await service.GetAsync(cancellationToken);
        return Results.Ok(policies);
    }

    private static async Task<IResult> GetAuthenticationPolicy(
        [FromRoute] Guid id,
        [FromServices] AuthenticationPolicyService service,
        CancellationToken cancellationToken)
    {
        var policy = await service.GetByIdAsync(id, cancellationToken);
        return policy == null ? Results.NotFound() : Results.Ok(policy);
    }

    private static async Task<IResult> CreateAuthenticationPolicy(
        [FromBody] AuthenticationPolicyRequest request,
        [FromServices] AuthenticationPolicyService service,
        CancellationToken cancellationToken)
    {
        var policy = await service.CreateAsync(request, cancellationToken);
        return Results.Created($"/api/authentication-policies/{policy.Id}", policy);
    }

    private static async Task<IResult> UpdateAuthenticationPolicy(
        [FromRoute] Guid id,
        [FromBody] AuthenticationPolicyRequest request,
        [FromServices] AuthenticationPolicyService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var policy = await service.UpdateAsync(id, request, cancellationToken);
            return Results.Ok(policy);
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> DeleteAuthenticationPolicy(
        [FromRoute] Guid id,
        [FromServices] AuthenticationPolicyService service,
        CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Results.NoContent();
    }
}

