namespace Sail.Compass.DataSources;
public interface IConfigurationSource : IAsyncDisposable
{
    string Name { get; }
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}