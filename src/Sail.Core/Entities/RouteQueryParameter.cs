namespace Sail.Core.Entities;

public class RouteQueryParameter
{
    public string Name { get; init; }
    public List<string>? Values { get; init; }
    public QueryParameterMatchMode Mode { get; init; }
    public bool IsCaseSensitive { get; init; }
}