namespace Sail.Core.Entities;

public class SessionAffinity
{
    public bool? Enabled { get; init; }
    public string? Policy { get; init; }
    public string? FailurePolicy { get; init; }
    public string? AffinityKeyName { get; init; }
    public SessionAffinityCookie? Cookie { get; init; }
}