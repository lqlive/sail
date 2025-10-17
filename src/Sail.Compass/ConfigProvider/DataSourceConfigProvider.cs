using System.Reactive.Subjects;
using Yarp.ReverseProxy.Configuration;

namespace Sail.Compass.ConfigProvider;

internal sealed class DataSourceConfigProvider : IProxyConfigProvider, IDisposable
{
    private readonly Subject<ConfigurationSnapshot>  _subscription  = new();
    private ConfigurationSnapshot? _snapshot;
    public IProxyConfig GetConfig()
    {
        if (_snapshot is null)
        {
            _subscription.Subscribe(
                onNext: value => _snapshot = value);
        }

        return _snapshot;
    }

    public void Dispose()
    {
        _subscription.Dispose();
    }
}