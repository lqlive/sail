using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.ServiceDiscovery;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.ServiceDiscovery;

namespace Sail.Core.ServiceDiscovery;

internal sealed class ServiceDiscoveryDestinationResolver : IDestinationResolver
{
    private readonly ServiceEndpointResolver _resolver;
    private readonly ServiceDiscoveryOptions _options;

    public ServiceDiscoveryDestinationResolver(
        ServiceEndpointResolver resolver,
        IOptions<ServiceDiscoveryOptions> options)
    {
        _resolver = resolver;
        _options = options.Value;
    }

    public async ValueTask<ResolvedDestinationCollection> ResolveDestinationsAsync(
        IReadOnlyDictionary<string, DestinationConfig> destinations,
        CancellationToken cancellationToken)
    {
        var results = new Dictionary<string, DestinationConfig>();
        var tasks = new List<Task<(List<(string Name, DestinationConfig Config)>, IChangeToken? ChangeToken)>>(destinations.Count);

        foreach (var (destinationId, destinationConfig) in destinations)
        {
            tasks.Add(ResolveHostAsync(destinationId, destinationConfig, cancellationToken));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
        
        var changeTokens = new List<IChangeToken>();
        foreach (var task in tasks)
        {
            var (configs, changeToken) = await task.ConfigureAwait(false);
            if (changeToken is not null)
            {
                changeTokens.Add(changeToken);
            }

            foreach (var (name, config) in configs)
            {
                results[name] = config;
            }
        }

        return new ResolvedDestinationCollection(results, new CompositeChangeToken(changeTokens));
    }

    private async Task<(List<(string Name, DestinationConfig Config)>, IChangeToken? ChangeToken)> ResolveHostAsync(
        string originalName,
        DestinationConfig originalConfig,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(originalConfig.Host))
        {
            return ([(originalName, originalConfig)], null);
        }

        var originalUri = new Uri(originalConfig.Address);
        var serviceName = originalUri.GetLeftPart(UriPartial.Authority);

        var result = await _resolver.GetEndpointsAsync(serviceName, cancellationToken).ConfigureAwait(false);
        var results = new List<(string Name, DestinationConfig Config)>(result.Endpoints.Count);
        var uriBuilder = new UriBuilder(originalUri);
        var healthUri = originalConfig.Health is { Length: > 0 } health ? new Uri(health) : null;
        var healthUriBuilder = healthUri is not null ? new UriBuilder(healthUri) : null;

        foreach (var endpoint in result.Endpoints)
        {
            var addressString = endpoint.ToString()!;
            Uri uri;
            if (!addressString.Contains("://"))
            {
                var scheme = GetDefaultScheme(originalUri);
                uri = new Uri($"{scheme}://{addressString}");
            }
            else
            {
                uri = new Uri(addressString);
            }

            uriBuilder.Scheme = uri.Scheme;
            uriBuilder.Host = uri.Host;
            uriBuilder.Port = uri.Port;
            var resolvedAddress = uriBuilder.Uri.ToString();
            var healthAddress = originalConfig.Health;
            
            if (healthUriBuilder is not null)
            {
                healthUriBuilder.Host = uri.Host;
                healthUriBuilder.Port = uri.Port;
                healthAddress = healthUriBuilder.Uri.ToString();
            }

            var name = $"{originalName}[{addressString}]";
            string? resolvedHost;

            if (!string.IsNullOrEmpty(originalConfig.Host))
            {
                resolvedHost = originalConfig.Host;
            }
            else if (uri.IsLoopback)
            {
                resolvedHost = null;
            }
            else
            {
                resolvedHost = originalUri.Authority;
            }

            var config = originalConfig with { Host = resolvedHost, Address = resolvedAddress, Health = healthAddress };
            results.Add((name, config));
        }

        return (results, result.ChangeToken);
    }

    private string GetDefaultScheme(Uri originalUri)
    {
        if (originalUri.Scheme.IndexOf('+') > 0)
        {
            var specifiedSchemes = originalUri.Scheme.Split('+');
            foreach (var scheme in specifiedSchemes)
            {
                if (_options.AllowAllSchemes || _options.AllowedSchemes.Contains(scheme, StringComparer.OrdinalIgnoreCase))
                {
                    return scheme;
                }
            }

            throw new InvalidOperationException($"None of the specified schemes ('{string.Join(", ", specifiedSchemes)}') are allowed by configuration.");
        }
        
        return originalUri.Scheme;
    }
}
