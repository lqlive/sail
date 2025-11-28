using ErrorOr;

namespace Sail.Route.Errors;

public static class RouteHeaderErrors
{
    public static Error NameRequired => Error.Validation(
        code: "RouteHeader.NameRequired",
        description: "Header name is required");

    public static Error NameTooLong => Error.Validation(
        code: "RouteHeader.NameTooLong",
        description: "Header name must not exceed 100 characters");

    public static Error ValuesRequired => Error.Validation(
        code: "RouteHeader.ValuesRequired",
        description: "Header values are required");

    public static Error ValuesEmpty => Error.Validation(
        code: "RouteHeader.ValuesEmpty",
        description: "At least one header value must be provided");

    public static Error ValuesContainEmpty => Error.Validation(
        code: "RouteHeader.ValuesContainEmpty",
        description: "Header values must not be empty");

    public static Error ModeInvalid => Error.Validation(
        code: "RouteHeader.ModeInvalid",
        description: "Invalid header match mode");
}

