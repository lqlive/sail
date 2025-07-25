namespace Sail.Compass.Watchers;
public abstract class ResourceWatcher<T>
{
    protected abstract IObservable<ResourceEvent<T>> GetObservable(bool watch);
}