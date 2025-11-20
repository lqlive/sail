namespace Sail.Core.Timeout;

public class TimeoutPolicyConfig
{
    public required string Name { get; init; }
    public required int Seconds { get; init; }
    public int? TimeoutStatusCode { get; init; }
}
