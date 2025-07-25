namespace Sail.Compass.DataSources;
public sealed class GrpcConfigurationSource : IConfigurationSource
{
    public string Name => "Grpc";

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
