using System.Text.Json;
using Yarp.ReverseProxy.Model;

namespace Sail.Core.Utilities;

internal static class RouteConfigExtensions
{
    public static T? GetMetadata<T>(this RouteModel route, string name)
    {
        ArgumentNullException.ThrowIfNull(route);

        var metadata = route.Config.Metadata;

        if (metadata is null || !metadata.TryGetValue(name, out var metadataString))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(metadataString)
            ?? throw new InvalidOperationException($"Metadata '{name}' in cluster '{route.Config.RouteId}' is not of type '{typeof(T).Name}'.");
    }
}
