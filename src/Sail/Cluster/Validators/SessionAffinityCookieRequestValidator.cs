using FluentValidation;
using Sail.Cluster.Models;
using Sail.Cluster.Errors;

namespace Sail.Cluster.Validators;

public class SessionAffinityCookieRequestValidator : AbstractValidator<SessionAffinityCookieRequest>
{
    public SessionAffinityCookieRequestValidator()
    {
        RuleFor(x => x.SecurePolicy)
            .IsInEnum()
            .When(x => x.SecurePolicy.HasValue)
            .WithMessage(SessionAffinityCookieErrors.SecurePolicyInvalid.Description)
            .WithErrorCode(SessionAffinityCookieErrors.SecurePolicyInvalid.Code);

        RuleFor(x => x.SameSite)
            .IsInEnum()
            .When(x => x.SameSite.HasValue)
            .WithMessage(SessionAffinityCookieErrors.SameSiteInvalid.Description)
            .WithErrorCode(SessionAffinityCookieErrors.SameSiteInvalid.Code);

        RuleFor(x => x.Expiration)
            .GreaterThan(TimeSpan.Zero)
            .When(x => x.Expiration.HasValue)
            .WithMessage(SessionAffinityCookieErrors.ExpirationInvalid.Description)
            .WithErrorCode(SessionAffinityCookieErrors.ExpirationInvalid.Code);

        RuleFor(x => x.MaxAge)
            .GreaterThan(TimeSpan.Zero)
            .When(x => x.MaxAge.HasValue)
            .WithMessage(SessionAffinityCookieErrors.MaxAgeInvalid.Description)
            .WithErrorCode(SessionAffinityCookieErrors.MaxAgeInvalid.Code);
    }
}
