using ErrorOr;

namespace Sail.Route.Errors;

public static class RouteErrors
{
    public static Error NameRequired => Error.Validation(
        code: "Route.NameRequired",
        description: "Route name is required");

    public static Error NameTooLong => Error.Validation(
        code: "Route.NameTooLong",
        description: "Route name must not exceed 200 characters");

    public static Error MatchRequired => Error.Validation(
        code: "Route.MatchRequired",
        description: "Route match configuration is required");

    public static Error OrderInvalid => Error.Validation(
        code: "Route.OrderInvalid",
        description: "Route order must be a non-negative number");

    public static Error ClusterIdInvalid => Error.Validation(
        code: "Route.ClusterIdInvalid",
        description: "ClusterId must be a valid GUID when provided");

    public static Error TimeoutInvalid => Error.Validation(
        code: "Route.TimeoutInvalid",
        description: "Timeout must be greater than zero when specified");

    public static Error MaxRequestBodySizeInvalid => Error.Validation(
        code: "Route.MaxRequestBodySizeInvalid",
        description: "MaxRequestBodySize must be greater than zero when specified");

    public static Error TransformsInvalid => Error.Validation(
        code: "Route.TransformsInvalid",
        description: "Each transform must contain at least one key-value pair");
}
