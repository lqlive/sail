using FluentValidation;
using Sail.Models.Clusters;

namespace Sail.Validators.Clusters;

public class PassiveHealthCheckRequestValidator : AbstractValidator<PassiveHealthCheckRequest>
{
    public PassiveHealthCheckRequestValidator()
    {
        RuleFor(x => x.ReactivationPeriod)
            .GreaterThan(TimeSpan.Zero)
            .When(x => x.ReactivationPeriod.HasValue)
            .WithMessage("Passive health check reactivation period must be greater than zero");
    }
}

