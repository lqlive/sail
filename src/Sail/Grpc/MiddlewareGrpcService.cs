using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MongoDB.Driver;
using Sail.Api.V1;
using Sail.Core.Entities;
using Sail.Core.Stores;
using Sail.Storage.MongoDB;
using Sail.Storage.MongoDB.Extensions;

namespace Sail.Grpc;

public class MiddlewareGrpcService(SailContext dbContext, IMiddlewareStore middlewareStore) : Api.V1.MiddlewareService.MiddlewareServiceBase
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
        var options = new ChangeStreamOptions
        {
            FullDocument = ChangeStreamFullDocumentOption.Required,
            FullDocumentBeforeChange = ChangeStreamFullDocumentBeforeChangeOption.Required
        };

        while (!context.CancellationToken.IsCancellationRequested)
        {
            var watch = await dbContext.Middlewares.WatchAsync(options, context.CancellationToken);

            await foreach (var changeStreamDocument in watch.ToAsyncEnumerable())
            {
                var document = changeStreamDocument.FullDocument;
                if (changeStreamDocument.OperationType == ChangeStreamOperationType.Delete)
                {
                    document = changeStreamDocument.FullDocumentBeforeChange;
                }

                var eventType = changeStreamDocument.OperationType switch
                {
                    ChangeStreamOperationType.Insert => EventType.Create,
                    ChangeStreamOperationType.Update => EventType.Update,
                    ChangeStreamOperationType.Delete => EventType.Delete,
                    _ => EventType.Unknown
                };
                
                var response = new WatchMiddlewareResponse
                {
                    Middleware = ConvertToProto(document),
                    EventType = eventType
                };
                
                await responseStream.WriteAsync(response);
            }
        }
    }

    private static Api.V1.Middleware ConvertToProto(Core.Entities.Middleware middleware)
    {
        var proto = new Api.V1.Middleware
        {
            MiddlewareId = middleware.Id.ToString(),
            Name = middleware.Name,
            Type = middleware.Type == Core.Entities.MiddlewareType.Cors 
                ? Api.V1.MiddlewareType.Cors 
                : middleware.Type == Core.Entities.MiddlewareType.Timeout
                    ? Api.V1.MiddlewareType.Timeout
                    : Api.V1.MiddlewareType.RateLimiter,
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

        return proto;
    }
}

