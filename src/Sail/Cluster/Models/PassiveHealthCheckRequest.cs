namespace Sail.Cluster.Models;

public record PassiveHealthCheckRequest
{
    public bool? Enabled { get; init; }
    public string? Policy { get; init; }
    public TimeSpan? ReactivationPeriod { get; init; }
}
