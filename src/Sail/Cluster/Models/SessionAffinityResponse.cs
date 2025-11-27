namespace Sail.Cluster.Models;

public record SessionAffinityResponse
{
    public bool? Enabled { get; init; }
    public string? Policy { get; init; }
    public string? FailurePolicy { get; init; }
    public string? AffinityKeyName { get; init; }
    public SessionAffinityCookieResponse? Cookie { get; init; }
}
