using FluentValidation;
using Sail.Cluster.Models;
using Sail.Cluster.Errors;

namespace Sail.Cluster.Validators;

public class DestinationRequestValidator : AbstractValidator<DestinationRequest>
{
    public DestinationRequestValidator()
    {
        RuleFor(x => x.Host)
            .MaximumLength(255)
            .WithMessage(DestinationErrors.HostTooLong.Description)
            .WithErrorCode(DestinationErrors.HostTooLong.Code)
            .When(x => !string.IsNullOrEmpty(x.Host));

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage(DestinationErrors.AddressRequired.Description)
            .WithErrorCode(DestinationErrors.AddressRequired.Code)
            .Must(address => Uri.TryCreate(address, UriKind.Absolute, out _))
            .WithMessage(DestinationErrors.AddressInvalid.Description)
            .WithErrorCode(DestinationErrors.AddressInvalid.Code);

    }
}
