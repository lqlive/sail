using ErrorOr;

namespace Sail.Middleware.Errors;

public static class MiddlewareErrors
{
    public static Error MiddlewareNotFound => Error.NotFound(
    code: "Middleware.NotFound",
    description: "Middleware not found");
}
