namespace Sail.Core.Entities;

public class SessionAffinity
{
    public bool? Enabled { get; set; }
    public string? Policy { get; set; }
    public string? FailurePolicy { get; set; }
    public string? AffinityKeyName { get; set; }
    public SessionAffinityCookie? Cookie { get; set; }
}