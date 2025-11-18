namespace Sail.Core.Entities;

public class PassiveHealthCheck
{
    public bool? Enabled { get; init; }
    public string? Policy { get; init; }
    public TimeSpan? ReactivationPeriod { get; init; }
}