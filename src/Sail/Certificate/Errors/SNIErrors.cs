using ErrorOr;

namespace Sail.Certificate.Errors;

public static class SNIErrors
{
    public static Error NameRequired => Error.Validation(
        code: "SNI.NameRequired",
        description: "SNI name is required");

    public static Error NameTooLong => Error.Validation(
        code: "SNI.NameTooLong",
        description: "SNI name must not exceed 200 characters");

    public static Error HostNameRequired => Error.Validation(
        code: "SNI.HostNameRequired",
        description: "SNI hostname is required");

    public static Error HostNameTooLong => Error.Validation(
        code: "SNI.HostNameTooLong",
        description: "Hostname must not exceed 255 characters");

    public static Error HostNameInvalid => Error.Validation(
        code: "SNI.HostNameInvalid",
        description: "Hostname must be a valid domain name or wildcard domain (e.g., example.com or *.example.com)");
}

