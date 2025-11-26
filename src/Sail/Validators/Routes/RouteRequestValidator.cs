using FluentValidation;
using Sail.Models.Routes;

namespace Sail.Validators.Routes;

public class RouteRequestValidator : AbstractValidator<RouteRequest>
{
    public RouteRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty();
    }
}
