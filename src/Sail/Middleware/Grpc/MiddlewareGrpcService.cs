using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Sail.Api.V1;
using Sail.Core.Stores;

namespace Sail.Middleware.Grpc;

public class MiddlewareGrpcService(IMiddlewareStore middlewareStore) : Api.V1.MiddlewareService.MiddlewareServiceBase
{
    public override async Task<ListMiddlewareResponse> List(Empty request, ServerCallContext context)
    {
        var middlewares = await middlewareStore.GetAsync(context.CancellationToken);

        var response = new ListMiddlewareResponse();
        foreach (var middleware in middlewares)
        {
            response.Items.Add(ConvertToProto(middleware));
        }

        return response;
    }

    public override async Task Watch(Empty request, IServerStreamWriter<WatchMiddlewareResponse> responseStream, ServerCallContext context)
    {
        await foreach (var changeEvent in middlewareStore.WatchAsync(context.CancellationToken))
        {
            var eventType = changeEvent.OperationType switch
            {
                ChangeStreamType.Insert => EventType.Create,
                ChangeStreamType.Update => EventType.Update,
                ChangeStreamType.Delete => EventType.Delete,
                _ => EventType.Unknown
            };

            var response = new WatchMiddlewareResponse
            {
                Middleware = ConvertToProto(changeEvent.Document),
                EventType = eventType
            };

            await responseStream.WriteAsync(response);
        }
    }

    private static Api.V1.Middleware ConvertToProto(Core.Entities.Middleware middleware)
    {
        var proto = new Api.V1.Middleware
        {
            MiddlewareId = middleware.Id.ToString(),
            Name = middleware.Name,
            Type = middleware.Type switch
            {
                Core.Entities.MiddlewareType.Cors => Api.V1.MiddlewareType.Cors,
                Core.Entities.MiddlewareType.RateLimiter => Api.V1.MiddlewareType.RateLimiter,
                Core.Entities.MiddlewareType.Timeout => Api.V1.MiddlewareType.Timeout,
                Core.Entities.MiddlewareType.Retry => Api.V1.MiddlewareType.Retry,
                _ => Api.V1.MiddlewareType.Cors
            },
            Enabled = middleware.Enabled
        };

        if (!string.IsNullOrEmpty(middleware.Description))
        {
            proto.Description = middleware.Description;
        }

        if (middleware.Cors != null)
        {
            proto.Cors = new Api.V1.Cors
            {
                Name = middleware.Cors.Name,
                AllowCredentials = middleware.Cors.AllowCredentials
            };

            if (middleware.Cors.MaxAge.HasValue)
            {
                proto.Cors.MaxAge = middleware.Cors.MaxAge.Value;
            }

            if (middleware.Cors.AllowOrigins != null)
            {
                proto.Cors.AllowOrigins.AddRange(middleware.Cors.AllowOrigins);
            }

            if (middleware.Cors.AllowMethods != null)
            {
                proto.Cors.AllowMethods.AddRange(middleware.Cors.AllowMethods);
            }

            if (middleware.Cors.AllowHeaders != null)
            {
                proto.Cors.AllowHeaders.AddRange(middleware.Cors.AllowHeaders);
            }

            if (middleware.Cors.ExposeHeaders != null)
            {
                proto.Cors.ExposeHeaders.AddRange(middleware.Cors.ExposeHeaders);
            }
        }

        if (middleware.RateLimiter != null)
        {
            proto.RateLimiter = new Api.V1.RateLimiter
            {
                Name = middleware.RateLimiter.Name,
                PermitLimit = middleware.RateLimiter.PermitLimit,
                Window = middleware.RateLimiter.Window,
                QueueLimit = middleware.RateLimiter.QueueLimit
            };
        }

        if (middleware.Timeout != null)
        {
            proto.Timeout = new Api.V1.Timeout
            {
                Name = middleware.Timeout.Name,
                Seconds = middleware.Timeout.Seconds
            };

            if (middleware.Timeout.TimeoutStatusCode.HasValue)
            {
                proto.Timeout.TimeoutStatusCode = middleware.Timeout.TimeoutStatusCode.Value;
            }
        }

        if (middleware.Retry != null)
        {
            proto.Retry = new Api.V1.Retry
            {
                Name = middleware.Retry.Name,
                MaxRetryAttempts = middleware.Retry.MaxRetryAttempts,
                RetryDelayMilliseconds = middleware.Retry.RetryDelayMilliseconds,
                UseExponentialBackoff = middleware.Retry.UseExponentialBackoff
            };

            if (middleware.Retry.RetryStatusCodes != null)
            {
                proto.Retry.RetryStatusCodes.AddRange(middleware.Retry.RetryStatusCodes);
            }
        }

        return proto;
    }
}


