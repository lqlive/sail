using FluentValidation;
using Sail.Route.Models;
using Sail.Route.Errors;

namespace Sail.Route.Validators;

public class RouteRequestValidator : AbstractValidator<RouteRequest>
{
    public RouteRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(RouteErrors.NameRequired.Description)
            .WithErrorCode(RouteErrors.NameRequired.Code)
            .MaximumLength(200)
            .WithMessage(RouteErrors.NameTooLong.Description)
            .WithErrorCode(RouteErrors.NameTooLong.Code);

        RuleFor(x => x.Match)
            .NotNull()
            .WithMessage(RouteErrors.MatchRequired.Description)
            .WithErrorCode(RouteErrors.MatchRequired.Code)
            .SetValidator(new RouteMatchRequestValidator());

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0)
            .WithMessage(RouteErrors.OrderInvalid.Description)
            .WithErrorCode(RouteErrors.OrderInvalid.Code);

        RuleFor(x => x.ClusterId)
            .NotEmpty()
            .When(x => x.ClusterId.HasValue)
            .WithMessage(RouteErrors.ClusterIdInvalid.Description)
            .WithErrorCode(RouteErrors.ClusterIdInvalid.Code);

        RuleFor(x => x.Timeout)
            .GreaterThan(TimeSpan.Zero)
            .When(x => x.Timeout.HasValue)
            .WithMessage(RouteErrors.TimeoutInvalid.Description)
            .WithErrorCode(RouteErrors.TimeoutInvalid.Code);

        RuleFor(x => x.MaxRequestBodySize)
            .GreaterThan(0)
            .When(x => x.MaxRequestBodySize.HasValue)
            .WithMessage(RouteErrors.MaxRequestBodySizeInvalid.Description)
            .WithErrorCode(RouteErrors.MaxRequestBodySizeInvalid.Code);

        RuleFor(x => x.Transforms)
            .Must(transforms => transforms == null || transforms.All(t => t != null && t.Any()))
            .When(x => x.Transforms != null)
            .WithMessage(RouteErrors.TransformsInvalid.Description)
            .WithErrorCode(RouteErrors.TransformsInvalid.Code);
    }
}
