using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Primitives;
using Sail.Compass.Watchers;
using Sail.Core.Entities;
using Yarp.ReverseProxy.Configuration;

namespace Sail.Compass.ConfigProvider;

internal sealed class DataSourceConfigProvider : IProxyConfigProvider, IDisposable
{
    private readonly BehaviorSubject<ConfigurationSnapshot> _configSubject;
    private readonly CompositeDisposable _subscriptions = new();
    private bool _disposed;

    public DataSourceConfigProvider(
        ResourceWatcher<Route> routeWatcher,
        ResourceWatcher<Cluster> clusterWatcher)
    {
        _configSubject = new BehaviorSubject<ConfigurationSnapshot>(CreateEmptySnapshot());

        var routes = CreateRouteStream(routeWatcher);
        var clusters = CreateClusterStream(clusterWatcher);

        var subscription = Observable.CombineLatest(routes, clusters, UpdateSnapshot)
            .Catch<ConfigurationSnapshot, Exception>(ex => Observable.Return(_configSubject.Value))
            .Subscribe(_configSubject.OnNext, _configSubject.OnError);
        
        _subscriptions.Add(subscription);
    }

    public IProxyConfig GetConfig() => _configSubject.Value;

    private IObservable<Dictionary<string, Route>> CreateRouteStream(
        ResourceWatcher<Route> watcher)
    {
        return watcher
            .GetObservable(watch: true)
            .Scan(
                seed: new Dictionary<string, Route>(StringComparer.OrdinalIgnoreCase),
                accumulator: (dict, evt) =>
                {
                    var key = evt.Value.Id.ToString();
                    var newDict = new Dictionary<string, Route>(dict, dict.Comparer);
                    
                    switch (evt.EventType)
                    {
                        case EventType.List:
                        case EventType.Created:
                        case EventType.Updated:
                            newDict[key] = evt.Value;
                            break;
                        case EventType.Deleted:
                            newDict.Remove(key);
                            break;
                    }
                    
                    return newDict;
                })
            .Throttle(TimeSpan.FromMilliseconds(100))
            .StartWith(new Dictionary<string, Route>(StringComparer.OrdinalIgnoreCase));
    }

    private IObservable<Dictionary<string, Cluster>> CreateClusterStream(
        ResourceWatcher<Cluster> watcher)
    {
        return watcher
            .GetObservable(watch: true)
            .Scan(
                seed: new Dictionary<string, Cluster>(StringComparer.OrdinalIgnoreCase),
                accumulator: (dict, evt) =>
                {
                    var key = evt.Value.Id.ToString();
                    var newDict = new Dictionary<string, Cluster>(dict, dict.Comparer);
                    
                    switch (evt.EventType)
                    {
                        case EventType.List:
                        case EventType.Created:
                        case EventType.Updated:
                            newDict[key] = evt.Value;
                            break;
                        case EventType.Deleted:
                            newDict.Remove(key);
                            break;
                    }
                    
                    return newDict;
                })
            .Throttle(TimeSpan.FromMilliseconds(100))
            .StartWith(new Dictionary<string, Cluster>(StringComparer.OrdinalIgnoreCase));
    }

    private ConfigurationSnapshot UpdateSnapshot(
        Dictionary<string, Route> routes,
        Dictionary<string, Cluster> clusters)
    {
        var snapshot = CreateEmptySnapshot();
        
        foreach (var route in routes.Values)
        {
        }
        
        foreach (var cluster in clusters.Values)
        {
        }

        return snapshot;
    }

    private static ConfigurationSnapshot CreateEmptySnapshot()
    {
        var cts = new CancellationTokenSource();
        return new ConfigurationSnapshot
        {
            ChangeToken = new CancellationChangeToken(cts.Token)
        };
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _subscriptions?.Dispose();
            _configSubject?.Dispose();
            _disposed = true;
        }
    }
}