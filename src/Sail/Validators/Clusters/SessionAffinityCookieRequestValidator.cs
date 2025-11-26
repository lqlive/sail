using FluentValidation;
using Sail.Models.Clusters;

namespace Sail.Validators.Clusters;

public class SessionAffinityCookieRequestValidator : AbstractValidator<SessionAffinityCookieRequest>
{
    public SessionAffinityCookieRequestValidator()
    {
        RuleFor(x => x.SecurePolicy)
            .IsInEnum()
            .When(x => x.SecurePolicy.HasValue)
            .WithMessage("Invalid cookie secure policy");

        RuleFor(x => x.SameSite)
            .IsInEnum()
            .When(x => x.SameSite.HasValue)
            .WithMessage("Invalid cookie SameSite mode");

        RuleFor(x => x.Expiration)
            .GreaterThan(TimeSpan.Zero)
            .When(x => x.Expiration.HasValue)
            .WithMessage("Cookie expiration must be greater than zero");

        RuleFor(x => x.MaxAge)
            .GreaterThan(TimeSpan.Zero)
            .When(x => x.MaxAge.HasValue)
            .WithMessage("Cookie max age must be greater than zero");
    }
}

