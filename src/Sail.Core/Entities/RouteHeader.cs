namespace Sail.Core.Entities;

public class RouteHeader
{
    public string Name { get; set; } = string.Empty;
    public List<string>? Values { get; set; }
    public HeaderMatchMode Mode { get; set; }
    public bool IsCaseSensitive { get; set; }
}