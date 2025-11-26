using FluentValidation;
using Sail.Core.Entities;
using Sail.Models.Middlewares;

namespace Sail.Validators.Middlewares;

public class MiddlewareRequestValidator : AbstractValidator<MiddlewareRequest>
{
    public MiddlewareRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Middleware name is required")
            .MaximumLength(200)
            .WithMessage("Middleware name must not exceed 200 characters");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid middleware type");

        RuleFor(x => x.Cors)
            .NotNull()
            .When(x => x.Type == MiddlewareType.Cors)
            .WithMessage("CORS configuration is required when type is CORS")
            .SetValidator(new CorsRequestValidator()!)
            .When(x => x.Cors != null);

        RuleFor(x => x.RateLimiter)
            .NotNull()
            .When(x => x.Type == MiddlewareType.RateLimiter)
            .WithMessage("RateLimiter configuration is required when type is RateLimiter")
            .SetValidator(new RateLimiterRequestValidator()!)
            .When(x => x.RateLimiter != null);

        RuleFor(x => x.Timeout)
            .NotNull()
            .When(x => x.Type == MiddlewareType.Timeout)
            .WithMessage("Timeout configuration is required when type is Timeout")
            .SetValidator(new TimeoutRequestValidator()!)
            .When(x => x.Timeout != null);

        RuleFor(x => x.Retry)
            .NotNull()
            .When(x => x.Type == MiddlewareType.Retry)
            .WithMessage("Retry configuration is required when type is Retry")
            .SetValidator(new RetryRequestValidator()!)
            .When(x => x.Retry != null);
    }
}

