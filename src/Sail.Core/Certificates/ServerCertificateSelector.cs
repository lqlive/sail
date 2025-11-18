using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sail.Core.Options;

namespace Sail.Core.Certificates;

public class ServerCertificateSelector(
    ILogger<ServerCertificateSelector> logger,
    IOptions<CertificateOptions> options)
    : IServerCertificateSelector
{
    private readonly ILogger<ServerCertificateSelector> _logger = logger;
    private readonly CertificateOptions _options = options.Value;
    
    private volatile IReadOnlyDictionary<string, X509Certificate2> _certificates = 
        new Dictionary<string, X509Certificate2>(StringComparer.OrdinalIgnoreCase);
    private volatile IReadOnlyDictionary<string, X509Certificate2> _wildcardCertificates = 
        new Dictionary<string, X509Certificate2>(StringComparer.OrdinalIgnoreCase);
    
    private X509Certificate2? _defaultCertificate;

    public IReadOnlyDictionary<string, X509Certificate2> Certificates => _certificates;
    
    public IReadOnlyDictionary<string, X509Certificate2> WildcardCertificates => _wildcardCertificates;

    public X509Certificate2 GetCertificate(ConnectionContext connectionContext, string domainName)
    {
        _logger.LogDebug("ConnectionId: '{ConnectionId}', SNI hostname: '{HostName}'", 
            connectionContext?.ConnectionId, domainName);

        if (string.IsNullOrEmpty(domainName))
        {
            return GetDefaultCertificate();
        }

        var certificates = _certificates;
        if (certificates.TryGetValue(domainName, out var certificate))
        {
            _logger.LogDebug("Exact match found for hostname: '{HostName}'", domainName);
            return certificate;
        }

        var firstDotIndex = domainName.IndexOf('.');
        if (firstDotIndex > 0 && firstDotIndex < domainName.Length - 1)
        {
            var domainSpan = domainName.AsSpan(firstDotIndex + 1);
            var wildcardCertificates = _wildcardCertificates;
            
            foreach (var kvp in wildcardCertificates)
            {
                if (MemoryExtensions.Equals(domainSpan, kvp.Key.AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogDebug("Wildcard match found for hostname: '{HostName}' (*.{Domain})", 
                        domainName, kvp.Key);
                    return kvp.Value;
                }
            }
        }

        _logger.LogWarning("No certificate found for hostname: '{HostName}', using default certificate", 
            domainName);
        return GetDefaultCertificate();
    }

    public Task UpdateAsync(IReadOnlyList<CertificateConfig> certificates, CancellationToken cancellationToken)
    {
        var newCertificates = new Dictionary<string, X509Certificate2>(StringComparer.OrdinalIgnoreCase);
        var newWildcardCertificates = new Dictionary<string, X509Certificate2>(StringComparer.OrdinalIgnoreCase);
        var loadedCerts = new HashSet<X509Certificate2>(ReferenceEqualityComparer.Instance);

        foreach (var config in certificates)
        {
            try
            {
                var cert = LoadCertificateFromConfig(config);
                loadedCerts.Add(cert);
                
                if (config.HostName.StartsWith("*.", StringComparison.Ordinal))
                {
                    var domain = config.HostName.AsSpan(2).ToString();
                    if (newWildcardCertificates.TryAdd(domain, cert))
                    {
                        _logger.LogInformation("Loaded wildcard certificate for domain: *.{Domain}", domain);
                    }
                }
                else
                {
                    if (newCertificates.TryAdd(config.HostName, cert))
                    {
                        _logger.LogInformation("Loaded certificate for hostname: {HostName}", config.HostName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load certificate for hostname: {HostName}", config.HostName);
            }
        }

        var oldCertificates = _certificates;
        var oldWildcardCertificates = _wildcardCertificates;

        var changed = !AreEquivalent(oldCertificates, newCertificates) || 
                     !AreEquivalent(oldWildcardCertificates, newWildcardCertificates);

        if (!changed)
        {
            _logger.LogDebug("Certificates unchanged, skipping update");
            DisposeUnusedCertificates(loadedCerts);
            return Task.CompletedTask;
        }

        _logger.LogInformation("Certificates changed. Applying new bindings for {ExactCount} exact names, {WildcardCount} wildcard names",
            newCertificates.Count, newWildcardCertificates.Count);

        _certificates = newCertificates;
        _wildcardCertificates = newWildcardCertificates;

        DisposeOldCertificates(oldCertificates, oldWildcardCertificates);

        return Task.CompletedTask;
    }

    private X509Certificate2 LoadCertificateFromConfig(CertificateConfig config)
    {
        return LoadCertificateFromPem(config.Cert, config.Key);
    }

    private X509Certificate2 GetDefaultCertificate()
    {
        if (_defaultCertificate != null)
        {
            return _defaultCertificate;
        }

        if (string.IsNullOrEmpty(_options.DefaultPath) || string.IsNullOrEmpty(_options.DefaultKeyPath))
        {
            throw new InvalidOperationException("Default certificate path or key path is not configured.");
        }

        try
        {
            _defaultCertificate = LoadCertificateFromPemFile(_options.DefaultPath, _options.DefaultKeyPath);
            _logger.LogInformation("Loaded default certificate from: {Path}", _options.DefaultPath);
            return _defaultCertificate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load default certificate from: {Path}", _options.DefaultPath);
            throw;
        }
    }

    private static X509Certificate2 LoadCertificateFromPem(string certPem, string keyPem)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return X509Certificate2.CreateFromPem(certPem, keyPem);
        }

        using var pemCertificate = X509Certificate2.CreateFromPem(certPem, keyPem);
        var pfxBytes = pemCertificate.Export(X509ContentType.Pkcs12);
        return X509CertificateLoader.LoadPkcs12(pfxBytes, null, 
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
    }

    private static X509Certificate2 LoadCertificateFromPemFile(string certPath, string keyPath)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return X509Certificate2.CreateFromPemFile(certPath, keyPath);
        }

        using var pemCertificate = X509Certificate2.CreateFromPemFile(certPath, keyPath);
        var pfxBytes = pemCertificate.Export(X509ContentType.Pkcs12);
        return X509CertificateLoader.LoadPkcs12(pfxBytes, null, 
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
    }

    private static bool AreEquivalent(
        IReadOnlyDictionary<string, X509Certificate2> old, 
        IReadOnlyDictionary<string, X509Certificate2> updated)
    {
        return old.Count == updated.Count && 
               old.All(kvp => updated.TryGetValue(kvp.Key, out var cert) && 
                             kvp.Value.Thumbprint == cert.Thumbprint);
    }

    private void DisposeOldCertificates(params IReadOnlyDictionary<string, X509Certificate2>[] certificateCollections)
    {
        var allCertificates = certificateCollections
            .SelectMany(collection => collection.Values)
            .Distinct();

        foreach (var cert in allCertificates)
        {
            TryDispose(cert);
        }
    }

    private void DisposeUnusedCertificates(HashSet<X509Certificate2> certificates)
    {
        foreach (var cert in certificates)
        {
            TryDispose(cert);
        }
    }

    private void TryDispose(X509Certificate2 certificate)
    {
        try
        {
            certificate.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disposing certificate with thumbprint: {Thumbprint}", 
                certificate.Thumbprint);
        }
    }
}