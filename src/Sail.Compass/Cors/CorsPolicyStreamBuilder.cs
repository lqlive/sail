using System.Reactive.Linq;
using Sail.Api.V1;
using Sail.Compass.Observers;
using Sail.Core.Cors;
using ObserverEventType = Sail.Compass.Observers.EventType;

namespace Sail.Compass.Cors;

internal static class CorsPolicyStreamBuilder
{
    public static IObservable<IReadOnlyList<CorsPolicyConfig>> BuildCorsPolicyStream(
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
            .Select(middlewares => ConvertToCorsPolicyConfigs(middlewares.Values))
            .StartWith(Array.Empty<CorsPolicyConfig>());
    }

    private static IReadOnlyList<CorsPolicyConfig> ConvertToCorsPolicyConfigs(
        IEnumerable<Middleware> middlewares)
    {
        var configs = new List<CorsPolicyConfig>();

        foreach (var middleware in middlewares)
        {
            if (middleware.Type != MiddlewareType.Cors ||
                !middleware.Enabled ||
                middleware.Cors == null)
            {
                continue;
            }

            configs.Add(new CorsPolicyConfig
            {
                Name = middleware.MiddlewareId,
                AllowOrigins = middleware.Cors.AllowOrigins.ToList(),
                AllowMethods = middleware.Cors.AllowMethods.ToList(),
                AllowHeaders = middleware.Cors.AllowHeaders.ToList(),
                ExposeHeaders = middleware.Cors.ExposeHeaders.ToList(),
                AllowCredentials = middleware.Cors.AllowCredentials,
                MaxAge = middleware.Cors.MaxAge
            });
        }

        return configs;
    }
}

