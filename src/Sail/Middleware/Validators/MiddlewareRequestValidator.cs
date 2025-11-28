using FluentValidation;
using Sail.Core.Entities;
using Sail.Middleware.Models;
using Sail.Middleware.Errors;

namespace Sail.Middleware.Validators;

public class MiddlewareRequestValidator : AbstractValidator<MiddlewareRequest>
{
    public MiddlewareRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(MiddlewareErrors.NameRequired.Description)
            .WithErrorCode(MiddlewareErrors.NameRequired.Code)
            .MaximumLength(200)
            .WithMessage(MiddlewareErrors.NameTooLong.Description)
            .WithErrorCode(MiddlewareErrors.NameTooLong.Code);

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage(MiddlewareErrors.InvalidType.Description)
            .WithErrorCode(MiddlewareErrors.InvalidType.Code);

        RuleFor(x => x.Cors)
            .NotNull()
            .When(x => x.Type == MiddlewareType.Cors)
            .WithMessage(MiddlewareErrors.CorsConfigRequired.Description)
            .WithErrorCode(MiddlewareErrors.CorsConfigRequired.Code)
            .SetValidator(new CorsRequestValidator()!)
            .When(x => x.Cors != null);

        RuleFor(x => x.RateLimiter)
            .NotNull()
            .When(x => x.Type == MiddlewareType.RateLimiter)
            .WithMessage(MiddlewareErrors.RateLimiterConfigRequired.Description)
            .WithErrorCode(MiddlewareErrors.RateLimiterConfigRequired.Code)
            .SetValidator(new RateLimiterRequestValidator()!)
            .When(x => x.RateLimiter != null);

        RuleFor(x => x.Timeout)
            .NotNull()
            .When(x => x.Type == MiddlewareType.Timeout)
            .WithMessage(MiddlewareErrors.TimeoutConfigRequired.Description)
            .WithErrorCode(MiddlewareErrors.TimeoutConfigRequired.Code)
            .SetValidator(new TimeoutRequestValidator()!)
            .When(x => x.Timeout != null);

        RuleFor(x => x.Retry)
            .NotNull()
            .When(x => x.Type == MiddlewareType.Retry)
            .WithMessage(MiddlewareErrors.RetryConfigRequired.Description)
            .WithErrorCode(MiddlewareErrors.RetryConfigRequired.Code)
            .SetValidator(new RetryRequestValidator()!)
            .When(x => x.Retry != null);
    }
}
