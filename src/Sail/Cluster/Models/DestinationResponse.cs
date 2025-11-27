namespace Sail.Cluster.Models;

public record DestinationResponse
{
    public Guid Id { get; set; }
    public string? Address { get; init; }
    public string? Health { get; init; }
    public string? Host { get; init; }
}
