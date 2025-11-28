using FluentValidation;
using Sail.Cluster.Models;
using Sail.Cluster.Errors;

namespace Sail.Cluster.Validators;

public class DestinationRequestValidator : AbstractValidator<DestinationRequest>
{
    public DestinationRequestValidator()
    {
        RuleFor(x => x.Host)
            .NotEmpty()
            .WithMessage(DestinationErrors.HostRequired.Description)
            .WithErrorCode(DestinationErrors.HostRequired.Code)
            .MaximumLength(255)
            .WithMessage(DestinationErrors.HostTooLong.Description)
            .WithErrorCode(DestinationErrors.HostTooLong.Code);

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage(DestinationErrors.AddressRequired.Description)
            .WithErrorCode(DestinationErrors.AddressRequired.Code)
            .Must(address => Uri.TryCreate(address, UriKind.Absolute, out _))
            .WithMessage(DestinationErrors.AddressInvalid.Description)
            .WithErrorCode(DestinationErrors.AddressInvalid.Code);

        RuleFor(x => x.Health)
            .NotEmpty()
            .WithMessage(DestinationErrors.HealthRequired.Description)
            .WithErrorCode(DestinationErrors.HealthRequired.Code);
    }
}
