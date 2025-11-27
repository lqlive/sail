namespace Sail.Cluster.Models;

public record HealthCheckResponse
{
    public string? AvailableDestinationsPolicy { get; set; }
    public ActiveHealthCheckResponse? Active { get; init; }
    public PassiveHealthCheckResponse? Passive { get; init; }
}
