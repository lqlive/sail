using ErrorOr;
using Sail.Core.Entities;
using Sail.Core.Stores;
using Sail.Route.Errors;
using Sail.Route.Models;

namespace Sail.Route.Validators;

public class RoutePolicyValidator(
    IAuthenticationPolicyStore authPolicyStore,
    IMiddlewareStore middlewareStore)
{
    public async Task<Error?> ValidateAuthorizationPolicyAsync(string? policyId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(policyId))
        {
            return null;
        }

        if (!Guid.TryParse(policyId, out var guid))
        {
            return RouteErrors.AuthorizationPolicyNotFound;
        }

        var policy = await authPolicyStore.GetAsync(guid, cancellationToken);
        if (policy is null)
        {
            return RouteErrors.AuthorizationPolicyNotFound;
        }

        return null;
    }
    public async Task<List<Error>> ValidatePoliciesAsync(RouteRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new List<Error>();

        var authError = await ValidateAuthorizationPolicyAsync(request.AuthorizationPolicy, cancellationToken);
        if (authError is not null)
        {
            errors.Add(authError.Value);
        }

        var rateLimiterError = await ValidateMiddlewarePolicyAsync(
            request.RateLimiterPolicy, 
            MiddlewareType.RateLimiter, 
            RouteErrors.RateLimiterPolicyNotFound, 
            cancellationToken);
        if (rateLimiterError is not null)
        {
            errors.Add(rateLimiterError.Value);
        }

        var corsError = await ValidateMiddlewarePolicyAsync(
            request.CorsPolicy, 
            MiddlewareType.Cors, 
            RouteErrors.CorsPolicyNotFound, 
            cancellationToken);
        if (corsError is not null)
        {
            errors.Add(corsError.Value);
        }

        var timeoutError = await ValidateMiddlewarePolicyAsync(
            request.TimeoutPolicy, 
            MiddlewareType.Timeout, 
            RouteErrors.TimeoutPolicyNotFound, 
            cancellationToken);
        if (timeoutError is not null)
        {
            errors.Add(timeoutError.Value);
        }

        return errors;
    }

    private async Task<Error?> ValidateMiddlewarePolicyAsync(
        string? policyId,
        MiddlewareType expectedType,
        Error notFoundError,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(policyId))
        {
            return null;
        }

        if (!Guid.TryParse(policyId, out var guid))
        {
            return notFoundError;
        }

        var middleware = await middlewareStore.GetAsync(guid, cancellationToken);
        if (middleware is null || middleware.Type != expectedType)
        {
            return notFoundError;
        }

        return null;
    }
}

