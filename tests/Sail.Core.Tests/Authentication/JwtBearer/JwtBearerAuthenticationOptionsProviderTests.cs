using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Sail.Core.Authentication;
using Sail.Core.Authentication.JwtBearer;

namespace Sail.Core.Tests.Authentication.JwtBearer;

public class JwtBearerAuthenticationOptionsProviderTests
{
    private readonly Mock<IAuthenticationSchemeProvider> _mockSchemeProvider;
    private readonly Mock<IOptionsMonitorCache<JwtBearerOptions>> _mockOptionsCache;
    private readonly SailAuthorizationPolicyProvider _policyProvider;
    private readonly Mock<ILogger<JwtBearerAuthenticationOptionsProvider>> _mockLogger;
    private readonly JwtBearerAuthenticationOptionsProvider _provider;
    private readonly List<AuthenticationScheme> _schemes;

    public JwtBearerAuthenticationOptionsProviderTests()
    {
        _mockSchemeProvider = new Mock<IAuthenticationSchemeProvider>();
        _mockOptionsCache = new Mock<IOptionsMonitorCache<JwtBearerOptions>>();
        var authOptions = Microsoft.Extensions.Options.Options.Create(new Microsoft.AspNetCore.Authorization.AuthorizationOptions());
        _policyProvider = new SailAuthorizationPolicyProvider(authOptions);
        _mockLogger = new Mock<ILogger<JwtBearerAuthenticationOptionsProvider>>();
        _schemes = new List<AuthenticationScheme>();

        _mockSchemeProvider
            .Setup(x => x.GetAllSchemesAsync())
            .ReturnsAsync(() => _schemes);

        _mockSchemeProvider
            .Setup(x => x.AddScheme(It.IsAny<AuthenticationScheme>()))
            .Callback<AuthenticationScheme>(scheme => _schemes.Add(scheme));

        _mockSchemeProvider
            .Setup(x => x.RemoveScheme(It.IsAny<string>()))
            .Callback<string>(name => _schemes.RemoveAll(s => s.Name == name));

        _mockOptionsCache
            .Setup(x => x.TryAdd(It.IsAny<string>(), It.IsAny<JwtBearerOptions>()))
            .Returns(true);

        _mockOptionsCache
            .Setup(x => x.TryRemove(It.IsAny<string>()));

        _provider = new JwtBearerAuthenticationOptionsProvider(
            _mockSchemeProvider.Object,
            _mockOptionsCache.Object,
            _policyProvider,
            _mockLogger.Object);
    }

    [Fact]
    public async Task UpdateAsync_WithSingleConfiguration_AddsScheme()
    {
        // Arrange
        var configurations = new Dictionary<string, JwtBearerConfiguration>
        {
            ["JwtScheme"] = new()
            {
                Authority = "https://auth.example.com",
                Audience = "api1"
            }
        };

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        _mockSchemeProvider.Verify(x => x.AddScheme(It.Is<AuthenticationScheme>(
            s => s.Name == "JwtScheme" && s.HandlerType == typeof(JwtBearerHandler)
        )), Times.Once);
        _mockOptionsCache.Verify(x => x.TryAdd("JwtScheme", It.IsAny<JwtBearerOptions>()), Times.Once);
        // Verify policy was added by checking we can retrieve it
        var policy = await _policyProvider.GetPolicyAsync("JwtScheme");
        Assert.NotNull(policy);
    }

    [Fact]
    public async Task UpdateAsync_WithMultipleConfigurations_AddsAllSchemes()
    {
        // Arrange
        var configurations = new Dictionary<string, JwtBearerConfiguration>
        {
            ["Scheme1"] = new()
            {
                Authority = "https://auth1.example.com",
                Audience = "api1"
            },
            ["Scheme2"] = new()
            {
                Authority = "https://auth2.example.com",
                Audience = "api2"
            },
            ["Scheme3"] = new()
            {
                Authority = "https://auth3.example.com",
                Audience = "api3"
            }
        };

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        _mockSchemeProvider.Verify(x => x.AddScheme(It.IsAny<AuthenticationScheme>()), Times.Exactly(3));
        _mockOptionsCache.Verify(x => x.TryAdd(It.IsAny<string>(), It.IsAny<JwtBearerOptions>()), Times.Exactly(3));
        // Verify policies were added by checking we can retrieve them
        Assert.NotNull(await _policyProvider.GetPolicyAsync("Scheme1"));
        Assert.NotNull(await _policyProvider.GetPolicyAsync("Scheme2"));
        Assert.NotNull(await _policyProvider.GetPolicyAsync("Scheme3"));
    }

    [Fact]
    public async Task UpdateAsync_WithExistingScheme_UpdatesScheme()
    {
        // Arrange
        _schemes.Add(new AuthenticationScheme("ExistingScheme", "ExistingScheme", typeof(JwtBearerHandler)));

        var configurations = new Dictionary<string, JwtBearerConfiguration>
        {
            ["ExistingScheme"] = new()
            {
                Authority = "https://auth.example.com",
                Audience = "api1"
            }
        };

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        _mockSchemeProvider.Verify(x => x.AddScheme(It.IsAny<AuthenticationScheme>()), Times.Never);
        _mockOptionsCache.Verify(x => x.TryRemove("ExistingScheme"), Times.Once);
        _mockOptionsCache.Verify(x => x.TryAdd("ExistingScheme", It.IsAny<JwtBearerOptions>()), Times.Once);
        // Verify policy still exists
        Assert.NotNull(await _policyProvider.GetPolicyAsync("ExistingScheme"));
    }

    [Fact]
    public async Task UpdateAsync_RemovesObsoleteSchemes()
    {
        // Arrange
        _schemes.Add(new AuthenticationScheme("OldScheme", "OldScheme", typeof(JwtBearerHandler)));
        _schemes.Add(new AuthenticationScheme("AnotherOldScheme", "AnotherOldScheme", typeof(JwtBearerHandler)));

        var configurations = new Dictionary<string, JwtBearerConfiguration>
        {
            ["NewScheme"] = new()
            {
                Authority = "https://auth.example.com",
                Audience = "api1"
            }
        };

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        _mockSchemeProvider.Verify(x => x.RemoveScheme("OldScheme"), Times.Once);
        _mockSchemeProvider.Verify(x => x.RemoveScheme("AnotherOldScheme"), Times.Once);
        _mockOptionsCache.Verify(x => x.TryRemove("OldScheme"), Times.Once);
        _mockOptionsCache.Verify(x => x.TryRemove("AnotherOldScheme"), Times.Once);
        // Verify policies were removed
        Assert.Null(await _policyProvider.GetPolicyAsync("OldScheme"));
        Assert.Null(await _policyProvider.GetPolicyAsync("AnotherOldScheme"));
    }

    [Fact]
    public async Task UpdateAsync_WithValidIssuers_ConfiguresTokenValidation()
    {
        // Arrange
        var configurations = new Dictionary<string, JwtBearerConfiguration>
        {
            ["JwtScheme"] = new()
            {
                Authority = "https://auth.example.com",
                Audience = "api1",
                ValidIssuers = new List<string> { "issuer1", "issuer2" }
            }
        };

        JwtBearerOptions? capturedOptions = null;
        _mockOptionsCache
            .Setup(x => x.TryAdd(It.IsAny<string>(), It.IsAny<JwtBearerOptions>()))
            .Callback<string, JwtBearerOptions>((_, options) => capturedOptions = options)
            .Returns(true);

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.NotNull(capturedOptions.TokenValidationParameters.ValidIssuers);
        Assert.Equal(2, capturedOptions.TokenValidationParameters.ValidIssuers.Count());
        Assert.Contains("issuer1", capturedOptions.TokenValidationParameters.ValidIssuers);
        Assert.Contains("issuer2", capturedOptions.TokenValidationParameters.ValidIssuers);
    }

    [Fact]
    public async Task UpdateAsync_WithValidAudiences_ConfiguresTokenValidation()
    {
        // Arrange
        var configurations = new Dictionary<string, JwtBearerConfiguration>
        {
            ["JwtScheme"] = new()
            {
                Authority = "https://auth.example.com",
                Audience = "api1",
                ValidAudiences = new List<string> { "audience1", "audience2", "audience3" }
            }
        };

        JwtBearerOptions? capturedOptions = null;
        _mockOptionsCache
            .Setup(x => x.TryAdd(It.IsAny<string>(), It.IsAny<JwtBearerOptions>()))
            .Callback<string, JwtBearerOptions>((_, options) => capturedOptions = options)
            .Returns(true);

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.NotNull(capturedOptions.TokenValidationParameters.ValidAudiences);
        Assert.Equal(3, capturedOptions.TokenValidationParameters.ValidAudiences.Count());
    }

    [Fact]
    public async Task UpdateAsync_WithCustomClockSkew_ConfiguresTokenValidation()
    {
        // Arrange
        var configurations = new Dictionary<string, JwtBearerConfiguration>
        {
            ["JwtScheme"] = new()
            {
                Authority = "https://auth.example.com",
                Audience = "api1",
                ClockSkew = 120 // 2 minutes
            }
        };

        JwtBearerOptions? capturedOptions = null;
        _mockOptionsCache
            .Setup(x => x.TryAdd(It.IsAny<string>(), It.IsAny<JwtBearerOptions>()))
            .Callback<string, JwtBearerOptions>((_, options) => capturedOptions = options)
            .Returns(true);

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.Equal(TimeSpan.FromMinutes(2), capturedOptions.TokenValidationParameters.ClockSkew);
    }

    [Fact]
    public async Task UpdateAsync_WithoutClockSkew_UsesDefaultClockSkew()
    {
        // Arrange
        var configurations = new Dictionary<string, JwtBearerConfiguration>
        {
            ["JwtScheme"] = new()
            {
                Authority = "https://auth.example.com",
                Audience = "api1"
            }
        };

        JwtBearerOptions? capturedOptions = null;
        _mockOptionsCache
            .Setup(x => x.TryAdd(It.IsAny<string>(), It.IsAny<JwtBearerOptions>()))
            .Callback<string, JwtBearerOptions>((_, options) => capturedOptions = options)
            .Returns(true);

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.Equal(TimeSpan.FromMinutes(5), capturedOptions.TokenValidationParameters.ClockSkew);
    }

    [Fact]
    public async Task UpdateAsync_WithValidationFlags_ConfiguresTokenValidation()
    {
        // Arrange
        var configurations = new Dictionary<string, JwtBearerConfiguration>
        {
            ["JwtScheme"] = new()
            {
                Authority = "https://auth.example.com",
                Audience = "api1",
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = false
            }
        };

        JwtBearerOptions? capturedOptions = null;
        _mockOptionsCache
            .Setup(x => x.TryAdd(It.IsAny<string>(), It.IsAny<JwtBearerOptions>()))
            .Callback<string, JwtBearerOptions>((_, options) => capturedOptions = options)
            .Returns(true);

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.False(capturedOptions.TokenValidationParameters.ValidateIssuer);
        Assert.False(capturedOptions.TokenValidationParameters.ValidateAudience);
        Assert.False(capturedOptions.TokenValidationParameters.ValidateLifetime);
        Assert.False(capturedOptions.TokenValidationParameters.ValidateIssuerSigningKey);
    }

    [Fact]
    public async Task UpdateAsync_WithRequireHttpsMetadata_ConfiguresOptions()
    {
        // Arrange
        var configurations = new Dictionary<string, JwtBearerConfiguration>
        {
            ["JwtScheme"] = new()
            {
                Authority = "https://auth.example.com",
                Audience = "api1",
                RequireHttpsMetadata = false
            }
        };

        JwtBearerOptions? capturedOptions = null;
        _mockOptionsCache
            .Setup(x => x.TryAdd(It.IsAny<string>(), It.IsAny<JwtBearerOptions>()))
            .Callback<string, JwtBearerOptions>((_, options) => capturedOptions = options)
            .Returns(true);

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.False(capturedOptions.RequireHttpsMetadata);
    }

    [Fact]
    public async Task UpdateAsync_WithSaveToken_ConfiguresOptions()
    {
        // Arrange
        var configurations = new Dictionary<string, JwtBearerConfiguration>
        {
            ["JwtScheme"] = new()
            {
                Authority = "https://auth.example.com",
                Audience = "api1",
                SaveToken = true
            }
        };

        JwtBearerOptions? capturedOptions = null;
        _mockOptionsCache
            .Setup(x => x.TryAdd(It.IsAny<string>(), It.IsAny<JwtBearerOptions>()))
            .Callback<string, JwtBearerOptions>((_, options) => capturedOptions = options)
            .Returns(true);

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.True(capturedOptions.SaveToken);
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyConfigurations_RemovesAllSchemes()
    {
        // Arrange
        _schemes.Add(new AuthenticationScheme("Scheme1", "Scheme1", typeof(JwtBearerHandler)));
        _schemes.Add(new AuthenticationScheme("Scheme2", "Scheme2", typeof(JwtBearerHandler)));

        var configurations = new Dictionary<string, JwtBearerConfiguration>();

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        _mockSchemeProvider.Verify(x => x.RemoveScheme("Scheme1"), Times.Once);
        _mockSchemeProvider.Verify(x => x.RemoveScheme("Scheme2"), Times.Once);
        // Verify policies were removed
        Assert.Null(await _policyProvider.GetPolicyAsync("Scheme1"));
        Assert.Null(await _policyProvider.GetPolicyAsync("Scheme2"));
    }

    [Fact]
    public async Task UpdateAsync_OnlyRemovesJwtBearerSchemes()
    {
        // Arrange
        _schemes.Add(new AuthenticationScheme("JwtScheme", "JwtScheme", typeof(JwtBearerHandler)));
        // Use a valid auth handler type
        _schemes.Add(new AuthenticationScheme("OtherScheme", "OtherScheme", typeof(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationHandler)));

        var configurations = new Dictionary<string, JwtBearerConfiguration>();

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        _mockSchemeProvider.Verify(x => x.RemoveScheme("JwtScheme"), Times.Once);
        _mockSchemeProvider.Verify(x => x.RemoveScheme("OtherScheme"), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithComplexConfiguration_ConfiguresAllProperties()
    {
        // Arrange
        var configurations = new Dictionary<string, JwtBearerConfiguration>
        {
            ["ComplexScheme"] = new()
            {
                Authority = "https://auth.example.com",
                Audience = "api1",
                RequireHttpsMetadata = false,
                SaveToken = true,
                ValidIssuers = new List<string> { "issuer1", "issuer2" },
                ValidAudiences = new List<string> { "audience1", "audience2" },
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = 60
            }
        };

        JwtBearerOptions? capturedOptions = null;
        _mockOptionsCache
            .Setup(x => x.TryAdd(It.IsAny<string>(), It.IsAny<JwtBearerOptions>()))
            .Callback<string, JwtBearerOptions>((_, options) => capturedOptions = options)
            .Returns(true);

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.Equal("https://auth.example.com", capturedOptions.Authority);
        Assert.Equal("api1", capturedOptions.Audience);
        Assert.False(capturedOptions.RequireHttpsMetadata);
        Assert.True(capturedOptions.SaveToken);
        Assert.Equal(TimeSpan.FromMinutes(1), capturedOptions.TokenValidationParameters.ClockSkew);
        Assert.True(capturedOptions.TokenValidationParameters.ValidateIssuer);
        Assert.True(capturedOptions.TokenValidationParameters.ValidateAudience);
    }
}

