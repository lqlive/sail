using ErrorOr;

namespace Sail.Route.Errors;

public static class RouteErrors
{
    public static Error RouteNotFound => Error.NotFound(
        code: "Route.NotFound",
        description: "Route not found");
}
