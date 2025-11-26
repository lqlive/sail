using FluentValidation;
using Sail.Models.Certificates;

namespace Sail.Validators.Certificates;

public class CertificateRequestValidator : AbstractValidator<CertificateRequest>
{
    public CertificateRequestValidator()
    {
        RuleFor(x => x.Cert)
            .NotEmpty()
            .WithMessage("Certificate content is required")
            .Must(cert => cert.Contains("BEGIN CERTIFICATE") && cert.Contains("END CERTIFICATE"))
            .WithMessage("Certificate must be in valid PEM format (BEGIN CERTIFICATE...END CERTIFICATE)");

        RuleFor(x => x.Key)
            .NotEmpty()
            .WithMessage("Private key is required")
            .Must(key => (key.Contains("BEGIN PRIVATE KEY") && key.Contains("END PRIVATE KEY")) ||
                         (key.Contains("BEGIN RSA PRIVATE KEY") && key.Contains("END RSA PRIVATE KEY")) ||
                         (key.Contains("BEGIN EC PRIVATE KEY") && key.Contains("END EC PRIVATE KEY")))
            .WithMessage("Private key must be in valid PEM format (BEGIN PRIVATE KEY...END PRIVATE KEY)");

        RuleForEach(x => x.SNIs)
            .SetValidator(new SNIRequestValidator())
            .When(x => x.SNIs != null && x.SNIs.Any());
    }
}

