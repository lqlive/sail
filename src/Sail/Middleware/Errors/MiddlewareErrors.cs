using ErrorOr;

namespace Sail.Middleware.Errors;

public static class MiddlewareErrors
{
    public static Error MiddlewareNotFound => Error.NotFound(
        code: "Middleware.NotFound",
        description: "Middleware not found");

    public static Error NameRequired => Error.Validation(
        code: "Middleware.NameRequired",
        description: "Middleware name is required");

    public static Error NameTooLong => Error.Validation(
        code: "Middleware.NameTooLong",
        description: "Middleware name must not exceed 200 characters");

    public static Error InvalidType => Error.Validation(
        code: "Middleware.InvalidType",
        description: "Invalid middleware type");

    public static Error CorsConfigRequired => Error.Validation(
        code: "Middleware.CorsConfigRequired",
        description: "CORS configuration is required when type is CORS");

    public static Error RateLimiterConfigRequired => Error.Validation(
        code: "Middleware.RateLimiterConfigRequired",
        description: "RateLimiter configuration is required when type is RateLimiter");

    public static Error TimeoutConfigRequired => Error.Validation(
        code: "Middleware.TimeoutConfigRequired",
        description: "Timeout configuration is required when type is Timeout");

    public static Error RetryConfigRequired => Error.Validation(
        code: "Middleware.RetryConfigRequired",
        description: "Retry configuration is required when type is Retry");
}
