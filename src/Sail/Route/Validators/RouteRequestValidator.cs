using FluentValidation;
using Sail.Route.Models;

namespace Sail.Route.Validators;

public class RouteRequestValidator : AbstractValidator<RouteRequest>
{
    public RouteRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Route name is required")
            .MaximumLength(200)
            .WithMessage("Route name must not exceed 200 characters");

        RuleFor(x => x.Match)
            .NotNull()
            .WithMessage("Route match configuration is required")
            .SetValidator(new RouteMatchRequestValidator());

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Route order must be a non-negative number");

        RuleFor(x => x.ClusterId)
            .NotEmpty()
            .When(x => x.ClusterId.HasValue)
            .WithMessage("ClusterId must be a valid GUID when provided");

        RuleFor(x => x.Timeout)
            .GreaterThan(TimeSpan.Zero)
            .When(x => x.Timeout.HasValue)
            .WithMessage("Timeout must be greater than zero when specified");

        RuleFor(x => x.MaxRequestBodySize)
            .GreaterThan(0)
            .When(x => x.MaxRequestBodySize.HasValue)
            .WithMessage("MaxRequestBodySize must be greater than zero when specified");

        RuleFor(x => x.Transforms)
            .Must(transforms => transforms == null || transforms.All(t => t != null && t.Any()))
            .When(x => x.Transforms != null)
            .WithMessage("Each transform must contain at least one key-value pair");
    }
}