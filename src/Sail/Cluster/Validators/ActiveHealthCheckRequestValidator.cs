using FluentValidation;
using Sail.Cluster.Models;

namespace Sail.Cluster.Validators;

public class ActiveHealthCheckRequestValidator : AbstractValidator<ActiveHealthCheckRequest>
{
    public ActiveHealthCheckRequestValidator()
    {
        RuleFor(x => x.Interval)
            .GreaterThan(TimeSpan.Zero)
            .When(x => x.Interval.HasValue)
            .WithMessage("Active health check interval must be greater than zero");

        RuleFor(x => x.Timeout)
            .GreaterThan(TimeSpan.Zero)
            .When(x => x.Timeout.HasValue)
            .WithMessage("Active health check timeout must be greater than zero");

        RuleFor(x => x.Path)
            .Must(path => string.IsNullOrEmpty(path) || path.StartsWith('/'))
            .When(x => !string.IsNullOrEmpty(x.Path))
            .WithMessage("Active health check path must start with '/'");
    }
}

