namespace Sail.Certificate.Models;

public record SNIResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string HostName { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
