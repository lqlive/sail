namespace Sail.Core.Entities;

public class Destination
{
    public Guid Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? Health { get; set; }
    public string? Host { get; set; }
}