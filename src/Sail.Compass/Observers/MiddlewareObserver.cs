using Grpc.Core;
using Sail.Api.V1;
using System.Reactive.Linq;
using Google.Protobuf.WellKnownTypes;
using Sail.Compass.Extensions;

namespace Sail.Compass.Observers;

internal sealed class MiddlewareObserver(MiddlewareService.MiddlewareServiceClient client) : ResourceObserver<Middleware>
{
    public override IObservable<ResourceEvent<Middleware>> GetObservable(bool watch)
    {
        if (!watch)
        {
            return List();
        }

        return Observable.Concat(
            List(),
            Watch()
        );
    }

    private IObservable<ResourceEvent<Middleware>> List()
    {
        return Observable.Create<ResourceEvent<Middleware>>((observer, cancellationToken) =>
        {
            var list = client.List(new Empty(), cancellationToken: cancellationToken);
            foreach (var item in list.Items)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                var resource = item.ToResourceEvent(EventType.List);
                observer.OnNext(resource!);
            }

            observer.OnCompleted();
            return Task.CompletedTask;
        });
    }

    private IObservable<ResourceEvent<Middleware>> Watch()
    {
        return Observable.Create<ResourceEvent<Middleware>>(async (observer, cancellationToken) =>
        {
            var result = client.Watch(new Empty(), cancellationToken: cancellationToken);
            var watch = result.ResponseStream;
            await foreach (var current in watch.ReadAllAsync(cancellationToken: cancellationToken))
            {
                var eventType = current.EventType switch
                {
                    Api.V1.EventType.Create => EventType.Created,
                    Api.V1.EventType.Update => EventType.Updated,
                    Api.V1.EventType.Delete => EventType.Deleted,
                    _ => EventType.Unknown,
                };

                var resource = current.Middleware.ToResourceEvent(eventType);
                observer.OnNext(resource!);
            }
        });
    }
}

