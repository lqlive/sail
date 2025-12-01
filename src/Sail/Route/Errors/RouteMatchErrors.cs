using ErrorOr;

namespace Sail.Route.Errors;

public static class RouteMatchErrors
{
    public static Error PathRequired => Error.Validation(
        code: "RouteMatch.PathRequired",
        description: "Route path is required");

    public static Error PathInvalid => Error.Validation(
        code: "RouteMatch.PathInvalid",
        description: "Route path must start with '/' or a route parameter '{'");

    public static Error MethodsContainEmpty => Error.Validation(
        code: "RouteMatch.MethodsContainEmpty",
        description: "HTTP methods must not be empty");

    public static Error HostsContainEmpty => Error.Validation(
        code: "RouteMatch.HostsContainEmpty",
        description: "Host names must not be empty");
}

