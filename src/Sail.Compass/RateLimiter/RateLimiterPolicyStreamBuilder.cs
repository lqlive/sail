using System.Reactive.Linq;
using Sail.Api.V1;
using Sail.Compass.Observers;
using Sail.Core.RateLimiter;
using ObserverEventType = Sail.Compass.Observers.EventType;

namespace Sail.Compass.RateLimiter;

internal static class RateLimiterPolicyStreamBuilder
{
    public static IObservable<IReadOnlyList<RateLimiterPolicyConfig>> BuildRateLimiterPolicyStream(
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
            .Select(middlewares => ConvertToRateLimiterPolicyConfigs(middlewares.Values))
            .StartWith(Array.Empty<RateLimiterPolicyConfig>());
    }

    private static IReadOnlyList<RateLimiterPolicyConfig> ConvertToRateLimiterPolicyConfigs(
        IEnumerable<Middleware> middlewares)
    {
        var configs = new List<RateLimiterPolicyConfig>();

        foreach (var middleware in middlewares)
        {
            if (middleware.Type != MiddlewareType.RateLimiter ||
                !middleware.Enabled ||
                middleware.RateLimiter == null)
            {
                continue;
            }

            configs.Add(new RateLimiterPolicyConfig
            {
                Name = middleware.MiddlewareId,
                PermitLimit = middleware.RateLimiter.PermitLimit,
                Window = middleware.RateLimiter.Window,
                QueueLimit = middleware.RateLimiter.QueueLimit
            });
        }

        return configs;
    }
}

