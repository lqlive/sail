using ErrorOr;

namespace Sail.Route.Errors;

public static class QueryParameterErrors
{
    public static Error NameRequired => Error.Validation(
        code: "QueryParameter.NameRequired",
        description: "Query parameter name is required");

    public static Error NameTooLong => Error.Validation(
        code: "QueryParameter.NameTooLong",
        description: "Query parameter name must not exceed 100 characters");

    public static Error ValuesRequired => Error.Validation(
        code: "QueryParameter.ValuesRequired",
        description: "Query parameter values are required");

    public static Error ValuesEmpty => Error.Validation(
        code: "QueryParameter.ValuesEmpty",
        description: "At least one query parameter value must be provided");

    public static Error ValuesContainEmpty => Error.Validation(
        code: "QueryParameter.ValuesContainEmpty",
        description: "Query parameter values must not be empty");

    public static Error ModeInvalid => Error.Validation(
        code: "QueryParameter.ModeInvalid",
        description: "Invalid query parameter match mode");
}

