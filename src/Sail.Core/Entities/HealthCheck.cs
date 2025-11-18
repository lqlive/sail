namespace Sail.Core.Entities;

public class HealthCheck
{
    public string? AvailableDestinationsPolicy { get; init; }
    public ActiveHealthCheck? Active { get; init; }
    public PassiveHealthCheck? Passive { get; init; }
}