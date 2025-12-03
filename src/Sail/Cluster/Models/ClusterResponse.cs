using Sail.Core.Entities;

namespace Sail.Cluster.Models;

public record ClusterResponse
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public bool Enabled { get; init; }
    public string? ServiceName { get; init; }
    public ServiceDiscoveryType? ServiceDiscoveryType { get; init; }
    public string? LoadBalancingPolicy { get; init; }
    public HealthCheckResponse? HealthCheck { get; init; }
    public SessionAffinityResponse? SessionAffinity { get; set; }
    public IEnumerable<DestinationResponse>? Destinations { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
