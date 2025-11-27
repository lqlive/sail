using FluentValidation;
using Sail.AuthenticationPolicy.Models;

namespace Sail.AuthenticationPolicy.Validators;

public class JwtBearerConfigRequestValidator : AbstractValidator<JwtBearerConfigRequest>
{
    public JwtBearerConfigRequestValidator()
    {
        RuleFor(x => x.Authority)
            .NotEmpty()
            .WithMessage("JWT Bearer authority is required")
            .Must(authority => Uri.TryCreate(authority, UriKind.Absolute, out _))
            .WithMessage("JWT Bearer authority must be a valid absolute URL");

        RuleFor(x => x.Audience)
            .NotEmpty()
            .WithMessage("JWT Bearer audience is required");

        RuleFor(x => x.ValidIssuers)
            .Must(issuers => issuers == null || issuers.All(i => !string.IsNullOrWhiteSpace(i)))
            .When(x => x.ValidIssuers != null && x.ValidIssuers.Any())
            .WithMessage("Valid issuers must not contain empty values");

        RuleFor(x => x.ValidAudiences)
            .Must(audiences => audiences == null || audiences.All(a => !string.IsNullOrWhiteSpace(a)))
            .When(x => x.ValidAudiences != null && x.ValidAudiences.Any())
            .WithMessage("Valid audiences must not contain empty values");

        RuleFor(x => x.ClockSkew)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ClockSkew.HasValue)
            .WithMessage("Clock skew must be greater than or equal to 0 seconds");
    }
}

