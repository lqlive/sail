using FluentValidation;
using Sail.Middleware.Models;
using Sail.Middleware.Errors;

namespace Sail.Middleware.Validators;

public class TimeoutRequestValidator : AbstractValidator<TimeoutRequest>
{
    public TimeoutRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(TimeoutErrors.NameRequired.Description)
            .WithErrorCode(TimeoutErrors.NameRequired.Code)
            .MaximumLength(200)
            .WithMessage(TimeoutErrors.NameTooLong.Description)
            .WithErrorCode(TimeoutErrors.NameTooLong.Code);

        RuleFor(x => x.Seconds)
            .GreaterThan(0)
            .WithMessage(TimeoutErrors.SecondsInvalid.Description)
            .WithErrorCode(TimeoutErrors.SecondsInvalid.Code);

        RuleFor(x => x.TimeoutStatusCode)
            .InclusiveBetween(400, 599)
            .When(x => x.TimeoutStatusCode.HasValue)
            .WithMessage(TimeoutErrors.StatusCodeInvalid.Description)
            .WithErrorCode(TimeoutErrors.StatusCodeInvalid.Code);
    }
}
