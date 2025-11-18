namespace Sail.Compass.Observers;
public abstract class ResourceObserver<T>
{
    public abstract IObservable<ResourceEvent<T>> GetObservable(bool watch);
}

