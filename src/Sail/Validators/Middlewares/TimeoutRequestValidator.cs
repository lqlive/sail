using FluentValidation;
using Sail.Models.Middlewares;

namespace Sail.Validators.Middlewares;

public class TimeoutRequestValidator : AbstractValidator<TimeoutRequest>
{
    public TimeoutRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Timeout policy name is required")
            .MaximumLength(200)
            .WithMessage("Timeout policy name must not exceed 200 characters");

        RuleFor(x => x.Seconds)
            .GreaterThan(0)
            .WithMessage("Timeout seconds must be greater than 0");

        RuleFor(x => x.TimeoutStatusCode)
            .InclusiveBetween(400, 599)
            .When(x => x.TimeoutStatusCode.HasValue)
            .WithMessage("Timeout status code must be between 400 and 599");
    }
}

