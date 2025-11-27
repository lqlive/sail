namespace Sail.Cluster.Models;

public record ActiveHealthCheckRequest
{
    public bool? Enabled { get; init; }
    public TimeSpan? Interval { get; init; }
    public TimeSpan? Timeout { get; init; }
    public string? Policy { get; init; }
    public string? Path { get; init; }
    public string? Query { get; init; }
}
