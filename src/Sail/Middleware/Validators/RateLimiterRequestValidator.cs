using FluentValidation;
using Sail.Middleware.Models;

namespace Sail.Middleware.Validators;

public class RateLimiterRequestValidator : AbstractValidator<RateLimiterRequest>
{
    public RateLimiterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Rate limiter policy name is required")
            .MaximumLength(200)
            .WithMessage("Rate limiter policy name must not exceed 200 characters");

        RuleFor(x => x.PermitLimit)
            .GreaterThan(0)
            .WithMessage("Rate limiter permit limit must be greater than 0");

        RuleFor(x => x.Window)
            .GreaterThan(0)
            .WithMessage("Rate limiter window must be greater than 0 seconds");

        RuleFor(x => x.QueueLimit)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Rate limiter queue limit must be greater than or equal to 0");
    }
}

