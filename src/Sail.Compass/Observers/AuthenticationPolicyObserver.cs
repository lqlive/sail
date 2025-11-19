using System.Reactive.Linq;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Sail.Api.V1;
using Sail.Compass.Extensions;

namespace Sail.Compass.Observers;

public sealed class AuthenticationPolicyObserver(
    AuthenticationPolicyService.AuthenticationPolicyServiceClient client) 
    : ResourceObserver<AuthenticationPolicy>
{
    public override IObservable<ResourceEvent<AuthenticationPolicy>> GetObservable(bool watch)
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

    private IObservable<ResourceEvent<AuthenticationPolicy>> List()
    {
        return Observable.Create<ResourceEvent<AuthenticationPolicy>>((observer, cancellationToken) =>
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

    private IObservable<ResourceEvent<AuthenticationPolicy>> Watch()
    {
        return Observable.Create<ResourceEvent<AuthenticationPolicy>>(async (observer, cancellationToken) =>
        {
            var result = client.Watch(new Empty(), cancellationToken: cancellationToken);
            var watchStream = result.ResponseStream;
            await foreach (var current in watchStream.ReadAllAsync(cancellationToken: cancellationToken))
            {
                var eventType = current.EventType switch
                {
                    Api.V1.EventType.Create => EventType.Created,
                    Api.V1.EventType.Update => EventType.Updated,
                    Api.V1.EventType.Delete => EventType.Deleted,
                    _ => EventType.Unknown,
                };

                var resource = current.Policy.ToResourceEvent(eventType);
                observer.OnNext(resource!);
            }
        });
    }
}

