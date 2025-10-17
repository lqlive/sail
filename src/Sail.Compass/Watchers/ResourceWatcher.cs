namespace Sail.Compass.Watchers;
public abstract class ResourceWatcher<T>
{
    public abstract IObservable<ResourceEvent<T>> GetObservable(bool watch);
}