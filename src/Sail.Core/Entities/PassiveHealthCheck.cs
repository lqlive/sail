namespace Sail.Core.Entities;

public class PassiveHealthCheck
{
    public bool? Enabled { get; set; }
    public string? Policy { get; set; }
    public TimeSpan? ReactivationPeriod { get; set; }
}