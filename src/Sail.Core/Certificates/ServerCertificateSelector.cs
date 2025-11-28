using Sail.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Connections;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

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
        Log.CertificateRequested(_logger, connectionContext?.ConnectionId, domainName);

        if (string.IsNullOrEmpty(domainName))
        {
            return GetDefaultCertificate();
        }

        var certificates = _certificates;
        if (certificates.TryGetValue(domainName, out var certificate))
        {
            Log.ExactMatchFound(_logger, domainName);
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
                    Log.WildcardMatchFound(_logger, domainName, kvp.Key);
                    return kvp.Value;
                }
            }
        }

        Log.NoCertificateFound(_logger, domainName);
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
                        Log.LoadedWildcardCertificate(_logger, domain);
                    }
                }
                else
                {
                    if (newCertificates.TryAdd(config.HostName, cert))
                    {
                        Log.LoadedCertificate(_logger, config.HostName);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.FailedToLoadCertificate(_logger, config.HostName, ex);
            }
        }

        var oldCertificates = _certificates;
        var oldWildcardCertificates = _wildcardCertificates;

        var changed = !AreEquivalent(oldCertificates, newCertificates) ||
                     !AreEquivalent(oldWildcardCertificates, newWildcardCertificates);

        if (!changed)
        {
            Log.CertificatesUnchanged(_logger);
            DisposeUnusedCertificates(loadedCerts);
            return Task.CompletedTask;
        }

        Log.CertificatesChanged(_logger, newCertificates.Count, newWildcardCertificates.Count);

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
            Log.LoadedDefaultCertificate(_logger, _options.DefaultPath);
            return _defaultCertificate;
        }
        catch (Exception ex)
        {
            Log.FailedToLoadDefaultCertificate(_logger, _options.DefaultPath, ex);
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
            Log.ErrorDisposingCertificate(_logger, certificate.Thumbprint, ex);
        }
    }

    private static class Log
    {
        private static readonly Action<ILogger, string?, string?, Exception?> _certificateRequested = LoggerMessage.Define<string?, string?>(
            LogLevel.Debug,
            new EventId(1, nameof(CertificateRequested)),
            "ConnectionId: '{ConnectionId}', SNI hostname: '{HostName}'");

        private static readonly Action<ILogger, string, Exception?> _exactMatchFound = LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(2, nameof(ExactMatchFound)),
            "Exact match found for hostname: '{HostName}'");

        private static readonly Action<ILogger, string, string, Exception?> _wildcardMatchFound = LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(3, nameof(WildcardMatchFound)),
            "Wildcard match found for hostname: '{HostName}' (*.{Domain})");

        private static readonly Action<ILogger, string, Exception?> _noCertificateFound = LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(4, nameof(NoCertificateFound)),
            "No certificate found for hostname: '{HostName}', using default certificate");

        private static readonly Action<ILogger, string, Exception?> _loadedWildcardCertificate = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(5, nameof(LoadedWildcardCertificate)),
            "Loaded wildcard certificate for domain: *.{Domain}");

        private static readonly Action<ILogger, string, Exception?> _loadedCertificate = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(6, nameof(LoadedCertificate)),
            "Loaded certificate for hostname: {HostName}");

        private static readonly Action<ILogger, string, Exception?> _failedToLoadCertificate = LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(7, nameof(FailedToLoadCertificate)),
            "Failed to load certificate for hostname: {HostName}");

        private static readonly Action<ILogger, Exception?> _certificatesUnchanged = LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(8, nameof(CertificatesUnchanged)),
            "Certificates unchanged, skipping update");

        private static readonly Action<ILogger, int, int, Exception?> _certificatesChanged = LoggerMessage.Define<int, int>(
            LogLevel.Information,
            new EventId(9, nameof(CertificatesChanged)),
            "Certificates changed. Applying new bindings for {ExactCount} exact names, {WildcardCount} wildcard names");

        private static readonly Action<ILogger, string, Exception?> _loadedDefaultCertificate = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(10, nameof(LoadedDefaultCertificate)),
            "Loaded default certificate from: {Path}");

        private static readonly Action<ILogger, string, Exception?> _failedToLoadDefaultCertificate = LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(11, nameof(FailedToLoadDefaultCertificate)),
            "Failed to load default certificate from: {Path}");

        private static readonly Action<ILogger, string, Exception?> _errorDisposingCertificate = LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(12, nameof(ErrorDisposingCertificate)),
            "Error disposing certificate with thumbprint: {Thumbprint}");

        public static void CertificateRequested(ILogger logger, string? connectionId, string? hostName)
        {
            _certificateRequested(logger, connectionId, hostName, null);
        }

        public static void ExactMatchFound(ILogger logger, string hostName)
        {
            _exactMatchFound(logger, hostName, null);
        }

        public static void WildcardMatchFound(ILogger logger, string hostName, string domain)
        {
            _wildcardMatchFound(logger, hostName, domain, null);
        }

        public static void NoCertificateFound(ILogger logger, string hostName)
        {
            _noCertificateFound(logger, hostName, null);
        }

        public static void LoadedWildcardCertificate(ILogger logger, string domain)
        {
            _loadedWildcardCertificate(logger, domain, null);
        }

        public static void LoadedCertificate(ILogger logger, string hostName)
        {
            _loadedCertificate(logger, hostName, null);
        }

        public static void FailedToLoadCertificate(ILogger logger, string hostName, Exception exception)
        {
            _failedToLoadCertificate(logger, hostName, exception);
        }

        public static void CertificatesUnchanged(ILogger logger)
        {
            _certificatesUnchanged(logger, null);
        }

        public static void CertificatesChanged(ILogger logger, int exactCount, int wildcardCount)
        {
            _certificatesChanged(logger, exactCount, wildcardCount, null);
        }

        public static void LoadedDefaultCertificate(ILogger logger, string path)
        {
            _loadedDefaultCertificate(logger, path, null);
        }

        public static void FailedToLoadDefaultCertificate(ILogger logger, string path, Exception exception)
        {
            _failedToLoadDefaultCertificate(logger, path, exception);
        }

        public static void ErrorDisposingCertificate(ILogger logger, string thumbprint, Exception exception)
        {
            _errorDisposingCertificate(logger, thumbprint, exception);
        }
    }
}