using FluentValidation;
using Sail.AuthenticationPolicy.Models;

namespace Sail.AuthenticationPolicy.Validators;

public class OpenIdConnectConfigRequestValidator : AbstractValidator<OpenIdConnectConfigRequest>
{
    public OpenIdConnectConfigRequestValidator()
    {
        RuleFor(x => x.Authority)
            .NotEmpty()
            .WithMessage("OpenID Connect authority is required")
            .Must(authority => Uri.TryCreate(authority, UriKind.Absolute, out _))
            .WithMessage("OpenID Connect authority must be a valid absolute URL");

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("OpenID Connect client ID is required")
            .MaximumLength(200)
            .WithMessage("Client ID must not exceed 200 characters");

        RuleFor(x => x.ClientSecret)
            .NotEmpty()
            .WithMessage("OpenID Connect client secret is required")
            .MaximumLength(500)
            .WithMessage("Client secret must not exceed 500 characters");

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
            .WithMessage("Invalid response type. Valid values are: code, id_token, token, or their combinations");

        RuleFor(x => x.Scope)
            .Must(scopes => scopes == null || scopes.All(s => !string.IsNullOrWhiteSpace(s)))
            .When(x => x.Scope != null && x.Scope.Any())
            .WithMessage("Scopes must not contain empty values")
            .Must(scopes => scopes == null || scopes.Contains("openid"))
            .When(x => x.Scope != null && x.Scope.Any())
            .WithMessage("OpenID Connect scope must include 'openid'");

        RuleFor(x => x.ClockSkew)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ClockSkew.HasValue)
            .WithMessage("Clock skew must be greater than or equal to 0 seconds");
    }
}

