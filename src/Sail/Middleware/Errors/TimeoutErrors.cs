using ErrorOr;

namespace Sail.Middleware.Errors;

public static class TimeoutErrors
{
    public static Error NameRequired => Error.Validation(
        code: "Timeout.NameRequired",
        description: "Timeout policy name is required");

    public static Error NameTooLong => Error.Validation(
        code: "Timeout.NameTooLong",
        description: "Timeout policy name must not exceed 200 characters");

    public static Error SecondsInvalid => Error.Validation(
        code: "Timeout.SecondsInvalid",
        description: "Timeout seconds must be greater than 0");

    public static Error StatusCodeInvalid => Error.Validation(
        code: "Timeout.StatusCodeInvalid",
        description: "Timeout status code must be between 400 and 599");
}

