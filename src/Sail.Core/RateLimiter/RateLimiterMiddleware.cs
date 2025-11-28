using System.Collections.Concurrent;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Sail.Core.Utilities;
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
        Log.MiddlewareInvoked(_logger, context.Request.Path.Value);

        var endpoint = context.GetEndpoint();
        Log.EndpointDetected(_logger, endpoint?.DisplayName);

        if (endpoint?.Metadata.GetMetadata<DisableRateLimitingAttribute>() is not null)
        {
            Log.RateLimitingDisabled(_logger);
            return _next(context);
        }

        var reverseProxyFeature = context.Features.Get<IReverseProxyFeature>();
        Log.ReverseProxyFeatureDetected(_logger, reverseProxyFeature != null, reverseProxyFeature?.Route?.Config?.RouteId);

        if (reverseProxyFeature?.Route.Config.Metadata?.ContainsKey("RateLimiterPolicy") == true)
        {
            Log.PolicyFoundInMetadata(_logger);
            return InvokeInternal(context);
        }

        Log.NoPolicyFound(_logger);
        return _next(context);
    }

    private async Task InvokeInternal(HttpContext context)
    {
        var reverseProxyFeature = context.Features.Get<IReverseProxyFeature>();
        var policyName = reverseProxyFeature?.Route.GetMetadata<string>("RateLimiterPolicy");

        if (string.IsNullOrEmpty(policyName))
        {
            await _next(context);
            return;
        }

        Log.UsingPolicy(_logger, policyName);
        await ApplyRateLimitAsync(context, policyName);
    }

    private async Task ApplyRateLimitAsync(HttpContext context, string policyName)
    {
        var policy = _policyProvider.GetPolicy(policyName);

        if (policy is null)
        {
            Log.PolicyNotFound(_logger, policyName);
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
                Log.LeaseAcquired(_logger, policyName);
                await _next(context);
            }
            else
            {
                Log.RateLimitExceeded(_logger, policyName);
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
            Log.RequestCanceled(_logger);
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

            Log.CreatedLimiter(_logger, policy.Name, policy.PermitLimit, policy.Window, policy.QueueLimit);

            return new SlidingWindowRateLimiter(options);
        });
    }

    private static class Log
    {
        private static readonly Action<ILogger, string?, Exception?> _middlewareInvoked = LoggerMessage.Define<string?>(
            LogLevel.Information,
            new EventId(1, nameof(MiddlewareInvoked)),
            "RateLimiterMiddleware invoked for path: {Path}");

        private static readonly Action<ILogger, string?, Exception?> _endpointDetected = LoggerMessage.Define<string?>(
            LogLevel.Information,
            new EventId(2, nameof(EndpointDetected)),
            "Endpoint: {Endpoint}");

        private static readonly Action<ILogger, Exception?> _rateLimitingDisabled = LoggerMessage.Define(
            LogLevel.Information,
            new EventId(3, nameof(RateLimitingDisabled)),
            "Rate limiting disabled for this endpoint");

        private static readonly Action<ILogger, bool, string?, Exception?> _reverseProxyFeatureDetected = LoggerMessage.Define<bool, string?>(
            LogLevel.Information,
            new EventId(4, nameof(ReverseProxyFeatureDetected)),
            "IReverseProxyFeature found: {Found}, Route: {Route}");

        private static readonly Action<ILogger, Exception?> _policyFoundInMetadata = LoggerMessage.Define(
            LogLevel.Information,
            new EventId(5, nameof(PolicyFoundInMetadata)),
            "RateLimiterPolicy found in metadata");

        private static readonly Action<ILogger, Exception?> _noPolicyFound = LoggerMessage.Define(
            LogLevel.Information,
            new EventId(6, nameof(NoPolicyFound)),
            "No rate limiter policy found, skipping");

        private static readonly Action<ILogger, string, Exception?> _usingPolicy = LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(7, nameof(UsingPolicy)),
            "Using rate limiter policy from route metadata: {PolicyName}");

        private static readonly Action<ILogger, string, Exception?> _policyNotFound = LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(8, nameof(PolicyNotFound)),
            "Rate limiting policy not found: {PolicyName}");

        private static readonly Action<ILogger, string, Exception?> _leaseAcquired = LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(9, nameof(LeaseAcquired)),
            "Rate limit lease acquired for policy: {PolicyName}");

        private static readonly Action<ILogger, string, Exception?> _rateLimitExceeded = LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(10, nameof(RateLimitExceeded)),
            "Rate limit exceeded for policy: {PolicyName}");

        private static readonly Action<ILogger, Exception?> _requestCanceled = LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(11, nameof(RequestCanceled)),
            "Request canceled while acquiring rate limit lease");

        private static readonly Action<ILogger, string, int, int, int, Exception?> _createdLimiter = LoggerMessage.Define<string, int, int, int>(
            LogLevel.Information,
            new EventId(12, nameof(CreatedLimiter)),
            "Created rate limiter for policy: {PolicyName}, PermitLimit: {PermitLimit}, Window: {Window}s, QueueLimit: {QueueLimit}");

        public static void MiddlewareInvoked(ILogger logger, string? path)
        {
            _middlewareInvoked(logger, path, null);
        }

        public static void EndpointDetected(ILogger logger, string? endpoint)
        {
            _endpointDetected(logger, endpoint ?? "null", null);
        }

        public static void RateLimitingDisabled(ILogger logger)
        {
            _rateLimitingDisabled(logger, null);
        }

        public static void ReverseProxyFeatureDetected(ILogger logger, bool found, string? route)
        {
            _reverseProxyFeatureDetected(logger, found, route ?? "null", null);
        }

        public static void PolicyFoundInMetadata(ILogger logger)
        {
            _policyFoundInMetadata(logger, null);
        }

        public static void NoPolicyFound(ILogger logger)
        {
            _noPolicyFound(logger, null);
        }

        public static void UsingPolicy(ILogger logger, string policyName)
        {
            _usingPolicy(logger, policyName, null);
        }

        public static void PolicyNotFound(ILogger logger, string policyName)
        {
            _policyNotFound(logger, policyName, null);
        }

        public static void LeaseAcquired(ILogger logger, string policyName)
        {
            _leaseAcquired(logger, policyName, null);
        }

        public static void RateLimitExceeded(ILogger logger, string policyName)
        {
            _rateLimitExceeded(logger, policyName, null);
        }

        public static void RequestCanceled(ILogger logger)
        {
            _requestCanceled(logger, null);
        }

        public static void CreatedLimiter(ILogger logger, string policyName, int permitLimit, int window, int queueLimit)
        {
            _createdLimiter(logger, policyName, permitLimit, window, queueLimit, null);
        }
    }
}
