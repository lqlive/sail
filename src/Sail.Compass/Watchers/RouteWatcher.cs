using Grpc.Core;
using Sail.Api.V1;
using System.Reactive.Linq;
using Google.Protobuf.WellKnownTypes;
using Sail.Compass.Extensions;

namespace Sail.Compass.Watchers;

internal sealed class RouteWatcher(RouteService.RouteServiceClient client) : ResourceWatcher<Route>
{
    public override IObservable<ResourceEvent<Route>> GetObservable(bool watch)
    {
        var result = watch ? Watch() : List();
        return result;
    }

    private IObservable<ResourceEvent<Route>> List()
    {
        return Observable.Create<ResourceEvent<Route>>((observer, cancellationToken) =>
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

    private IObservable<ResourceEvent<Route>> Watch()
    {
        return Observable.Create<ResourceEvent<Route>>(async (observer, cancellationToken) =>
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

                var resource = current.Route.ToResourceEvent(eventType);
                observer.OnNext(resource!);
            }
        });
    }
}