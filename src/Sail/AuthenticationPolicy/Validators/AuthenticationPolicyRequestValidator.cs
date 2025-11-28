using FluentValidation;
using Sail.Core.Entities;
using Sail.AuthenticationPolicy.Models;
using Sail.AuthenticationPolicy.Errors;

namespace Sail.AuthenticationPolicy.Validators;

public class AuthenticationPolicyRequestValidator : AbstractValidator<AuthenticationPolicyRequest>
{
    public AuthenticationPolicyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(AuthenticationPolicyErrors.NameRequired.Description)
            .WithErrorCode(AuthenticationPolicyErrors.NameRequired.Code)
            .MaximumLength(200)
            .WithMessage(AuthenticationPolicyErrors.NameTooLong.Description)
            .WithErrorCode(AuthenticationPolicyErrors.NameTooLong.Code);

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage(AuthenticationPolicyErrors.TypeInvalid.Description)
            .WithErrorCode(AuthenticationPolicyErrors.TypeInvalid.Code);

        RuleFor(x => x.JwtBearer)
            .NotNull()
            .When(x => x.Type == AuthenticationSchemeType.JwtBearer)
            .WithMessage(AuthenticationPolicyErrors.JwtBearerConfigRequired.Description)
            .WithErrorCode(AuthenticationPolicyErrors.JwtBearerConfigRequired.Code)
            .SetValidator(new JwtBearerConfigRequestValidator()!)
            .When(x => x.JwtBearer != null);

        RuleFor(x => x.OpenIdConnect)
            .NotNull()
            .When(x => x.Type == AuthenticationSchemeType.OpenIdConnect)
            .WithMessage(AuthenticationPolicyErrors.OpenIdConnectConfigRequired.Description)
            .WithErrorCode(AuthenticationPolicyErrors.OpenIdConnectConfigRequired.Code)
            .SetValidator(new OpenIdConnectConfigRequestValidator()!)
            .When(x => x.OpenIdConnect != null);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage(AuthenticationPolicyErrors.DescriptionTooLong.Description)
            .WithErrorCode(AuthenticationPolicyErrors.DescriptionTooLong.Code);
    }
}
