using FluentValidation;
using Sail.Certificate.Models;
using Sail.Certificate.Errors;
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
            .WithMessage(SNIErrors.NameRequired.Description)
            .WithErrorCode(SNIErrors.NameRequired.Code)
            .MaximumLength(200)
            .WithMessage(SNIErrors.NameTooLong.Description)
            .WithErrorCode(SNIErrors.NameTooLong.Code);

        RuleFor(x => x.HostName)
            .NotEmpty()
            .WithMessage(SNIErrors.HostNameRequired.Description)
            .WithErrorCode(SNIErrors.HostNameRequired.Code)
            .MaximumLength(255)
            .WithMessage(SNIErrors.HostNameTooLong.Description)
            .WithErrorCode(SNIErrors.HostNameTooLong.Code)
            .Must(hostname => HostnameRegex.IsMatch(hostname))
            .WithMessage(SNIErrors.HostNameInvalid.Description)
            .WithErrorCode(SNIErrors.HostNameInvalid.Code);
    }
}
