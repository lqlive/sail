namespace Sail.Compass.DataSources;
public interface IConfigurationSource : IAsyncDisposable
{
    string Name { get; }
}