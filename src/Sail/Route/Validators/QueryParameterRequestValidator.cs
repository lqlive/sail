using FluentValidation;
using Sail.Route.Models;
using Sail.Route.Errors;

namespace Sail.Route.Validators;

public class QueryParameterRequestValidator : AbstractValidator<QueryParameterRequest>
{
    public QueryParameterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(QueryParameterErrors.NameRequired.Description)
            .WithErrorCode(QueryParameterErrors.NameRequired.Code)
            .MaximumLength(100)
            .WithMessage(QueryParameterErrors.NameTooLong.Description)
            .WithErrorCode(QueryParameterErrors.NameTooLong.Code);

        RuleFor(x => x.Values)
            .NotNull()
            .WithMessage(QueryParameterErrors.ValuesRequired.Description)
            .WithErrorCode(QueryParameterErrors.ValuesRequired.Code)
            .Must(values => values.Any())
            .WithMessage(QueryParameterErrors.ValuesEmpty.Description)
            .WithErrorCode(QueryParameterErrors.ValuesEmpty.Code)
            .Must(values => values.All(v => !string.IsNullOrWhiteSpace(v)))
            .WithMessage(QueryParameterErrors.ValuesContainEmpty.Description)
            .WithErrorCode(QueryParameterErrors.ValuesContainEmpty.Code);

        RuleFor(x => x.Mode)
            .IsInEnum()
            .WithMessage(QueryParameterErrors.ModeInvalid.Description)
            .WithErrorCode(QueryParameterErrors.ModeInvalid.Code);
    }
}
