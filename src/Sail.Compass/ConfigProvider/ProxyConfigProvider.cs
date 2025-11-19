using System.Globalization;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.Primitives;
using Sail.Api.V1;
using Sail.Compass.Observers;
using Yarp.ReverseProxy.Configuration;
using ObserverEventType = Sail.Compass.Observers.EventType;
using YarpRouteMatch = Yarp.ReverseProxy.Configuration.RouteMatch;

namespace Sail.Compass.ConfigProvider;

internal sealed class ProxyConfigProvider : IProxyConfigProvider, IDisposable
{
    private readonly Lock _lockObject = new();
    private readonly CompositeDisposable _subscriptions = new();
    private ProxyConfigSnapshot? _snapshot;
    private CancellationTokenSource? _changeToken;
    private bool _disposed;

    public ProxyConfigProvider(
        ResourceObserver<Route> routeObserver,
        ResourceObserver<Cluster> clusterObserver)
    {
        var routes = CreateRouteStream(routeObserver);
        var clusters = CreateClusterStream(clusterObserver);

        var subscription = routes
            .CombineLatest(clusters, (r, c) => (Routes: r, Clusters: c))
            .Subscribe(x => UpdateSnapshot(x.Routes, x.Clusters));
        
        _subscriptions.Add(subscription);
    }

    public IProxyConfig GetConfig()
    {
        lock (_lockObject)
        {
            return _snapshot ?? new ProxyConfigSnapshot();
        }
    }

    private IObservable<Dictionary<string, Route>> CreateRouteStream(
        ResourceObserver<Route> observer)
    {
        return observer
            .GetObservable(watch: true)
            .Scan(
                seed: new Dictionary<string, Route>(StringComparer.OrdinalIgnoreCase),
                accumulator: (keys, @event) =>
                {
                    var key = @event.Value.RouteId;
                    var newKeys = new Dictionary<string, Route>(keys, keys.Comparer);
                    
                    switch (@event.EventType)
                    {
                        case ObserverEventType.List:
                        case ObserverEventType.Created:
                        case ObserverEventType.Updated:
                            newKeys[key] = @event.Value;
                            break;
                        case ObserverEventType.Deleted:
                            newKeys.Remove(key);
                            break;
                    }
                    
                    return newKeys;
                })
            .Throttle(TimeSpan.FromMilliseconds(100))
            .StartWith(new Dictionary<string, Route>(StringComparer.OrdinalIgnoreCase));
    }

    private IObservable<Dictionary<string, Cluster>> CreateClusterStream(
        ResourceObserver<Cluster> observer)
    {
        return observer
            .GetObservable(watch: true)
            .Scan(
                seed: new Dictionary<string, Cluster>(StringComparer.OrdinalIgnoreCase),
                accumulator: (keys, @event) =>
                {
                    var key = @event.Value.ClusterId;
                    var newKeys = new Dictionary<string, Cluster>(keys, keys.Comparer);
                    
                    switch (@event.EventType)
                    {
                        case ObserverEventType.List:
                        case ObserverEventType.Created:
                        case ObserverEventType.Updated:
                            newKeys[key] = @event.Value;
                            break;
                        case ObserverEventType.Deleted:
                            newKeys.Remove(key);
                            break;
                    }
                    
                    return newKeys;
                })
            .Throttle(TimeSpan.FromMilliseconds(100))
            .StartWith(new Dictionary<string, Cluster>(StringComparer.OrdinalIgnoreCase));
    }

    private void UpdateSnapshot(
        Dictionary<string, Route> routes,
        Dictionary<string, Cluster> clusters)
    {
        lock (_lockObject)
        {
            var newSnapshot = new ProxyConfigSnapshot();
            
            foreach (var cluster in clusters.Values)
            {
                newSnapshot.Clusters.Add(ConvertCluster(cluster));
            }
            
            foreach (var route in routes.Values)
            {
                newSnapshot.Routes.Add(ConvertRoute(route));
            }

            var oldToken = _changeToken;
            _changeToken = new CancellationTokenSource();
            newSnapshot.ChangeToken = new CancellationChangeToken(_changeToken.Token);
            
            _snapshot = newSnapshot;

            try
            {
                oldToken?.Cancel(throwOnFirstException: false);
            }
            catch
            {
            }
        }
    }

    private static ClusterConfig ConvertCluster(Cluster cluster)
    {
        var clusterConfig = new ClusterConfig
        {
            ClusterId = cluster.ClusterId,
            LoadBalancingPolicy = cluster.LoadBalancingPolicy,
            HealthCheck = new HealthCheckConfig
            {
                AvailableDestinationsPolicy = cluster.HealthCheck?.AvailableDestinationsPolicy,
                Active = new ActiveHealthCheckConfig
                {
                    Enabled = cluster.HealthCheck?.Active?.Enabled,
                    Interval = TimeSpan.TryParse(cluster.HealthCheck?.Active?.Interval, CultureInfo.InvariantCulture, out var interval) ? interval : null,
                    Timeout = TimeSpan.TryParse(cluster.HealthCheck?.Active?.Timeout, CultureInfo.InvariantCulture, out var timeout) ? timeout : null,
                    Policy = cluster.HealthCheck?.Active?.Policy,
                    Path = cluster.HealthCheck?.Active?.Path,
                    Query = cluster.HealthCheck?.Active?.Query
                },
                Passive = new PassiveHealthCheckConfig
                {
                    Enabled = cluster.HealthCheck?.Passive?.Enabled,
                    Policy = cluster.HealthCheck?.Passive?.Policy,
                    ReactivationPeriod = TimeSpan.TryParse(cluster.HealthCheck?.Passive?.ReactivationPeriod, CultureInfo.InvariantCulture, out var reactivationPeriod) ? reactivationPeriod : null
                }
            },
            SessionAffinity = new SessionAffinityConfig
            {
                Enabled = cluster.SessionAffinity?.Enabled,
                Policy = cluster.SessionAffinity?.Policy,
                FailurePolicy = cluster.SessionAffinity?.FailurePolicy,
                AffinityKeyName = cluster.SessionAffinity?.AffinityKeyName ?? string.Empty,
                Cookie = new SessionAffinityCookieConfig
                {
                    Path = cluster.SessionAffinity?.Cookie?.Path,
                    Domain = cluster.SessionAffinity?.Cookie?.Domain,
                    HttpOnly = cluster.SessionAffinity?.Cookie?.HttpOnly,
                    Expiration = TimeSpan.TryParse(cluster.SessionAffinity?.Cookie?.Expiration, CultureInfo.InvariantCulture, out var expiration) ? expiration : null,
                    MaxAge = TimeSpan.TryParse(cluster.SessionAffinity?.Cookie?.MaxAge, CultureInfo.InvariantCulture, out var maxAge) ? maxAge : null,
                    IsEssential = cluster.SessionAffinity?.Cookie?.IsEssential
                }
            },
            Destinations = cluster.Destinations?.ToDictionary(x => x.DestinationId, x => new DestinationConfig
            {
                Host = x.Host,
                Health = x.Health,
                Address = x.Address
            })
        };

        return clusterConfig;
    }

    private static RouteConfig ConvertRoute(Route route)
    {
        var metadata = new Dictionary<string, string>();
        
        if (!string.IsNullOrEmpty(route.RateLimiterPolicy))
        {
            metadata["RateLimiterPolicy"] = route.RateLimiterPolicy;
        }

        if (route.HttpsRedirect)
        {
            metadata["HttpsRedirect"] = "true";
        }

        return new RouteConfig
        {
            RouteId = route.RouteId,
            ClusterId = route.ClusterId,
            Match = new YarpRouteMatch
            {
                Hosts = route.Match.Hosts.ToArray(),
                Path = route.Match.Path,
                Methods = route.Match.Methods.ToArray(),
                Headers = route.Match.Headers.Select(x => new RouteHeader
                {
                    Name = x.Name,
                    Values = x.Values.ToArray(),
                    Mode = (HeaderMatchMode)x.Mode,
                    IsCaseSensitive = x.IsCaseSensitive
                }).ToArray(),
                QueryParameters = route.Match.QueryParameters.Select(x => new RouteQueryParameter
                {
                    Name = x.Name,
                    Values = x.Values.ToArray(),
                    Mode = (QueryParameterMatchMode)x.Mode,
                    IsCaseSensitive = x.IsCaseSensitive
                }).ToArray()
            },
            AuthorizationPolicy = route.AuthorizationPolicy,
            RateLimiterPolicy = null,
            Timeout = TimeSpan.TryParse(route.Timeout, CultureInfo.InvariantCulture, out var timeout) ? timeout : null,
            TimeoutPolicy = route.TimeoutPolicy,
            CorsPolicy = route.CorsPolicy,
            MaxRequestBodySize = route.MaxRequestBodySize,
            Metadata = metadata,
            Transforms = route.Transforms.Select(x => x.Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)).ToArray(),
            Order = route.Order
        };
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _subscriptions?.Dispose();
            _changeToken?.Dispose();
            _disposed = true;
        }
    }
}

