using FluentValidation;
using Sail.Cluster.Models;
using Sail.Cluster.Errors;

namespace Sail.Cluster.Validators;

public class SessionAffinityRequestValidator : AbstractValidator<SessionAffinityRequest>
{
    public SessionAffinityRequestValidator()
    {
        RuleFor(x => x.Policy)
            .NotEmpty()
            .When(x => x.Enabled)
            .WithMessage(SessionAffinityErrors.PolicyRequired.Description)
            .WithErrorCode(SessionAffinityErrors.PolicyRequired.Code);

        RuleFor(x => x.Cookie)
            .SetValidator(new SessionAffinityCookieRequestValidator())
            .When(x => x.Cookie != null);
    }
}
