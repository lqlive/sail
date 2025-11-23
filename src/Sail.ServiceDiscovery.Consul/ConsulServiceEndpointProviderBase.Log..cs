using Microsoft.Extensions.Logging;

namespace Sail.ServiceDiscovery.Consul;

internal partial class ConsulServiceEndpointProviderBase
{
    internal static partial class Log
    {
        [LoggerMessage(1, LogLevel.Trace,
            "Resolving endpoints for service '{ServiceName}' using host lookup for name '{RecordName}'.",
            EventName = "AddressQuery")]
        public static partial void AddressQuery(ILogger logger, string serviceName, string recordName);

        [LoggerMessage(2, LogLevel.Debug, "Skipping endpoint resolution for service '{ServiceName}': '{Reason}'.",
            EventName = "SkippedResolution")]
        public static partial void SkippedResolution(ILogger logger, string serviceName, string reason);
    }
}