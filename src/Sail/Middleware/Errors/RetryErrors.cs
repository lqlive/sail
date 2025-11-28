using ErrorOr;

namespace Sail.Middleware.Errors;

public static class RetryErrors
{
    public static Error NameRequired => Error.Validation(
        code: "Retry.NameRequired",
        description: "Retry policy name is required");

    public static Error NameTooLong => Error.Validation(
        code: "Retry.NameTooLong",
        description: "Retry policy name must not exceed 200 characters");

    public static Error MaxRetryAttemptsTooLow => Error.Validation(
        code: "Retry.MaxRetryAttemptsTooLow",
        description: "Max retry attempts must be greater than or equal to 0");

    public static Error MaxRetryAttemptsTooHigh => Error.Validation(
        code: "Retry.MaxRetryAttemptsTooHigh",
        description: "Max retry attempts should not exceed 10 for safety");

    public static Error StatusCodesRequired => Error.Validation(
        code: "Retry.StatusCodesRequired",
        description: "Retry status codes are required");

    public static Error StatusCodesEmpty => Error.Validation(
        code: "Retry.StatusCodesEmpty",
        description: "At least one retry status code must be provided");

    public static Error StatusCodesInvalid => Error.Validation(
        code: "Retry.StatusCodesInvalid",
        description: "Retry status codes must be between 400 and 599");

    public static Error DelayTooLow => Error.Validation(
        code: "Retry.DelayTooLow",
        description: "Retry delay must be greater than or equal to 0 milliseconds");

    public static Error DelayTooHigh => Error.Validation(
        code: "Retry.DelayTooHigh",
        description: "Retry delay should not exceed 60000 milliseconds (60 seconds)");
}

