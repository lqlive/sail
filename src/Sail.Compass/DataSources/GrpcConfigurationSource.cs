using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Sail.Compass.Hosting;

namespace Sail.Compass.DataSources;

public sealed class GrpcConfigurationSource : BackgroundHostedService, IConfigurationSource, IAsyncDisposable
{
    private readonly Lock _sync = new();
    private readonly SemaphoreSlim _ready = new(0);
    private readonly SemaphoreSlim _start = new(0);

    public GrpcConfigurationSource(IHostApplicationLifetime hostApplicationLifetime, ILogger logger) 
        : base(hostApplicationLifetime, logger)
    {
    }

    public string Name => "GrpcSource";

    public override Task RunAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
