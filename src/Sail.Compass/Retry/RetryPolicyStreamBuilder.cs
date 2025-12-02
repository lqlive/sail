using System.Reactive.Linq;
using Sail.Api.V1;
using Sail.Compass.Observers;
using Sail.Core.Retry;
using ObserverEventType = Sail.Compass.Observers.EventType;

namespace Sail.Compass.Retry;

internal static class RetryPolicyStreamBuilder
{
    public static IObservable<IReadOnlyList<RetryPolicyConfig>> BuildRetryPolicyStream(
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
            .Select(middlewares => ConvertToRetryPolicyConfigs(middlewares.Values))
            .StartWith(Array.Empty<RetryPolicyConfig>());
    }

    private static IReadOnlyList<RetryPolicyConfig> ConvertToRetryPolicyConfigs(
        IEnumerable<Middleware> middlewares)
    {
        var configs = new List<RetryPolicyConfig>();

        foreach (var middleware in middlewares)
        {
            if (middleware.Type != MiddlewareType.Retry
                || !middleware.Enabled
                || middleware.Retry == null)
            {
                continue;
            }

            configs.Add(new RetryPolicyConfig
            {
                Name = middleware.MiddlewareId,
                MaxRetryAttempts = middleware.Retry.MaxRetryAttempts,
                RetryStatusCodes = middleware.Retry.RetryStatusCodes.ToArray(),
                RetryDelayMilliseconds = middleware.Retry.RetryDelayMilliseconds,
                UseExponentialBackoff = middleware.Retry.UseExponentialBackoff
            });
        }

        return configs;
    }
}

