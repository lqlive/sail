namespace Sail.Core.Entities;

public class Destination
{
    public Guid Id { get; init; }
    public string Address { get; init; }
    public string? Health { get; init; }
    public string? Host { get; init; }
}