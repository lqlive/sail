namespace Sail.Route.Models;

public record RouteMatchRequest(
    string Path,
    List<string>? Methods,
    List<string>? Hosts,
    List<QueryParameterRequest>? QueryParameters,
    List<RouteHeaderRequest>? Headers);