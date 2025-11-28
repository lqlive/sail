using ErrorOr;

namespace Sail.AuthenticationPolicy.Errors;

public static class AuthenticationPolicyErrors
{
    public static Error NameRequired => Error.Validation(
        code: "AuthenticationPolicy.NameRequired",
        description: "Authentication policy name is required");

    public static Error NameTooLong => Error.Validation(
        code: "AuthenticationPolicy.NameTooLong",
        description: "Authentication policy name must not exceed 200 characters");

    public static Error TypeInvalid => Error.Validation(
        code: "AuthenticationPolicy.TypeInvalid",
        description: "Invalid authentication scheme type");

    public static Error JwtBearerConfigRequired => Error.Validation(
        code: "AuthenticationPolicy.JwtBearerConfigRequired",
        description: "JWT Bearer configuration is required when type is JwtBearer");

    public static Error OpenIdConnectConfigRequired => Error.Validation(
        code: "AuthenticationPolicy.OpenIdConnectConfigRequired",
        description: "OpenID Connect configuration is required when type is OpenIdConnect");

    public static Error DescriptionTooLong => Error.Validation(
        code: "AuthenticationPolicy.DescriptionTooLong",
        description: "Description must not exceed 500 characters");
}

