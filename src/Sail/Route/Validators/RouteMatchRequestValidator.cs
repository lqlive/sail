using FluentValidation;
using Sail.Route.Models;
using Sail.Route.Errors;

namespace Sail.Route.Validators;

public class RouteMatchRequestValidator : AbstractValidator<RouteMatchRequest>
{
    public RouteMatchRequestValidator()
    {
        RuleFor(x => x.Path)
            .NotEmpty()
            .WithMessage(RouteMatchErrors.PathRequired.Description)
            .WithErrorCode(RouteMatchErrors.PathRequired.Code)
            .Must(path => path.StartsWith('/') || path.StartsWith('{'))
            .WithMessage(RouteMatchErrors.PathInvalid.Description)
            .WithErrorCode(RouteMatchErrors.PathInvalid.Code);

        RuleFor(x => x.Methods)
            .Must(methods => methods == null || methods.All(m => !string.IsNullOrWhiteSpace(m)))
            .When(x => x.Methods != null && x.Methods.Any())
            .WithMessage(RouteMatchErrors.MethodsContainEmpty.Description)
            .WithErrorCode(RouteMatchErrors.MethodsContainEmpty.Code);

        RuleFor(x => x.Hosts)
            .Must(hosts => hosts == null || hosts.All(h => !string.IsNullOrWhiteSpace(h)))
            .When(x => x.Hosts != null && x.Hosts.Any())
            .WithMessage(RouteMatchErrors.HostsContainEmpty.Description)
            .WithErrorCode(RouteMatchErrors.HostsContainEmpty.Code);

        RuleForEach(x => x.QueryParameters)
            .SetValidator(new QueryParameterRequestValidator())
            .When(x => x.QueryParameters != null && x.QueryParameters.Any());

        RuleForEach(x => x.Headers)
            .SetValidator(new RouteHeaderRequestValidator())
            .When(x => x.Headers != null && x.Headers.Any());
    }
}
