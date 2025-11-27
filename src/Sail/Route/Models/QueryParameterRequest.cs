using Sail.Core.Entities;

namespace Sail.Route.Models;

public record QueryParameterRequest(
    string Name,
    List<string> Values,
    QueryParameterMatchMode Mode,
    bool IsCaseSensitive);