using FluentValidation;
using Sail.AuthenticationPolicy.Models;
using Sail.AuthenticationPolicy.Errors;

namespace Sail.AuthenticationPolicy.Validators;

public class JwtBearerConfigRequestValidator : AbstractValidator<JwtBearerConfigRequest>
{
    public JwtBearerConfigRequestValidator()
    {
        RuleFor(x => x.Authority)
            .NotEmpty()
            .WithMessage(JwtBearerErrors.AuthorityRequired.Description)
            .WithErrorCode(JwtBearerErrors.AuthorityRequired.Code)
            .Must(authority => Uri.TryCreate(authority, UriKind.Absolute, out _))
            .WithMessage(JwtBearerErrors.AuthorityInvalid.Description)
            .WithErrorCode(JwtBearerErrors.AuthorityInvalid.Code);

        RuleFor(x => x.Audience)
            .NotEmpty()
            .WithMessage(JwtBearerErrors.AudienceRequired.Description)
            .WithErrorCode(JwtBearerErrors.AudienceRequired.Code);

        RuleFor(x => x.ValidIssuers)
            .Must(issuers => issuers == null || issuers.All(i => !string.IsNullOrWhiteSpace(i)))
            .When(x => x.ValidIssuers != null && x.ValidIssuers.Any())
            .WithMessage(JwtBearerErrors.ValidIssuersContainEmpty.Description)
            .WithErrorCode(JwtBearerErrors.ValidIssuersContainEmpty.Code);

        RuleFor(x => x.ValidAudiences)
            .Must(audiences => audiences == null || audiences.All(a => !string.IsNullOrWhiteSpace(a)))
            .When(x => x.ValidAudiences != null && x.ValidAudiences.Any())
            .WithMessage(JwtBearerErrors.ValidAudiencesContainEmpty.Description)
            .WithErrorCode(JwtBearerErrors.ValidAudiencesContainEmpty.Code);

        RuleFor(x => x.ClockSkew)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ClockSkew.HasValue)
            .WithMessage(JwtBearerErrors.ClockSkewInvalid.Description)
            .WithErrorCode(JwtBearerErrors.ClockSkewInvalid.Code);
    }
}
