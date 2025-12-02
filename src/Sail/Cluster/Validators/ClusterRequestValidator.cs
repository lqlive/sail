using FluentValidation;
using Sail.Cluster.Models;
using Sail.Cluster.Errors;

namespace Sail.Cluster.Validators;

public class ClusterRequestValidator : AbstractValidator<ClusterRequest>
{
    public ClusterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ClusterErrors.NameRequired.Description)
            .WithErrorCode(ClusterErrors.NameRequired.Code)
            .MaximumLength(200)
            .WithMessage(ClusterErrors.NameTooLong.Description)
            .WithErrorCode(ClusterErrors.NameTooLong.Code);

        RuleFor(x => x.ServiceName)
            .NotEmpty()
            .WithMessage(ClusterErrors.ServiceNameRequired.Description)
            .WithErrorCode(ClusterErrors.ServiceNameRequired.Code)
            .When(x => x.ServiceDiscoveryType.HasValue);

        RuleFor(x => x.ServiceName)
            .MaximumLength(200)
            .WithMessage(ClusterErrors.ServiceNameTooLong.Description)
            .WithErrorCode(ClusterErrors.ServiceNameTooLong.Code)
            .When(x => !string.IsNullOrEmpty(x.ServiceName));

        RuleFor(x => x.ServiceDiscoveryType)
            .IsInEnum()
            .When(x => x.ServiceDiscoveryType.HasValue)
            .WithMessage(ClusterErrors.ServiceDiscoveryTypeInvalid.Description)
            .WithErrorCode(ClusterErrors.ServiceDiscoveryTypeInvalid.Code);

        RuleFor(x => x.LoadBalancingPolicy)
            .NotEmpty()
            .WithMessage(ClusterErrors.LoadBalancingPolicyRequired.Description)
            .WithErrorCode(ClusterErrors.LoadBalancingPolicyRequired.Code);

        RuleFor(x => x.HealthCheck)
            .SetValidator(new HealthCheckRequestValidator())
            .When(x => x.HealthCheck != null);

        RuleFor(x => x.SessionAffinity)
            .SetValidator(new SessionAffinityRequestValidator())
            .When(x => x.SessionAffinity != null);

        RuleFor(x => x.Destinations)
            .NotNull()
            .WithMessage(ClusterErrors.DestinationsRequired.Description)
            .WithErrorCode(ClusterErrors.DestinationsRequired.Code)
            .Must(destinations => destinations.Any())
            .WithMessage(ClusterErrors.DestinationsEmpty.Description)
            .WithErrorCode(ClusterErrors.DestinationsEmpty.Code);

        RuleForEach(x => x.Destinations)
            .SetValidator(new DestinationRequestValidator());
    }
}
