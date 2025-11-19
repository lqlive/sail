using System.Collections.Concurrent;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Yarp.ReverseProxy.Model;

namespace Sail.Core.RateLimiter;

public class RateLimiterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRateLimiterPolicyProvider _policyProvider;
    private readonly ILogger<RateLimiterMiddleware> _logger;
    private readonly ConcurrentDictionary<string, SlidingWindowRateLimiter> _limiters = new();
    private readonly int _rejectionStatusCode = StatusCodes.Status429TooManyRequests;

    public RateLimiterMiddleware(
        RequestDelegate next,
        IRateLimiterPolicyProvider policyProvider,
        ILogger<RateLimiterMiddleware> logger)
    {
        _next = next;
        _policyProvider = policyProvider;
        _logger = logger;
    }

    public Task Invoke(HttpContext context)
    {
        _logger.LogInformation("RateLimiterMiddleware invoked for path: {Path}", context.Request.Path);
        
        var endpoint = context.GetEndpoint();
        _logger.LogInformation("Endpoint: {Endpoint}", endpoint?.DisplayName ?? "null");

        if (endpoint?.Metadata.GetMetadata<DisableRateLimitingAttribute>() is not null)
        {
            _logger.LogInformation("Rate limiting disabled for this endpoint");
            return _next(context);
        }

        var reverseProxyFeature = context.Features.Get<IReverseProxyFeature>();
        _logger.LogInformation("IReverseProxyFeature found: {Found}, Route: {Route}", 
            reverseProxyFeature != null, 
            reverseProxyFeature?.Route?.Config?.RouteId ?? "null");

        if (reverseProxyFeature?.Route.Config.Metadata?.ContainsKey("RateLimiterPolicy") == true)
        {
            _logger.LogInformation("RateLimiterPolicy found in metadata");
            return InvokeInternal(context);
        }

        _logger.LogInformation("No rate limiter policy found, skipping");
        return _next(context);
    }

    private async Task InvokeInternal(HttpContext context)
    {
        var reverseProxyFeature = context.Features.Get<IReverseProxyFeature>();
        if (reverseProxyFeature?.Route.Config.Metadata?.TryGetValue("RateLimiterPolicy", out var policyName) != true 
            || string.IsNullOrEmpty(policyName))
        {
            await _next(context);
            return;
        }

        _logger.LogDebug("Using rate limiter policy from route metadata: {PolicyName}", policyName);
        await ApplyRateLimitAsync(context, policyName);
    }

    private async Task ApplyRateLimitAsync(HttpContext context, string policyName)
    {
        var policy = _policyProvider.GetPolicy(policyName);

        if (policy is null)
        {
            _logger.LogWarning("Rate limiting policy not found: {PolicyName}", policyName);
            await _next(context);
            return;
        }

        var limiter = GetOrCreateLimiter(policy);

        RateLimitLease? lease = null;
        try
        {
            lease = await limiter.AcquireAsync(permitCount: 1, context.RequestAborted);

            if (lease.IsAcquired)
            {
                _logger.LogDebug("Rate limit lease acquired for policy: {PolicyName}", policyName);
                await _next(context);
            }
            else
            {
                _logger.LogWarning("Rate limit exceeded for policy: {PolicyName}", policyName);
                context.Response.StatusCode = _rejectionStatusCode;
                
                if (lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString("F0");
                }

                await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            }
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogDebug("Request canceled while acquiring rate limit lease");
        }
        finally
        {
            lease?.Dispose();
        }
    }

    private SlidingWindowRateLimiter GetOrCreateLimiter(RateLimiterPolicyConfig policy)
    {
        return _limiters.GetOrAdd(policy.Name, _ =>
        {
            var options = new SlidingWindowRateLimiterOptions
            {
                PermitLimit = policy.PermitLimit,
                Window = TimeSpan.FromSeconds(policy.Window),
                SegmentsPerWindow = 10,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = policy.QueueLimit
            };

            _logger.LogInformation(
                "Created rate limiter for policy: {PolicyName}, PermitLimit: {PermitLimit}, Window: {Window}s, QueueLimit: {QueueLimit}",
                policy.Name, policy.PermitLimit, policy.Window, policy.QueueLimit);

            return new SlidingWindowRateLimiter(options);
        });
    }
}