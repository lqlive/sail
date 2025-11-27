using Sail.Core.Entities;

namespace Sail.Route.Models;

public record RouteHeaderRequest(string Name, List<string> Values, HeaderMatchMode Mode, bool IsCaseSensitive);
