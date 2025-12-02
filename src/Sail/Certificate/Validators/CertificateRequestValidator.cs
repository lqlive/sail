using FluentValidation;
using Sail.Certificate.Models;
using Sail.Certificate.Errors;

namespace Sail.Certificate.Validators;

public class CertificateRequestValidator : AbstractValidator<CertificateRequest>
{
    public CertificateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Certificate name is required");

        RuleFor(x => x.Cert)
            .NotEmpty()
            .WithMessage(CertificateErrors.CertRequired.Description)
            .WithErrorCode(CertificateErrors.CertRequired.Code)
            .Must(cert => cert.Contains("BEGIN CERTIFICATE") && cert.Contains("END CERTIFICATE"))
            .WithMessage(CertificateErrors.CertInvalidFormat.Description)
            .WithErrorCode(CertificateErrors.CertInvalidFormat.Code);

        RuleFor(x => x.Key)
            .NotEmpty()
            .WithMessage(CertificateErrors.KeyRequired.Description)
            .WithErrorCode(CertificateErrors.KeyRequired.Code)
            .Must(key => (key.Contains("BEGIN PRIVATE KEY") && key.Contains("END PRIVATE KEY")) ||
                         (key.Contains("BEGIN RSA PRIVATE KEY") && key.Contains("END RSA PRIVATE KEY")) ||
                         (key.Contains("BEGIN EC PRIVATE KEY") && key.Contains("END EC PRIVATE KEY")))
            .WithMessage(CertificateErrors.KeyInvalidFormat.Description)
            .WithErrorCode(CertificateErrors.KeyInvalidFormat.Code);

        RuleForEach(x => x.SNIs)
            .SetValidator(new SNIRequestValidator())
            .When(x => x.SNIs != null && x.SNIs.Any());
    }
}
