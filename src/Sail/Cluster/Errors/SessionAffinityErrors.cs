using ErrorOr;

namespace Sail.Cluster.Errors;

public static class SessionAffinityErrors
{
    public static Error PolicyRequired => Error.Validation(
        code: "SessionAffinity.PolicyRequired",
        description: "Session affinity policy is required when enabled");
}

public static class SessionAffinityCookieErrors
{
    public static Error SecurePolicyInvalid => Error.Validation(
        code: "SessionAffinityCookie.SecurePolicyInvalid",
        description: "Invalid cookie secure policy");

    public static Error SameSiteInvalid => Error.Validation(
        code: "SessionAffinityCookie.SameSiteInvalid",
        description: "Invalid cookie SameSite mode");

    public static Error ExpirationInvalid => Error.Validation(
        code: "SessionAffinityCookie.ExpirationInvalid",
        description: "Cookie expiration must be greater than zero");

    public static Error MaxAgeInvalid => Error.Validation(
        code: "SessionAffinityCookie.MaxAgeInvalid",
        description: "Cookie max age must be greater than zero");
}

