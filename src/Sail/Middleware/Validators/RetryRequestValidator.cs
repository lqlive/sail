using FluentValidation;
using Sail.Middleware.Models;

namespace Sail.Middleware.Validators;

public class RetryRequestValidator : AbstractValidator<RetryRequest>
{
    public RetryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Retry policy name is required")
            .MaximumLength(200)
            .WithMessage("Retry policy name must not exceed 200 characters");

        RuleFor(x => x.MaxRetryAttempts)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Max retry attempts must be greater than or equal to 0")
            .LessThanOrEqualTo(10)
            .WithMessage("Max retry attempts should not exceed 10 for safety");

        RuleFor(x => x.RetryStatusCodes)
            .NotNull()
            .WithMessage("Retry status codes are required")
            .Must(codes => codes.Length > 0)
            .WithMessage("At least one retry status code must be provided")
            .Must(codes => codes.All(code => code >= 400 && code <= 599))
            .WithMessage("Retry status codes must be between 400 and 599");

        RuleFor(x => x.RetryDelayMilliseconds)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Retry delay must be greater than or equal to 0 milliseconds")
            .LessThanOrEqualTo(60000)
            .WithMessage("Retry delay should not exceed 60000 milliseconds (60 seconds)");
    }
}

