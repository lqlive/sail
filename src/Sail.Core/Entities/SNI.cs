namespace Sail.Core.Entities;

public class SNI
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string HostName { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
}