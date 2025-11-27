using FluentValidation;
using Sail.Certificate.Models;
using System.Text.RegularExpressions;

namespace Sail.Certificate.Validators;

public class SNIRequestValidator : AbstractValidator<SNIRequest>
{
    private static readonly Regex HostnameRegex = new(
        @"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$|^\*\.([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])$",
        RegexOptions.Compiled);

    public SNIRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("SNI name is required")
            .MaximumLength(200)
            .WithMessage("SNI name must not exceed 200 characters");

        RuleFor(x => x.HostName)
            .NotEmpty()
            .WithMessage("SNI hostname is required")
            .MaximumLength(255)
            .WithMessage("Hostname must not exceed 255 characters")
            .Must(hostname => HostnameRegex.IsMatch(hostname))
            .WithMessage("Hostname must be a valid domain name or wildcard domain (e.g., example.com or *.example.com)");
    }
}

