using Grpc.Core;
using Sail.Api.V1;
using System.Reactive.Linq;
using Google.Protobuf.WellKnownTypes;
using Sail.Compass.Extensions;

namespace Sail.Compass.Observers;

internal sealed class CertificateObserver(CertificateService.CertificateServiceClient client) : ResourceObserver<Certificate>
{
    public override IObservable<ResourceEvent<Certificate>> GetObservable(bool watch)
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

    private IObservable<ResourceEvent<Certificate>> List()
    {
        return Observable.Create<ResourceEvent<Certificate>>((observer, cancellationToken) =>
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

    private IObservable<ResourceEvent<Certificate>> Watch()
    {
        return Observable.Create<ResourceEvent<Certificate>>(async (observer, cancellationToken) =>
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

                var resource = current.Certificate.ToResourceEvent(eventType);
                observer.OnNext(resource!);
            }
        });
    }
}

