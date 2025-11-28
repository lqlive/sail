using ErrorOr;

namespace Sail.AuthenticationPolicy.Errors;

public static class JwtBearerErrors
{
    public static Error AuthorityRequired => Error.Validation(
        code: "JwtBearer.AuthorityRequired",
        description: "JWT Bearer authority is required");

    public static Error AuthorityInvalid => Error.Validation(
        code: "JwtBearer.AuthorityInvalid",
        description: "JWT Bearer authority must be a valid absolute URL");

    public static Error AudienceRequired => Error.Validation(
        code: "JwtBearer.AudienceRequired",
        description: "JWT Bearer audience is required");

    public static Error ValidIssuersContainEmpty => Error.Validation(
        code: "JwtBearer.ValidIssuersContainEmpty",
        description: "Valid issuers must not contain empty values");

    public static Error ValidAudiencesContainEmpty => Error.Validation(
        code: "JwtBearer.ValidAudiencesContainEmpty",
        description: "Valid audiences must not contain empty values");

    public static Error ClockSkewInvalid => Error.Validation(
        code: "JwtBearer.ClockSkewInvalid",
        description: "Clock skew must be greater than or equal to 0 seconds");
}

