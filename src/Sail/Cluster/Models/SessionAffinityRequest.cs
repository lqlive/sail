namespace Sail.Cluster.Models;

public record SessionAffinityRequest
{
    public bool Enabled { get; init; }
    public string? Policy { get; init; }
    public string? FailurePolicy { get; init; }
    public string? AffinityKeyName { get; init; }
    public SessionAffinityCookieRequest? Cookie { get; init; }
}
