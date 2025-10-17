namespace Sail.Compass.Watchers;
public readonly struct ResourceEvent<TResource>(EventType eventType, TResource value, TResource oldValue = default!)
{
    public EventType EventType { get; } = eventType;
    public TResource OldValue { get; } = oldValue;
    public TResource Value { get; } = value;
}

public enum EventType
{
    Unknown = 0,
    Created = 1,
    Updated = 2,
    Deleted = 3,
    List = 4
}