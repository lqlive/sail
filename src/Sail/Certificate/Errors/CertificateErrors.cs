using ErrorOr;

namespace Sail.Certificate.Errors;

public static class CertificateErrors
{
    public static Error CertRequired => Error.Validation(
        code: "Certificate.CertRequired",
        description: "Certificate content is required");

    public static Error CertInvalidFormat => Error.Validation(
        code: "Certificate.CertInvalidFormat",
        description: "Certificate must be in valid PEM format (BEGIN CERTIFICATE...END CERTIFICATE)");

    public static Error KeyRequired => Error.Validation(
        code: "Certificate.KeyRequired",
        description: "Private key is required");

    public static Error KeyInvalidFormat => Error.Validation(
        code: "Certificate.KeyInvalidFormat",
        description: "Private key must be in valid PEM format (BEGIN PRIVATE KEY...END PRIVATE KEY)");

    public static Error CertificateNotFound => Error.NotFound(
        code: "Certificate.NotFound",
        description: "Certificate not found");

    public static Error SNINotFound => Error.NotFound(
        code: "Certificate.SNINotFound",
        description: "SNI not found");
}

