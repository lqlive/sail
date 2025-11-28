using ErrorOr;

namespace Sail.Middleware.Errors;

public static class RateLimiterErrors
{
    public static Error NameRequired => Error.Validation(
        code: "RateLimiter.NameRequired",
        description: "Rate limiter policy name is required");

    public static Error NameTooLong => Error.Validation(
        code: "RateLimiter.NameTooLong",
        description: "Rate limiter policy name must not exceed 200 characters");

    public static Error PermitLimitInvalid => Error.Validation(
        code: "RateLimiter.PermitLimitInvalid",
        description: "Rate limiter permit limit must be greater than 0");

    public static Error WindowInvalid => Error.Validation(
        code: "RateLimiter.WindowInvalid",
        description: "Rate limiter window must be greater than 0 seconds");

    public static Error QueueLimitInvalid => Error.Validation(
        code: "RateLimiter.QueueLimitInvalid",
        description: "Rate limiter queue limit must be greater than or equal to 0");
}

