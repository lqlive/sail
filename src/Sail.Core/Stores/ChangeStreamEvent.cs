namespace Sail.Core.Stores;

public record ChangeStreamEvent<T>
{
    public required T Document { get; init; }
    public required ChangeStreamType OperationType { get; init; }
}

public enum ChangeStreamType
{
    Insert,
    Update,
    Delete,
    Replace,
    Invalidate,
    Drop,
    DropDatabase,
    Rename
}

