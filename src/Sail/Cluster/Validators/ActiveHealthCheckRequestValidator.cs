using FluentValidation;
using Sail.Cluster.Models;
using Sail.Cluster.Errors;

namespace Sail.Cluster.Validators;

public class ActiveHealthCheckRequestValidator : AbstractValidator<ActiveHealthCheckRequest>
{
    public ActiveHealthCheckRequestValidator()
    {
        RuleFor(x => x.Interval)
            .GreaterThan(TimeSpan.Zero)
            .When(x => x.Interval.HasValue)
            .WithMessage(ActiveHealthCheckErrors.IntervalInvalid.Description)
            .WithErrorCode(ActiveHealthCheckErrors.IntervalInvalid.Code);

        RuleFor(x => x.Timeout)
            .GreaterThan(TimeSpan.Zero)
            .When(x => x.Timeout.HasValue)
            .WithMessage(ActiveHealthCheckErrors.TimeoutInvalid.Description)
            .WithErrorCode(ActiveHealthCheckErrors.TimeoutInvalid.Code);

        RuleFor(x => x.Path)
            .Must(path => string.IsNullOrEmpty(path) || path.StartsWith('/'))
            .When(x => !string.IsNullOrEmpty(x.Path))
            .WithMessage(ActiveHealthCheckErrors.PathInvalid.Description)
            .WithErrorCode(ActiveHealthCheckErrors.PathInvalid.Code);
    }
}
