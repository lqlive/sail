namespace Sail.Core.Entities;

public class Cluster
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? ServiceName { get; init; }
    public ServiceDiscoveryType? ServiceDiscoveryType { get; init; }
    public string? LoadBalancingPolicy { get; init; }
    public HealthCheck? HealthCheck { get; init; }
    public SessionAffinity? SessionAffinity { get; init; }
    public List<Destination>? Destinations { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
}