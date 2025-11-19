namespace Sail.Core.RateLimiter;

public class RateLimiterPolicyConfig
{
    public required string Name { get; init; }
    public int PermitLimit { get; init; }
    public int Window { get; init; }
    public int QueueLimit { get; init; }
}
