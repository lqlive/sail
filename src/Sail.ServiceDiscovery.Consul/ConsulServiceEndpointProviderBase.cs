using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.ServiceDiscovery;

namespace Sail.ServiceDiscovery.Consul;

internal abstract partial class ConsulServiceEndpointProviderBase : IServiceEndpointProvider
{
    private readonly Lock _lock = new();
    private readonly ILogger _logger;
    private readonly CancellationTokenSource _disposeCancellation = new();
    private readonly TimeProvider _timeProvider;
    private long _lastRefreshTimeStamp;
    private Task _resolveTask = Task.CompletedTask;
    private bool _hasEndpoints;
    private CancellationChangeToken _lastChangeToken;
    private CancellationTokenSource _lastCollectionCancellation;
    private List<ServiceEndpoint>? _lastEndpointCollection;
    private TimeSpan _nextRefreshPeriod;


    protected ConsulServiceEndpointProviderBase(
        ServiceEndpointQuery query,
        ILogger logger,
        TimeProvider timeProvider)
    {
        ServiceName = query.ToString()!;
        _logger = logger;
        _lastEndpointCollection = null;
        _timeProvider = timeProvider;
        _lastRefreshTimeStamp = _timeProvider.GetTimestamp();
        var cancellation = _lastCollectionCancellation = new CancellationTokenSource();
        _lastChangeToken = new CancellationChangeToken(cancellation.Token);
    }

    private TimeSpan ElapsedSinceRefresh => _timeProvider.GetElapsedTime(_lastRefreshTimeStamp);

    protected string ServiceName { get; }

    protected abstract double RetryBackOffFactor { get; }

    protected abstract TimeSpan MinRetryPeriod { get; }

    protected abstract TimeSpan MaxRetryPeriod { get; }

    protected abstract TimeSpan DefaultRefreshPeriod { get; }

    protected CancellationToken ShutdownToken => _disposeCancellation.Token;
    private bool ShouldRefresh() => _lastEndpointCollection is null || _lastChangeToken is { HasChanged: true } || ElapsedSinceRefresh >= _nextRefreshPeriod;

    protected abstract Task ResolveAsyncCore();

    public async ValueTask PopulateAsync(IServiceEndpointBuilder endpoints, CancellationToken cancellationToken)
    {
        if (endpoints.Endpoints.Count != 0)
        {
            Log.SkippedResolution(_logger, ServiceName, "Collection has existing endpoints");
            return;
        }

        if (ShouldRefresh())
        {
            Task resolveTask;
            lock (_lock)
            {
                if (_resolveTask.IsCompleted && ShouldRefresh())
                {
                    _resolveTask = ResolveAsyncCore();
                }

                resolveTask = _resolveTask;
            }

            await resolveTask.WaitAsync(cancellationToken).ConfigureAwait(false);
        }

        lock (_lock)
        {
            if (_lastEndpointCollection is { Count: > 0 } eps)
            {
                foreach (var ep in eps)
                {
                    endpoints.Endpoints.Add(ep);
                }
            }

            endpoints.AddChangeToken(_lastChangeToken);
        }
    }
    protected void SetResult(List<ServiceEndpoint> endpoints, TimeSpan validityPeriod)
    {
        lock (_lock)
        {
            if (endpoints is { Count: > 0 })
            {
                _lastRefreshTimeStamp = _timeProvider.GetTimestamp();
                _nextRefreshPeriod = DefaultRefreshPeriod;
                _hasEndpoints = true;
            }
            else
            {
                _nextRefreshPeriod = GetRefreshPeriod();
                validityPeriod = TimeSpan.Zero;
                _hasEndpoints = false;
            }

            if (validityPeriod <= TimeSpan.Zero)
            {
                validityPeriod = _nextRefreshPeriod;
            }
            else if (validityPeriod > _nextRefreshPeriod)
            {
                validityPeriod = _nextRefreshPeriod;
            }

            _lastCollectionCancellation.Cancel();
            var cancellation = _lastCollectionCancellation = new CancellationTokenSource(validityPeriod, _timeProvider);
            _lastChangeToken = new CancellationChangeToken(cancellation.Token);
            _lastEndpointCollection = endpoints;
        }

        return;

        TimeSpan GetRefreshPeriod()
        {
            if (_hasEndpoints)
            {
                return MinRetryPeriod;
            }

            var nextTicks = (long)(_nextRefreshPeriod.Ticks * RetryBackOffFactor);
            if (nextTicks <= 0 || nextTicks > MaxRetryPeriod.Ticks)
            {
                return MaxRetryPeriod;
            }

            return TimeSpan.FromTicks(nextTicks);
        }
    }
    public async ValueTask DisposeAsync()
    {
        await _disposeCancellation.CancelAsync();

        if (_resolveTask is { } task)
        {
            await task.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
    }
}