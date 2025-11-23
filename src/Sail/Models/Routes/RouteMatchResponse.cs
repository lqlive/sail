namespace Sail.Models.Routes;

public record RouteMatchResponse
{
    public List<string> Methods { get; init; }
    public List<string> Hosts { get; init; }
    public IEnumerable<RouteHeaderResponse> Headers { get; init; }
    public string Path { get; init; }
    public IEnumerable<RouteQueryParameterResponse> QueryParameters { get; init; }
}
