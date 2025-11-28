using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Polly;
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
            Log.RetryPolicyNotFound(_logger, policyName!);
            return _next(context);
        }

        return InvokeInternalAsync(context, reverseProxyFeature, policy);
    }

    private async Task InvokeInternalAsync(HttpContext context, IReverseProxyFeature reverseProxyFeature, RetryPipelineWrapper policy)
    {
        context.Request.EnableBuffering();

        var availableDestinations = reverseProxyFeature.AvailableDestinations;
        var resilienceContext = ResilienceContextPool.Shared.Get(context.RequestAborted);

        resilienceContext.Properties.Set(RetryKeys.OnRetryCallback, () =>
        {
            PrepareRetryAttempt(context, reverseProxyFeature, availableDestinations);
        });

        try
        {
            await policy.Pipeline.ExecuteAsync(async ctx =>
            {
                await _next(context);
                ThrowIfRetryableStatusCode(context.Response.StatusCode, policy.Config);
            }, resilienceContext);
        }
        finally
        {
            ResilienceContextPool.Shared.Return(resilienceContext);
        }
    }

    private void PrepareRetryAttempt(
        HttpContext context,
        IReverseProxyFeature reverseProxyFeature,
        IReadOnlyList<DestinationState> availableDestinations)
    {
        var healthyDestinations = GetHealthyDestinations(reverseProxyFeature, availableDestinations);

        if (healthyDestinations.Count == 0)
        {
            Log.NoHealthyDestinations(_logger, reverseProxyFeature.Route.Config.RouteId);
            return;
        }

        reverseProxyFeature.AvailableDestinations = healthyDestinations;
        reverseProxyFeature.ProxiedDestination = null;
        context.Request.Body.Position = 0;
    }

    private static List<DestinationState> GetHealthyDestinations(
        IReverseProxyFeature reverseProxyFeature,
        IReadOnlyList<DestinationState> availableDestinations)
    {
        return availableDestinations
            .Where(destination => destination != reverseProxyFeature.ProxiedDestination)
            .ToList();
    }

    private static void ThrowIfRetryableStatusCode(int statusCode, RetryPolicyConfig config)
    {
        if (config.RetryStatusCodes.Contains(statusCode))
        {
            throw new RetryableHttpException(statusCode);
        }
    }

    private static class Log
    {
        private static readonly Action<ILogger, string, Exception?> _retryPolicyNotFound = LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(1, nameof(RetryPolicyNotFound)),
            "Retry policy not found: {PolicyName}");

        private static readonly Action<ILogger, string, Exception?> _noHealthyDestinations = LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(2, nameof(NoHealthyDestinations)),
            "No healthy destinations available for retry. Route: {RouteId}");

        public static void RetryPolicyNotFound(ILogger logger, string policyName)
        {
            _retryPolicyNotFound(logger, policyName, null);
        }

        public static void NoHealthyDestinations(ILogger logger, string routeId)
        {
            _noHealthyDestinations(logger, routeId, null);
        }
    }
}
