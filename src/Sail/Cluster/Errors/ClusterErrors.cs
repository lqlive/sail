using ErrorOr;

namespace Sail.Cluster.Errors;

public static class ClusterErrors
{
    public static Error NameRequired => Error.Validation(
        code: "Cluster.NameRequired",
        description: "Cluster name is required");

    public static Error NameTooLong => Error.Validation(
        code: "Cluster.NameTooLong",
        description: "Cluster name must not exceed 200 characters");

    public static Error ServiceNameRequired => Error.Validation(
        code: "Cluster.ServiceNameRequired",
        description: "Service name is required");

    public static Error ServiceNameTooLong => Error.Validation(
        code: "Cluster.ServiceNameTooLong",
        description: "Service name must not exceed 200 characters");

    public static Error ServiceDiscoveryTypeInvalid => Error.Validation(
        code: "Cluster.ServiceDiscoveryTypeInvalid",
        description: "Invalid service discovery type");

    public static Error LoadBalancingPolicyRequired => Error.Validation(
        code: "Cluster.LoadBalancingPolicyRequired",
        description: "Load balancing policy is required");

    public static Error DestinationsRequired => Error.Validation(
        code: "Cluster.DestinationsRequired",
        description: "Destinations are required");

    public static Error DestinationsEmpty => Error.Validation(
        code: "Cluster.DestinationsEmpty",
        description: "At least one destination must be provided");
}

