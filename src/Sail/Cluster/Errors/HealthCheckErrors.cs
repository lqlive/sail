using ErrorOr;

namespace Sail.Cluster.Errors;

public static class ActiveHealthCheckErrors
{
    public static Error IntervalInvalid => Error.Validation(
        code: "ActiveHealthCheck.IntervalInvalid",
        description: "Active health check interval must be greater than zero");

    public static Error TimeoutInvalid => Error.Validation(
        code: "ActiveHealthCheck.TimeoutInvalid",
        description: "Active health check timeout must be greater than zero");

    public static Error PathInvalid => Error.Validation(
        code: "ActiveHealthCheck.PathInvalid",
        description: "Active health check path must start with '/'");
}

public static class PassiveHealthCheckErrors
{
    public static Error ReactivationPeriodInvalid => Error.Validation(
        code: "PassiveHealthCheck.ReactivationPeriodInvalid",
        description: "Passive health check reactivation period must be greater than zero");
}

