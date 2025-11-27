using FluentValidation;
using Sail.Cluster.Models;

namespace Sail.Cluster.Validators;

public class ClusterRequestValidator : AbstractValidator<ClusterRequest>
{
    public ClusterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Cluster name is required")
            .MaximumLength(200)
            .WithMessage("Cluster name must not exceed 200 characters");

        RuleFor(x => x.ServiceName)
            .NotEmpty()
            .WithMessage("Service name is required")
            .MaximumLength(200)
            .WithMessage("Service name must not exceed 200 characters");

        RuleFor(x => x.ServiceDiscoveryType)
            .IsInEnum()
            .When(x => x.ServiceDiscoveryType.HasValue)
            .WithMessage("Invalid service discovery type");

        RuleFor(x => x.LoadBalancingPolicy)
            .NotEmpty()
            .WithMessage("Load balancing policy is required");

        RuleFor(x => x.HealthCheck)
            .SetValidator(new HealthCheckRequestValidator())
            .When(x => x.HealthCheck != null);

        RuleFor(x => x.SessionAffinity)
            .SetValidator(new SessionAffinityRequestValidator())
            .When(x => x.SessionAffinity != null);

        RuleFor(x => x.Destinations)
            .NotNull()
            .WithMessage("Destinations are required")
            .Must(destinations => destinations.Any())
            .WithMessage("At least one destination must be provided");

        RuleForEach(x => x.Destinations)
            .SetValidator(new DestinationRequestValidator());
    }
}

