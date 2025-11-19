using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace Yarp.Extensions.Resilience.RateLimiter;

public class RateLimiterMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IRateLimiterPolicyProvider policyProvider)
    {
        var endpoint = context.GetEndpoint();
        
        var enableRateLimitingAttribute = endpoint?.Metadata.GetMetadata<EnableRateLimitingAttribute>();
        
        if (enableRateLimitingAttribute is null)
        {
            await next(context);
            return;
        }

        var policyName = enableRateLimitingAttribute.PolicyName;
        
        var policy = policyProvider.GetPolicy(policyName);

        var limiter = new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions());
        var lease = await limiter.AcquireAsync(1, context.RequestAborted);
 
        if (!lease.IsAcquired)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return;
        }

        await next(context);
    }
}