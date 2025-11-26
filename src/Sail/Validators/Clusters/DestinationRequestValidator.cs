using FluentValidation;
using Sail.Models.Clusters;

namespace Sail.Validators.Clusters;

public class DestinationRequestValidator : AbstractValidator<DestinationRequest>
{
    public DestinationRequestValidator()
    {
        RuleFor(x => x.Host)
            .NotEmpty()
            .WithMessage("Destination host is required")
            .MaximumLength(255)
            .WithMessage("Destination host must not exceed 255 characters");

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage("Destination address is required")
            .Must(address => Uri.TryCreate(address, UriKind.Absolute, out _))
            .WithMessage("Destination address must be a valid absolute URL");

        RuleFor(x => x.Health)
            .NotEmpty()
            .WithMessage("Destination health is required");
    }
}

