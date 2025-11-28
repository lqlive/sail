using FluentValidation;
using Sail.Cluster.Models;
using Sail.Cluster.Errors;

namespace Sail.Cluster.Validators;

public class PassiveHealthCheckRequestValidator : AbstractValidator<PassiveHealthCheckRequest>
{
    public PassiveHealthCheckRequestValidator()
    {
        RuleFor(x => x.ReactivationPeriod)
            .GreaterThan(TimeSpan.Zero)
            .When(x => x.ReactivationPeriod.HasValue)
            .WithMessage(PassiveHealthCheckErrors.ReactivationPeriodInvalid.Description)
            .WithErrorCode(PassiveHealthCheckErrors.ReactivationPeriodInvalid.Code);
    }
}
