using ErrorOr;

namespace Sail.Middleware.Errors;

public static class CorsErrors
{
    public static Error NameRequired => Error.Validation(
        code: "Cors.NameRequired",
        description: "CORS policy name is required");

    public static Error NameTooLong => Error.Validation(
        code: "Cors.NameTooLong",
        description: "CORS policy name must not exceed 200 characters");

    public static Error AllowOriginsContainEmpty => Error.Validation(
        code: "Cors.AllowOriginsContainEmpty",
        description: "CORS allow origins must not contain empty values");

    public static Error AllowMethodsContainEmpty => Error.Validation(
        code: "Cors.AllowMethodsContainEmpty",
        description: "CORS allow methods must not contain empty values");

    public static Error AllowHeadersContainEmpty => Error.Validation(
        code: "Cors.AllowHeadersContainEmpty",
        description: "CORS allow headers must not contain empty values");

    public static Error ExposeHeadersContainEmpty => Error.Validation(
        code: "Cors.ExposeHeadersContainEmpty",
        description: "CORS expose headers must not contain empty values");

    public static Error MaxAgeInvalid => Error.Validation(
        code: "Cors.MaxAgeInvalid",
        description: "CORS max age must be greater than or equal to 0");
}

