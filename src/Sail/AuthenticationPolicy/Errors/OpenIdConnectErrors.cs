using ErrorOr;

namespace Sail.AuthenticationPolicy.Errors;

public static class OpenIdConnectErrors
{
    public static Error AuthorityRequired => Error.Validation(
        code: "OpenIdConnect.AuthorityRequired",
        description: "OpenID Connect authority is required");

    public static Error AuthorityInvalid => Error.Validation(
        code: "OpenIdConnect.AuthorityInvalid",
        description: "OpenID Connect authority must be a valid absolute URL");

    public static Error ClientIdRequired => Error.Validation(
        code: "OpenIdConnect.ClientIdRequired",
        description: "OpenID Connect client ID is required");

    public static Error ClientIdTooLong => Error.Validation(
        code: "OpenIdConnect.ClientIdTooLong",
        description: "Client ID must not exceed 200 characters");

    public static Error ClientSecretRequired => Error.Validation(
        code: "OpenIdConnect.ClientSecretRequired",
        description: "OpenID Connect client secret is required");

    public static Error ClientSecretTooLong => Error.Validation(
        code: "OpenIdConnect.ClientSecretTooLong",
        description: "Client secret must not exceed 500 characters");

    public static Error ResponseTypeInvalid => Error.Validation(
        code: "OpenIdConnect.ResponseTypeInvalid",
        description: "Invalid response type. Valid values are: code, id_token, token, or their combinations");

    public static Error ScopesContainEmpty => Error.Validation(
        code: "OpenIdConnect.ScopesContainEmpty",
        description: "Scopes must not contain empty values");

    public static Error ScopesMissingOpenId => Error.Validation(
        code: "OpenIdConnect.ScopesMissingOpenId",
        description: "OpenID Connect scope must include 'openid'");

    public static Error ClockSkewInvalid => Error.Validation(
        code: "OpenIdConnect.ClockSkewInvalid",
        description: "Clock skew must be greater than or equal to 0 seconds");
}

