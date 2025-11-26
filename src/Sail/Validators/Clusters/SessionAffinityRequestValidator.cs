using FluentValidation;
using Sail.Models.Clusters;

namespace Sail.Validators.Clusters;

public class SessionAffinityRequestValidator : AbstractValidator<SessionAffinityRequest>
{
    public SessionAffinityRequestValidator()
    {
        RuleFor(x => x.Policy)
            .NotEmpty()
            .When(x => x.Enabled)
            .WithMessage("Session affinity policy is required when enabled");

        RuleFor(x => x.Cookie)
            .SetValidator(new SessionAffinityCookieRequestValidator())
            .When(x => x.Cookie != null);
    }
}

