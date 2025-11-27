using FluentValidation;
using Sail.Route.Models;

namespace Sail.Route.Validators;

public class RouteHeaderRequestValidator : AbstractValidator<RouteHeaderRequest>
{
    public RouteHeaderRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Header name is required")
            .MaximumLength(100)
            .WithMessage("Header name must not exceed 100 characters");

        RuleFor(x => x.Values)
            .NotNull()
            .WithMessage("Header values are required")
            .Must(values => values.Any())
            .WithMessage("At least one header value must be provided")
            .Must(values => values.All(v => !string.IsNullOrWhiteSpace(v)))
            .WithMessage("Header values must not be empty");

        RuleFor(x => x.Mode)
            .IsInEnum()
            .WithMessage("Invalid header match mode");
    }
}

