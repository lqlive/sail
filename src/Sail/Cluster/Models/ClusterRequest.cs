using Sail.Core.Entities;

namespace Sail.Cluster.Models;

public record ClusterRequest(
    string Name,
    bool Enabled,
    string? ServiceName,
    ServiceDiscoveryType? ServiceDiscoveryType,
    HealthCheckRequest? HealthCheck,
    SessionAffinityRequest? SessionAffinity,
    string LoadBalancingPolicy,
    List<DestinationRequest> Destinations);
