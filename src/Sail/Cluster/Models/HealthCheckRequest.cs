namespace Sail.Cluster.Models;

public record HealthCheckRequest
{
    public string? AvailableDestinationsPolicy { get; set; }
    public ActiveHealthCheckRequest? Active { get; init; }
    public PassiveHealthCheckRequest? Passive { get; init; }
}
