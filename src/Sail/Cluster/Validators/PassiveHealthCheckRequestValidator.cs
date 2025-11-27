using FluentValidation;
using Sail.Cluster.Models;

namespace Sail.Cluster.Validators;

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

