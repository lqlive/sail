using System.Text.Json;
using Yarp.ReverseProxy.Model;

namespace Sail.Core.Utilities;

internal static class ClusterConfigExtensions
{
    public static T GetMetadata<T>(this ClusterModel cluster, string name)
    {
        ArgumentNullException.ThrowIfNull(cluster);

        var metadata = cluster.Config.Metadata;

        if (metadata is null || !metadata.TryGetValue(name, out var metadataString))
        {
            throw new KeyNotFoundException($"Metadata '{name}' not found in cluster '{cluster.Config.ClusterId}'.");
        }

        return JsonSerializer.Deserialize<T>(metadataString)
            ?? throw new InvalidOperationException($"Metadata '{name}' in cluster '{cluster.Config.ClusterId}' is not of type '{typeof(T).Name}'.");
    }
}