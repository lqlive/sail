using FluentValidation;
using Sail.Route.Models;
using Sail.Route.Errors;

namespace Sail.Route.Validators;

public class RouteHeaderRequestValidator : AbstractValidator<RouteHeaderRequest>
{
    public RouteHeaderRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(RouteHeaderErrors.NameRequired.Description)
            .WithErrorCode(RouteHeaderErrors.NameRequired.Code)
            .MaximumLength(100)
            .WithMessage(RouteHeaderErrors.NameTooLong.Description)
            .WithErrorCode(RouteHeaderErrors.NameTooLong.Code);

        RuleFor(x => x.Values)
            .Must(values => values == null || values.All(v => !string.IsNullOrWhiteSpace(v)))
            .WithMessage(RouteHeaderErrors.ValuesContainEmpty.Description)
            .WithErrorCode(RouteHeaderErrors.ValuesContainEmpty.Code);

        RuleFor(x => x.Mode)
            .IsInEnum()
            .WithMessage(RouteHeaderErrors.ModeInvalid.Description)
            .WithErrorCode(RouteHeaderErrors.ModeInvalid.Code);
    }
}
