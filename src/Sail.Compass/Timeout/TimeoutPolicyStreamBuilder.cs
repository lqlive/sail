using System.Reactive.Linq;
using Sail.Api.V1;
using Sail.Compass.Observers;
using Sail.Core.Timeout;
using ObserverEventType = Sail.Compass.Observers.EventType;

namespace Sail.Compass.Timeout;

internal static class TimeoutPolicyStreamBuilder
{
    public static IObservable<IReadOnlyList<TimeoutPolicyConfig>> BuildTimeoutPolicyStream(
        ResourceObserver<Middleware> middlewareObserver)
    {
        return middlewareObserver
            .GetObservable(watch: true)
            .Scan(
                seed: new Dictionary<string, Middleware>(StringComparer.OrdinalIgnoreCase),
                accumulator: (middlewares, @event) =>
                {
                    var key = @event.Value.MiddlewareId;
                    var newMiddlewares = new Dictionary<string, Middleware>(middlewares, middlewares.Comparer);

                    switch (@event.EventType)
                    {
                        case ObserverEventType.List:
                        case ObserverEventType.Created:
                        case ObserverEventType.Updated:
                            newMiddlewares[key] = @event.Value;
                            break;
                        case ObserverEventType.Deleted:
                            newMiddlewares.Remove(key);
                            break;
                    }

                    return newMiddlewares;
                })
            .Throttle(TimeSpan.FromMilliseconds(100))
            .Select(middlewares => ConvertToTimeoutPolicyConfigs(middlewares.Values))
            .StartWith(Array.Empty<TimeoutPolicyConfig>());
    }

    private static IReadOnlyList<TimeoutPolicyConfig> ConvertToTimeoutPolicyConfigs(
        IEnumerable<Middleware> middlewares)
    {
        var configs = new List<TimeoutPolicyConfig>();

        foreach (var middleware in middlewares)
        {
            if (middleware.Type != MiddlewareType.Timeout
                || !middleware.Enabled
                || middleware.Timeout == null)
            {
                continue;
            }

            configs.Add(new TimeoutPolicyConfig
            {
                Name = middleware.MiddlewareId,
                Seconds = middleware.Timeout.Seconds,
                TimeoutStatusCode = middleware.Timeout.TimeoutStatusCode
            });
        }

        return configs;
    }
}

