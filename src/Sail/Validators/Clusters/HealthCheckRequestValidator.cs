using FluentValidation;
using Sail.Models.Clusters;

namespace Sail.Validators.Clusters;

public class HealthCheckRequestValidator : AbstractValidator<HealthCheckRequest>
{
    public HealthCheckRequestValidator()
    {
        RuleFor(x => x.Active)
            .SetValidator(new ActiveHealthCheckRequestValidator())
            .When(x => x.Active != null);

        RuleFor(x => x.Passive)
            .SetValidator(new PassiveHealthCheckRequestValidator())
            .When(x => x.Passive != null);
    }
}

