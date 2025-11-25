namespace Sail.Core.Entities;

public class RouteMatch
{
    public List<string> Methods { get; set; } = new();
    public List<string> Hosts { get; set; } = new();
    public List<RouteHeader> Headers { get; set; } = new();
    public string Path { get; set; } = string.Empty;
    public List<RouteQueryParameter> QueryParameters { get; set; } = new();
}