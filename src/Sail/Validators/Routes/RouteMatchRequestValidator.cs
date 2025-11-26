using FluentValidation;
using Sail.Models.Routes;

namespace Sail.Validators.Routes;

public class RouteMatchRequestValidator : AbstractValidator<RouteMatchRequest>
{
    public RouteMatchRequestValidator()
    {
        RuleFor(x => x.Path)
            .NotEmpty()
            .WithMessage("Route path is required")
            .Must(path => path.StartsWith('/'))
            .WithMessage("Route path must start with '/'");

        RuleFor(x => x.Methods)
            .Must(methods => methods == null || methods.All(m => !string.IsNullOrWhiteSpace(m)))
            .When(x => x.Methods != null && x.Methods.Any())
            .WithMessage("HTTP methods must not be empty");

        RuleFor(x => x.Hosts)
            .Must(hosts => hosts == null || hosts.All(h => !string.IsNullOrWhiteSpace(h)))
            .When(x => x.Hosts != null && x.Hosts.Any())
            .WithMessage("Host names must not be empty");

        RuleForEach(x => x.QueryParameters)
            .SetValidator(new QueryParameterRequestValidator())
            .When(x => x.QueryParameters != null && x.QueryParameters.Any());

        RuleForEach(x => x.Headers)
            .SetValidator(new RouteHeaderRequestValidator())
            .When(x => x.Headers != null && x.Headers.Any());
    }
}

