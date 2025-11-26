using FluentValidation;
using Sail.Core.Entities;
using Sail.Models.AuthenticationPolicies;

namespace Sail.Validators.AuthenticationPolicies;

public class AuthenticationPolicyRequestValidator : AbstractValidator<AuthenticationPolicyRequest>
{
    public AuthenticationPolicyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Authentication policy name is required")
            .MaximumLength(200)
            .WithMessage("Authentication policy name must not exceed 200 characters");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid authentication scheme type");

        RuleFor(x => x.JwtBearer)
            .NotNull()
            .When(x => x.Type == AuthenticationSchemeType.JwtBearer)
            .WithMessage("JWT Bearer configuration is required when type is JwtBearer")
            .SetValidator(new JwtBearerConfigRequestValidator()!)
            .When(x => x.JwtBearer != null);

        RuleFor(x => x.OpenIdConnect)
            .NotNull()
            .When(x => x.Type == AuthenticationSchemeType.OpenIdConnect)
            .WithMessage("OpenID Connect configuration is required when type is OpenIdConnect")
            .SetValidator(new OpenIdConnectConfigRequestValidator()!)
            .When(x => x.OpenIdConnect != null);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description must not exceed 500 characters");
    }
}

