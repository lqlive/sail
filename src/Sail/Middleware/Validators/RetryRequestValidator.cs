using FluentValidation;
using Sail.Middleware.Models;
using Sail.Middleware.Errors;

namespace Sail.Middleware.Validators;

public class RetryRequestValidator : AbstractValidator<RetryRequest>
{
    public RetryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(RetryErrors.NameRequired.Description)
            .WithErrorCode(RetryErrors.NameRequired.Code)
            .MaximumLength(200)
            .WithMessage(RetryErrors.NameTooLong.Description)
            .WithErrorCode(RetryErrors.NameTooLong.Code);

        RuleFor(x => x.MaxRetryAttempts)
            .GreaterThanOrEqualTo(0)
            .WithMessage(RetryErrors.MaxRetryAttemptsTooLow.Description)
            .WithErrorCode(RetryErrors.MaxRetryAttemptsTooLow.Code)
            .LessThanOrEqualTo(10)
            .WithMessage(RetryErrors.MaxRetryAttemptsTooHigh.Description)
            .WithErrorCode(RetryErrors.MaxRetryAttemptsTooHigh.Code);

        RuleFor(x => x.RetryStatusCodes)
            .NotNull()
            .WithMessage(RetryErrors.StatusCodesRequired.Description)
            .WithErrorCode(RetryErrors.StatusCodesRequired.Code)
            .Must(codes => codes.Length > 0)
            .WithMessage(RetryErrors.StatusCodesEmpty.Description)
            .WithErrorCode(RetryErrors.StatusCodesEmpty.Code)
            .Must(codes => codes.All(code => code >= 400 && code <= 599))
            .WithMessage(RetryErrors.StatusCodesInvalid.Description)
            .WithErrorCode(RetryErrors.StatusCodesInvalid.Code);

        RuleFor(x => x.RetryDelayMilliseconds)
            .GreaterThanOrEqualTo(0)
            .WithMessage(RetryErrors.DelayTooLow.Description)
            .WithErrorCode(RetryErrors.DelayTooLow.Code)
            .LessThanOrEqualTo(60000)
            .WithMessage(RetryErrors.DelayTooHigh.Description)
            .WithErrorCode(RetryErrors.DelayTooHigh.Code);
    }
}
