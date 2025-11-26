using FluentValidation;
using Sail.Models.Routes;

namespace Sail.Validators.Routes;

public class QueryParameterRequestValidator : AbstractValidator<QueryParameterRequest>
{
    public QueryParameterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Query parameter name is required")
            .MaximumLength(100)
            .WithMessage("Query parameter name must not exceed 100 characters");

        RuleFor(x => x.Values)
            .NotNull()
            .WithMessage("Query parameter values are required")
            .Must(values => values.Any())
            .WithMessage("At least one query parameter value must be provided")
            .Must(values => values.All(v => !string.IsNullOrWhiteSpace(v)))
            .WithMessage("Query parameter values must not be empty");

        RuleFor(x => x.Mode)
            .IsInEnum()
            .WithMessage("Invalid query parameter match mode");
    }
}

