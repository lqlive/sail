namespace Sail.Core.Retry;

public class RetryPolicyConfig
{
    public required string Name { get; init; }
    public int MaxRetryAttempts { get; init; } = 1;
    public int[] RetryStatusCodes { get; init; } = [500, 502, 503, 504];
    public int RetryDelayMilliseconds { get; init; } = 1000;
    public bool UseExponentialBackoff { get; init; } = false;
}

