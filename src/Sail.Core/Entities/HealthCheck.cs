namespace Sail.Core.Entities;

public class HealthCheck
{
    public string? AvailableDestinationsPolicy { get; set; }
    public ActiveHealthCheck? Active { get; set; }
    public PassiveHealthCheck? Passive { get; set; }
}