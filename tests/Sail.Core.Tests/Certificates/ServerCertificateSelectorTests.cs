using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Sail.Core.Certificates;
using Sail.Core.Options;

namespace Sail.Core.Tests.Certificates;

public class ServerCertificateSelectorTests : IDisposable
{
    private readonly Mock<ILogger<ServerCertificateSelector>> _mockLogger;
    private readonly Mock<IOptions<CertificateOptions>> _mockOptions;
    private readonly CertificateOptions _certificateOptions;
    private readonly ServerCertificateSelector _selector;
    private readonly List<X509Certificate2> _certificatesToDispose = new();

    public ServerCertificateSelectorTests()
    {
        _mockLogger = new Mock<ILogger<ServerCertificateSelector>>();
        _certificateOptions = new CertificateOptions();
        _mockOptions = new Mock<IOptions<CertificateOptions>>();
        _mockOptions.Setup(x => x.Value).Returns(_certificateOptions);
        _selector = new ServerCertificateSelector(_mockLogger.Object, _mockOptions.Object);
    }

    [Fact]
    public void GetCertificate_WithNullDomainName_ThrowsInvalidOperationException()
    {
        // Arrange
        var context = new Mock<ConnectionContext>().Object;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _selector.GetCertificate(context, null!));
    }

    [Fact]
    public void GetCertificate_WithEmptyDomainName_ThrowsInvalidOperationException()
    {
        // Arrange
        var context = new Mock<ConnectionContext>().Object;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _selector.GetCertificate(context, string.Empty));
    }

    [Fact]
    public async Task GetCertificate_WithExactMatch_ReturnsCorrectCertificate()
    {
        // Arrange
        var hostname = "example.com";
        var cert = CreateTestCertificate(hostname);
        _certificatesToDispose.Add(cert);

        var configs = new List<CertificateConfig>
        {
            new()
            {
                HostName = hostname,
                Cert = ExportToPem(cert),
                Key = ExportPrivateKeyToPem(cert)
            }
        };

        await _selector.UpdateAsync(configs, CancellationToken.None);

        var context = new Mock<ConnectionContext>().Object;

        // Act
        var result = _selector.GetCertificate(context, hostname);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(hostname, result.Subject);
    }

    [Fact]
    public async Task GetCertificate_IsCaseInsensitive()
    {
        // Arrange
        var hostname = "Example.Com";
        var cert = CreateTestCertificate(hostname);
        _certificatesToDispose.Add(cert);

        var configs = new List<CertificateConfig>
        {
            new()
            {
                HostName = hostname,
                Cert = ExportToPem(cert),
                Key = ExportPrivateKeyToPem(cert)
            }
        };

        await _selector.UpdateAsync(configs, CancellationToken.None);

        var context = new Mock<ConnectionContext>().Object;

        // Act
        var result1 = _selector.GetCertificate(context, "example.com");
        var result2 = _selector.GetCertificate(context, "EXAMPLE.COM");
        var result3 = _selector.GetCertificate(context, "Example.Com");

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotNull(result3);
        Assert.Equal(result1.Thumbprint, result2.Thumbprint);
        Assert.Equal(result2.Thumbprint, result3.Thumbprint);
    }

    [Fact]
    public async Task GetCertificate_WithWildcardMatch_ReturnsCorrectCertificate()
    {
        // Arrange
        var wildcardHostname = "*.example.com";
        var cert = CreateTestCertificate(wildcardHostname);
        _certificatesToDispose.Add(cert);

        var configs = new List<CertificateConfig>
        {
            new()
            {
                HostName = wildcardHostname,
                Cert = ExportToPem(cert),
                Key = ExportPrivateKeyToPem(cert)
            }
        };

        await _selector.UpdateAsync(configs, CancellationToken.None);

        var context = new Mock<ConnectionContext>().Object;

        // Act
        var result1 = _selector.GetCertificate(context, "sub.example.com");
        var result2 = _selector.GetCertificate(context, "api.example.com");
        var result3 = _selector.GetCertificate(context, "www.example.com");

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotNull(result3);
        Assert.Equal(result1.Thumbprint, result2.Thumbprint);
        Assert.Equal(result2.Thumbprint, result3.Thumbprint);
    }

    [Fact]
    public async Task GetCertificate_WildcardDoesNotMatchBaseDomain()
    {
        // Arrange
        var wildcardHostname = "*.example.com";
        var cert = CreateTestCertificate(wildcardHostname);
        _certificatesToDispose.Add(cert);

        var configs = new List<CertificateConfig>
        {
            new()
            {
                HostName = wildcardHostname,
                Cert = ExportToPem(cert),
                Key = ExportPrivateKeyToPem(cert)
            }
        };

        await _selector.UpdateAsync(configs, CancellationToken.None);

        var context = new Mock<ConnectionContext>().Object;

        // Act & Assert
        // Wildcard should not match the base domain "example.com"
        Assert.Throws<InvalidOperationException>(() => _selector.GetCertificate(context, "example.com"));
    }

    [Fact]
    public async Task GetCertificate_ExactMatchTakesPrecedenceOverWildcard()
    {
        // Arrange
        var exactHostname = "api.example.com";
        var wildcardHostname = "*.example.com";

        var exactCert = CreateTestCertificate(exactHostname);
        var wildcardCert = CreateTestCertificate(wildcardHostname);
        _certificatesToDispose.Add(exactCert);
        _certificatesToDispose.Add(wildcardCert);

        var configs = new List<CertificateConfig>
        {
            new()
            {
                HostName = wildcardHostname,
                Cert = ExportToPem(wildcardCert),
                Key = ExportPrivateKeyToPem(wildcardCert)
            },
            new()
            {
                HostName = exactHostname,
                Cert = ExportToPem(exactCert),
                Key = ExportPrivateKeyToPem(exactCert)
            }
        };

        await _selector.UpdateAsync(configs, CancellationToken.None);

        var context = new Mock<ConnectionContext>().Object;

        // Act
        var result = _selector.GetCertificate(context, exactHostname);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(exactCert.Thumbprint, result.Thumbprint);
        Assert.NotEqual(wildcardCert.Thumbprint, result.Thumbprint);
    }

    [Fact]
    public async Task UpdateAsync_WithMultipleCertificates_AllAvailable()
    {
        // Arrange
        var cert1 = CreateTestCertificate("example1.com");
        var cert2 = CreateTestCertificate("example2.com");
        var cert3 = CreateTestCertificate("*.example3.com");
        _certificatesToDispose.AddRange(new[] { cert1, cert2, cert3 });

        var configs = new List<CertificateConfig>
        {
            new()
            {
                HostName = "example1.com",
                Cert = ExportToPem(cert1),
                Key = ExportPrivateKeyToPem(cert1)
            },
            new()
            {
                HostName = "example2.com",
                Cert = ExportToPem(cert2),
                Key = ExportPrivateKeyToPem(cert2)
            },
            new()
            {
                HostName = "*.example3.com",
                Cert = ExportToPem(cert3),
                Key = ExportPrivateKeyToPem(cert3)
            }
        };

        // Act
        await _selector.UpdateAsync(configs, CancellationToken.None);

        // Assert
        Assert.Equal(2, _selector.Certificates.Count);
        Assert.Single(_selector.WildcardCertificates);
    }

    [Fact]
    public async Task UpdateAsync_ReplacesExistingCertificates()
    {
        // Arrange
        var oldCert = CreateTestCertificate("old.example.com");
        var newCert = CreateTestCertificate("new.example.com");
        _certificatesToDispose.AddRange(new[] { oldCert, newCert });

        var oldConfigs = new List<CertificateConfig>
        {
            new()
            {
                HostName = "old.example.com",
                Cert = ExportToPem(oldCert),
                Key = ExportPrivateKeyToPem(oldCert)
            }
        };

        var newConfigs = new List<CertificateConfig>
        {
            new()
            {
                HostName = "new.example.com",
                Cert = ExportToPem(newCert),
                Key = ExportPrivateKeyToPem(newCert)
            }
        };

        // Act
        await _selector.UpdateAsync(oldConfigs, CancellationToken.None);
        await _selector.UpdateAsync(newConfigs, CancellationToken.None);

        // Assert
        Assert.Single(_selector.Certificates);
        Assert.True(_selector.Certificates.ContainsKey("new.example.com"));
        Assert.False(_selector.Certificates.ContainsKey("old.example.com"));
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyConfigs_ClearsCertificates()
    {
        // Arrange
        var cert = CreateTestCertificate("example.com");
        _certificatesToDispose.Add(cert);

        var configs = new List<CertificateConfig>
        {
            new()
            {
                HostName = "example.com",
                Cert = ExportToPem(cert),
                Key = ExportPrivateKeyToPem(cert)
            }
        };

        await _selector.UpdateAsync(configs, CancellationToken.None);

        // Act
        await _selector.UpdateAsync(new List<CertificateConfig>(), CancellationToken.None);

        // Assert
        Assert.Empty(_selector.Certificates);
        Assert.Empty(_selector.WildcardCertificates);
    }

    [Fact]
    public async Task UpdateAsync_WithSameCertificates_NoChange()
    {
        // Arrange
        var cert = CreateTestCertificate("example.com");
        _certificatesToDispose.Add(cert);

        var configs = new List<CertificateConfig>
        {
            new()
            {
                HostName = "example.com",
                Cert = ExportToPem(cert),
                Key = ExportPrivateKeyToPem(cert)
            }
        };

        // Act
        await _selector.UpdateAsync(configs, CancellationToken.None);
        var certCountBefore = _selector.Certificates.Count;
        await _selector.UpdateAsync(configs, CancellationToken.None);
        var certCountAfter = _selector.Certificates.Count;

        // Assert
        Assert.Equal(certCountBefore, certCountAfter);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidCertificate_SkipsAndContinues()
    {
        // Arrange
        var validCert = CreateTestCertificate("valid.example.com");
        _certificatesToDispose.Add(validCert);

        var configs = new List<CertificateConfig>
        {
            new()
            {
                HostName = "invalid.example.com",
                Cert = "invalid-cert-pem",
                Key = "invalid-key-pem"
            },
            new()
            {
                HostName = "valid.example.com",
                Cert = ExportToPem(validCert),
                Key = ExportPrivateKeyToPem(validCert)
            }
        };

        // Act
        await _selector.UpdateAsync(configs, CancellationToken.None);

        // Assert
        Assert.Single(_selector.Certificates);
        Assert.True(_selector.Certificates.ContainsKey("valid.example.com"));
        Assert.False(_selector.Certificates.ContainsKey("invalid.example.com"));
    }

    [Fact]
    public async Task Certificates_Property_ReturnsReadOnlyDictionary()
    {
        // Arrange
        var cert = CreateTestCertificate("example.com");
        _certificatesToDispose.Add(cert);

        var configs = new List<CertificateConfig>
        {
            new()
            {
                HostName = "example.com",
                Cert = ExportToPem(cert),
                Key = ExportPrivateKeyToPem(cert)
            }
        };

        // Act
        await _selector.UpdateAsync(configs, CancellationToken.None);

        // Assert
        Assert.IsAssignableFrom<IReadOnlyDictionary<string, X509Certificate2>>(_selector.Certificates);
        Assert.Single(_selector.Certificates);
    }

    [Fact]
    public async Task WildcardCertificates_Property_ReturnsReadOnlyDictionary()
    {
        // Arrange
        var cert = CreateTestCertificate("*.example.com");
        _certificatesToDispose.Add(cert);

        var configs = new List<CertificateConfig>
        {
            new()
            {
                HostName = "*.example.com",
                Cert = ExportToPem(cert),
                Key = ExportPrivateKeyToPem(cert)
            }
        };

        // Act
        await _selector.UpdateAsync(configs, CancellationToken.None);

        // Assert
        Assert.IsAssignableFrom<IReadOnlyDictionary<string, X509Certificate2>>(_selector.WildcardCertificates);
        Assert.Single(_selector.WildcardCertificates);
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateHostname_OnlyFirstIsAdded()
    {
        // Arrange
        var cert1 = CreateTestCertificate("example.com");
        var cert2 = CreateTestCertificate("example.com");
        _certificatesToDispose.AddRange(new[] { cert1, cert2 });

        var configs = new List<CertificateConfig>
        {
            new()
            {
                HostName = "example.com",
                Cert = ExportToPem(cert1),
                Key = ExportPrivateKeyToPem(cert1)
            },
            new()
            {
                HostName = "example.com",
                Cert = ExportToPem(cert2),
                Key = ExportPrivateKeyToPem(cert2)
            }
        };

        // Act
        await _selector.UpdateAsync(configs, CancellationToken.None);

        // Assert
        Assert.Single(_selector.Certificates);
        var storedCert = _selector.Certificates["example.com"];
        Assert.Equal(cert1.Thumbprint, storedCert.Thumbprint);
    }

    public void Dispose()
    {
        foreach (var cert in _certificatesToDispose)
        {
            cert?.Dispose();
        }
    }

    #region Helper Methods

    private static X509Certificate2 CreateTestCertificate(string subjectName)
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            $"CN={subjectName}",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(false, false, 0, false));

        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                false));

        request.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(
                new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, // serverAuth
                false));

        var sanBuilder = new SubjectAlternativeNameBuilder();
        if (subjectName.StartsWith("*."))
        {
            sanBuilder.AddDnsName(subjectName);
        }
        else
        {
            sanBuilder.AddDnsName(subjectName);
        }
        request.CertificateExtensions.Add(sanBuilder.Build());

        var certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(365));

        // Export to PFX and reload to ensure the private key is exportable
        var pfxBytes = certificate.Export(X509ContentType.Pfx, string.Empty);
        return X509CertificateLoader.LoadPkcs12(pfxBytes, string.Empty,
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
    }

    private static string ExportToPem(X509Certificate2 certificate)
    {
        return certificate.ExportCertificatePem();
    }

    private static string ExportPrivateKeyToPem(X509Certificate2 certificate)
    {
        if (certificate.GetRSAPrivateKey() is RSA rsa)
        {
            return rsa.ExportRSAPrivateKeyPem();
        }

        throw new InvalidOperationException("Certificate does not have an RSA private key.");
    }

    #endregion
}

