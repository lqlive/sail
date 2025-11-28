using FluentValidation;
using Sail.AuthenticationPolicy.Models;
using Sail.AuthenticationPolicy.Errors;

namespace Sail.AuthenticationPolicy.Validators;

public class OpenIdConnectConfigRequestValidator : AbstractValidator<OpenIdConnectConfigRequest>
{
    public OpenIdConnectConfigRequestValidator()
    {
        RuleFor(x => x.Authority)
            .NotEmpty()
            .WithMessage(OpenIdConnectErrors.AuthorityRequired.Description)
            .WithErrorCode(OpenIdConnectErrors.AuthorityRequired.Code)
            .Must(authority => Uri.TryCreate(authority, UriKind.Absolute, out _))
            .WithMessage(OpenIdConnectErrors.AuthorityInvalid.Description)
            .WithErrorCode(OpenIdConnectErrors.AuthorityInvalid.Code);

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage(OpenIdConnectErrors.ClientIdRequired.Description)
            .WithErrorCode(OpenIdConnectErrors.ClientIdRequired.Code)
            .MaximumLength(200)
            .WithMessage(OpenIdConnectErrors.ClientIdTooLong.Description)
            .WithErrorCode(OpenIdConnectErrors.ClientIdTooLong.Code);

        RuleFor(x => x.ClientSecret)
            .NotEmpty()
            .WithMessage(OpenIdConnectErrors.ClientSecretRequired.Description)
            .WithErrorCode(OpenIdConnectErrors.ClientSecretRequired.Code)
            .MaximumLength(500)
            .WithMessage(OpenIdConnectErrors.ClientSecretTooLong.Description)
            .WithErrorCode(OpenIdConnectErrors.ClientSecretTooLong.Code);

        RuleFor(x => x.ResponseType)
            .Must(rt => string.IsNullOrEmpty(rt) || 
                        rt == "code" || 
                        rt == "id_token" || 
                        rt == "token" || 
                        rt == "id_token token" || 
                        rt == "code id_token" || 
                        rt == "code token" || 
                        rt == "code id_token token")
            .When(x => !string.IsNullOrEmpty(x.ResponseType))
            .WithMessage(OpenIdConnectErrors.ResponseTypeInvalid.Description)
            .WithErrorCode(OpenIdConnectErrors.ResponseTypeInvalid.Code);

        RuleFor(x => x.Scope)
            .Must(scopes => scopes == null || scopes.All(s => !string.IsNullOrWhiteSpace(s)))
            .When(x => x.Scope != null && x.Scope.Any())
            .WithMessage(OpenIdConnectErrors.ScopesContainEmpty.Description)
            .WithErrorCode(OpenIdConnectErrors.ScopesContainEmpty.Code)
            .Must(scopes => scopes == null || scopes.Contains("openid"))
            .When(x => x.Scope != null && x.Scope.Any())
            .WithMessage(OpenIdConnectErrors.ScopesMissingOpenId.Description)
            .WithErrorCode(OpenIdConnectErrors.ScopesMissingOpenId.Code);

        RuleFor(x => x.ClockSkew)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ClockSkew.HasValue)
            .WithMessage(OpenIdConnectErrors.ClockSkewInvalid.Description)
            .WithErrorCode(OpenIdConnectErrors.ClockSkewInvalid.Code);
    }
}
