using ErrorOr;

namespace Sail.Cluster.Errors;

public static class DestinationErrors
{
    public static Error HostRequired => Error.Validation(
        code: "Destination.HostRequired",
        description: "Destination host is required");

    public static Error HostTooLong => Error.Validation(
        code: "Destination.HostTooLong",
        description: "Destination host must not exceed 255 characters");

    public static Error AddressRequired => Error.Validation(
        code: "Destination.AddressRequired",
        description: "Destination address is required");

    public static Error AddressInvalid => Error.Validation(
        code: "Destination.AddressInvalid",
        description: "Destination address must be a valid absolute URL");

    public static Error HealthRequired => Error.Validation(
        code: "Destination.HealthRequired",
        description: "Destination health is required");
}

