using FluentValidation;
using Sail.Middleware.Models;
using Sail.Middleware.Errors;

namespace Sail.Middleware.Validators;

public class RateLimiterRequestValidator : AbstractValidator<RateLimiterRequest>
{
    public RateLimiterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(RateLimiterErrors.NameRequired.Description)
            .WithErrorCode(RateLimiterErrors.NameRequired.Code)
            .MaximumLength(200)
            .WithMessage(RateLimiterErrors.NameTooLong.Description)
            .WithErrorCode(RateLimiterErrors.NameTooLong.Code);

        RuleFor(x => x.PermitLimit)
            .GreaterThan(0)
            .WithMessage(RateLimiterErrors.PermitLimitInvalid.Description)
            .WithErrorCode(RateLimiterErrors.PermitLimitInvalid.Code);

        RuleFor(x => x.Window)
            .GreaterThan(0)
            .WithMessage(RateLimiterErrors.WindowInvalid.Description)
            .WithErrorCode(RateLimiterErrors.WindowInvalid.Code);

        RuleFor(x => x.QueueLimit)
            .GreaterThanOrEqualTo(0)
            .WithMessage(RateLimiterErrors.QueueLimitInvalid.Description)
            .WithErrorCode(RateLimiterErrors.QueueLimitInvalid.Code);
    }
}
