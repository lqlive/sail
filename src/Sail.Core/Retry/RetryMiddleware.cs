using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sail.Core.Utilities;
using Yarp.ReverseProxy.Model;

namespace Sail.Core.Retry;

public class RetryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRetryPolicyProvider _policyProvider;
    private readonly ILogger<RetryMiddleware> _logger;

    public RetryMiddleware(
        RequestDelegate next,
        IRetryPolicyProvider policyProvider,
        ILogger<RetryMiddleware> logger)
    {
        _next = next;
        _policyProvider = policyProvider;
        _logger = logger;
    }

    public Task InvokeAsync(HttpContext context)
    {
        var reverseProxyFeature = context.Features.Get<IReverseProxyFeature>();

        if (reverseProxyFeature?.Route.Config.Metadata?.ContainsKey("RetryPolicy") != true)
        {
            return _next(context);
        }

        var policyName = reverseProxyFeature.Route.GetMetadata<string>("RetryPolicy");

        var policy = _policyProvider.GetPolicy(policyName!);

        if (policy is null)
        {
            _logger.LogWarning("Retry policy not found: {PolicyName}", policyName);
            return _next(context);
        }

        return InvokeInternalAsync(context, reverseProxyFeature, policy);
    }

    private async Task InvokeInternalAsync(HttpContext context, IReverseProxyFeature reverseProxyFeature, RetryPolicyConfig policy)
    {
        context.Request.EnableBuffering();

        var availableDestinations = reverseProxyFeature.AvailableDestinations;
        var retryCount = 0;

        await _next(context);

        var statusCode = context.Response.StatusCode;

        while (policy.RetryStatusCodes.Contains(statusCode) && retryCount < policy.MaxRetryAttempts)
        {
            var healthyDestinations = availableDestinations
                .Where(m => m != reverseProxyFeature.ProxiedDestination)
                .ToList();

            if (healthyDestinations.Count == 0)
            {
                _logger.LogWarning("No healthy destinations available for retry. Route: {RouteId}", reverseProxyFeature.Route.Config.RouteId);
                return;
            }

            retryCount++;

            var delay = CalculateRetryDelay(policy, retryCount);
            _logger.LogInformation("Retrying request (attempt {RetryCount}/{MaxRetryAttempts}) after {Delay}ms. Route: {RouteId}, StatusCode: {StatusCode}",
                retryCount, policy.MaxRetryAttempts, delay, reverseProxyFeature.Route.Config.RouteId, statusCode);

            await Task.Delay(delay, context.RequestAborted);

            reverseProxyFeature.AvailableDestinations = healthyDestinations;
            reverseProxyFeature.ProxiedDestination = null;
            context.Request.Body.Position = 0;

            await _next(context);
            statusCode = context.Response.StatusCode;
        }

        if (retryCount > 0)
        {
            _logger.LogInformation("Retry completed after {RetryCount} attempts. Final StatusCode: {StatusCode}", retryCount, statusCode);
        }
    }

    private static int CalculateRetryDelay(RetryPolicyConfig policy, int retryAttempt)
    {
        if (policy.UseExponentialBackoff)
        {
            return policy.RetryDelayMilliseconds * (int)Math.Pow(2, retryAttempt - 1);
        }

        return policy.RetryDelayMilliseconds * retryAttempt;
    }
}

