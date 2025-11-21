using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Sail.Core.Authentication;
using Sail.Core.Authentication.OpenIdConnect;

namespace Sail.Core.Tests.Authentication.OpenIdConnect;

public class OpenIdConnectAuthenticationOptionsProviderTests
{
    private readonly Mock<IAuthenticationSchemeProvider> _mockSchemeProvider;
    private readonly Mock<IOptionsMonitorCache<OpenIdConnectOptions>> _mockOptionsCache;
    private readonly SailAuthorizationPolicyProvider _policyProvider;
    private readonly Mock<ILogger<OpenIdConnectAuthenticationOptionsProvider>> _mockLogger;
    private readonly OpenIdConnectAuthenticationOptionsProvider _provider;
    private readonly List<AuthenticationScheme> _schemes;

    public OpenIdConnectAuthenticationOptionsProviderTests()
    {
        _mockSchemeProvider = new Mock<IAuthenticationSchemeProvider>();
        _mockOptionsCache = new Mock<IOptionsMonitorCache<OpenIdConnectOptions>>();
        var authOptions = Microsoft.Extensions.Options.Options.Create(new Microsoft.AspNetCore.Authorization.AuthorizationOptions());
        _policyProvider = new SailAuthorizationPolicyProvider(authOptions);
        _mockLogger = new Mock<ILogger<OpenIdConnectAuthenticationOptionsProvider>>();
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
            .Setup(x => x.TryAdd(It.IsAny<string>(), It.IsAny<OpenIdConnectOptions>()))
            .Returns(true);

        _mockOptionsCache
            .Setup(x => x.TryRemove(It.IsAny<string>()));

        _provider = new OpenIdConnectAuthenticationOptionsProvider(
            _mockSchemeProvider.Object,
            _mockOptionsCache.Object,
            _policyProvider,
            _mockLogger.Object);
    }

    [Fact]
    public async Task UpdateAsync_WithSingleConfiguration_AddsScheme()
    {
        // Arrange
        var configurations = new Dictionary<string, OpenIdConnectConfiguration>
        {
            ["OidcScheme"] = new()
            {
                Authority = "https://auth.example.com",
                ClientId = "client-id",
                ClientSecret = "client-secret"
            }
        };

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        _mockSchemeProvider.Verify(x => x.AddScheme(It.Is<AuthenticationScheme>(
            s => s.Name == "OidcScheme" && s.HandlerType == typeof(OpenIdConnectHandler)
        )), Times.Once);
        _mockOptionsCache.Verify(x => x.TryAdd("OidcScheme", It.IsAny<OpenIdConnectOptions>()), Times.Once);
        // Verify policy was added by checking we can retrieve it
        var policy = await _policyProvider.GetPolicyAsync("OidcScheme");
        Assert.NotNull(policy);
    }

    [Fact]
    public async Task UpdateAsync_WithMultipleConfigurations_AddsAllSchemes()
    {
        // Arrange
        var configurations = new Dictionary<string, OpenIdConnectConfiguration>
        {
            ["Scheme1"] = new()
            {
                Authority = "https://auth1.example.com",
                ClientId = "client1",
                ClientSecret = "secret1"
            },
            ["Scheme2"] = new()
            {
                Authority = "https://auth2.example.com",
                ClientId = "client2",
                ClientSecret = "secret2"
            },
            ["Scheme3"] = new()
            {
                Authority = "https://auth3.example.com",
                ClientId = "client3",
                ClientSecret = "secret3"
            }
        };

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        _mockSchemeProvider.Verify(x => x.AddScheme(It.IsAny<AuthenticationScheme>()), Times.Exactly(3));
        _mockOptionsCache.Verify(x => x.TryAdd(It.IsAny<string>(), It.IsAny<OpenIdConnectOptions>()), Times.Exactly(3));
        // Verify policies were added
        Assert.NotNull(await _policyProvider.GetPolicyAsync("Scheme1"));
        Assert.NotNull(await _policyProvider.GetPolicyAsync("Scheme2"));
        Assert.NotNull(await _policyProvider.GetPolicyAsync("Scheme3"));
    }

    [Fact]
    public async Task UpdateAsync_WithExistingScheme_UpdatesScheme()
    {
        // Arrange
        _schemes.Add(new AuthenticationScheme("ExistingScheme", "ExistingScheme", typeof(OpenIdConnectHandler)));

        var configurations = new Dictionary<string, OpenIdConnectConfiguration>
        {
            ["ExistingScheme"] = new()
            {
                Authority = "https://auth.example.com",
                ClientId = "client-id",
                ClientSecret = "client-secret"
            }
        };

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        _mockSchemeProvider.Verify(x => x.AddScheme(It.IsAny<AuthenticationScheme>()), Times.Never);
        _mockOptionsCache.Verify(x => x.TryRemove("ExistingScheme"), Times.Once);
        _mockOptionsCache.Verify(x => x.TryAdd("ExistingScheme", It.IsAny<OpenIdConnectOptions>()), Times.Once);
        // Verify policy still exists
        Assert.NotNull(await _policyProvider.GetPolicyAsync("ExistingScheme"));
    }

    [Fact]
    public async Task UpdateAsync_RemovesObsoleteSchemes()
    {
        // Arrange
        _schemes.Add(new AuthenticationScheme("OldScheme", "OldScheme", typeof(OpenIdConnectHandler)));
        _schemes.Add(new AuthenticationScheme("AnotherOldScheme", "AnotherOldScheme", typeof(OpenIdConnectHandler)));

        var configurations = new Dictionary<string, OpenIdConnectConfiguration>
        {
            ["NewScheme"] = new()
            {
                Authority = "https://auth.example.com",
                ClientId = "client-id",
                ClientSecret = "client-secret"
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
    public async Task UpdateAsync_WithCustomScopes_ConfiguresOptions()
    {
        // Arrange
        var configurations = new Dictionary<string, OpenIdConnectConfiguration>
        {
            ["OidcScheme"] = new()
            {
                Authority = "https://auth.example.com",
                ClientId = "client-id",
                ClientSecret = "client-secret",
                Scope = new List<string> { "openid", "profile", "email", "api" }
            }
        };

        OpenIdConnectOptions? capturedOptions = null;
        _mockOptionsCache
            .Setup(x => x.TryAdd(It.IsAny<string>(), It.IsAny<OpenIdConnectOptions>()))
            .Callback<string, OpenIdConnectOptions>((_, options) => capturedOptions = options)
            .Returns(true);

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.Equal(4, capturedOptions.Scope.Count);
        Assert.Contains("openid", capturedOptions.Scope);
        Assert.Contains("profile", capturedOptions.Scope);
        Assert.Contains("email", capturedOptions.Scope);
        Assert.Contains("api", capturedOptions.Scope);
    }

    [Fact]
    public async Task UpdateAsync_WithoutScopes_DoesNotClearDefaultScopes()
    {
        // Arrange
        var configurations = new Dictionary<string, OpenIdConnectConfiguration>
        {
            ["OidcScheme"] = new()
            {
                Authority = "https://auth.example.com",
                ClientId = "client-id",
                ClientSecret = "client-secret",
                Scope = null
            }
        };

        OpenIdConnectOptions? capturedOptions = null;
        _mockOptionsCache
            .Setup(x => x.TryAdd(It.IsAny<string>(), It.IsAny<OpenIdConnectOptions>()))
            .Callback<string, OpenIdConnectOptions>((_, options) => capturedOptions = options)
            .Returns(true);

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOptions);
        // Default scopes should remain
        Assert.NotEmpty(capturedOptions.Scope);
    }

    [Fact]
    public async Task UpdateAsync_WithCustomClockSkew_ConfiguresTokenValidation()
    {
        // Arrange
        var configurations = new Dictionary<string, OpenIdConnectConfiguration>
        {
            ["OidcScheme"] = new()
            {
                Authority = "https://auth.example.com",
                ClientId = "client-id",
                ClientSecret = "client-secret",
                ClockSkew = 180 // 3 minutes
            }
        };

        OpenIdConnectOptions? capturedOptions = null;
        _mockOptionsCache
            .Setup(x => x.TryAdd(It.IsAny<string>(), It.IsAny<OpenIdConnectOptions>()))
            .Callback<string, OpenIdConnectOptions>((_, options) => capturedOptions = options)
            .Returns(true);

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.NotNull(capturedOptions.TokenValidationParameters);
        Assert.Equal(TimeSpan.FromMinutes(3), capturedOptions.TokenValidationParameters.ClockSkew);
    }

    [Fact]
    public async Task UpdateAsync_WithoutClockSkew_UsesDefaultTokenValidationParameters()
    {
        // Arrange
        var configurations = new Dictionary<string, OpenIdConnectConfiguration>
        {
            ["OidcScheme"] = new()
            {
                Authority = "https://auth.example.com",
                ClientId = "client-id",
                ClientSecret = "client-secret"
            }
        };

        OpenIdConnectOptions? capturedOptions = null;
        _mockOptionsCache
            .Setup(x => x.TryAdd(It.IsAny<string>(), It.IsAny<OpenIdConnectOptions>()))
            .Callback<string, OpenIdConnectOptions>((_, options) => capturedOptions = options)
            .Returns(true);

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOptions);
        // OpenIdConnectOptions always has default TokenValidationParameters
        Assert.NotNull(capturedOptions.TokenValidationParameters);
        // Default ClockSkew is 5 minutes
        Assert.Equal(TimeSpan.FromMinutes(5), capturedOptions.TokenValidationParameters.ClockSkew);
    }

    [Fact]
    public async Task UpdateAsync_WithCustomResponseType_ConfiguresOptions()
    {
        // Arrange
        var configurations = new Dictionary<string, OpenIdConnectConfiguration>
        {
            ["OidcScheme"] = new()
            {
                Authority = "https://auth.example.com",
                ClientId = "client-id",
                ClientSecret = "client-secret",
                ResponseType = "id_token token"
            }
        };

        OpenIdConnectOptions? capturedOptions = null;
        _mockOptionsCache
            .Setup(x => x.TryAdd(It.IsAny<string>(), It.IsAny<OpenIdConnectOptions>()))
            .Callback<string, OpenIdConnectOptions>((_, options) => capturedOptions = options)
            .Returns(true);

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.Equal("id_token token", capturedOptions.ResponseType);
    }

    [Fact]
    public async Task UpdateAsync_WithoutResponseType_UsesDefaultCode()
    {
        // Arrange
        var configurations = new Dictionary<string, OpenIdConnectConfiguration>
        {
            ["OidcScheme"] = new()
            {
                Authority = "https://auth.example.com",
                ClientId = "client-id",
                ClientSecret = "client-secret"
            }
        };

        OpenIdConnectOptions? capturedOptions = null;
        _mockOptionsCache
            .Setup(x => x.TryAdd(It.IsAny<string>(), It.IsAny<OpenIdConnectOptions>()))
            .Callback<string, OpenIdConnectOptions>((_, options) => capturedOptions = options)
            .Returns(true);

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.Equal("code", capturedOptions.ResponseType);
    }

    [Fact]
    public async Task UpdateAsync_WithRequireHttpsMetadata_ConfiguresOptions()
    {
        // Arrange
        var configurations = new Dictionary<string, OpenIdConnectConfiguration>
        {
            ["OidcScheme"] = new()
            {
                Authority = "http://localhost:5000", // HTTP for dev
                ClientId = "client-id",
                ClientSecret = "client-secret",
                RequireHttpsMetadata = false
            }
        };

        OpenIdConnectOptions? capturedOptions = null;
        _mockOptionsCache
            .Setup(x => x.TryAdd(It.IsAny<string>(), It.IsAny<OpenIdConnectOptions>()))
            .Callback<string, OpenIdConnectOptions>((_, options) => capturedOptions = options)
            .Returns(true);

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.False(capturedOptions.RequireHttpsMetadata);
    }

    [Fact]
    public async Task UpdateAsync_WithSaveTokens_ConfiguresOptions()
    {
        // Arrange
        var configurations = new Dictionary<string, OpenIdConnectConfiguration>
        {
            ["OidcScheme"] = new()
            {
                Authority = "https://auth.example.com",
                ClientId = "client-id",
                ClientSecret = "client-secret",
                SaveTokens = false
            }
        };

        OpenIdConnectOptions? capturedOptions = null;
        _mockOptionsCache
            .Setup(x => x.TryAdd(It.IsAny<string>(), It.IsAny<OpenIdConnectOptions>()))
            .Callback<string, OpenIdConnectOptions>((_, options) => capturedOptions = options)
            .Returns(true);

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.False(capturedOptions.SaveTokens);
    }

    [Fact]
    public async Task UpdateAsync_WithGetClaimsFromUserInfoEndpoint_ConfiguresOptions()
    {
        // Arrange
        var configurations = new Dictionary<string, OpenIdConnectConfiguration>
        {
            ["OidcScheme"] = new()
            {
                Authority = "https://auth.example.com",
                ClientId = "client-id",
                ClientSecret = "client-secret",
                GetClaimsFromUserInfoEndpoint = false
            }
        };

        OpenIdConnectOptions? capturedOptions = null;
        _mockOptionsCache
            .Setup(x => x.TryAdd(It.IsAny<string>(), It.IsAny<OpenIdConnectOptions>()))
            .Callback<string, OpenIdConnectOptions>((_, options) => capturedOptions = options)
            .Returns(true);

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.False(capturedOptions.GetClaimsFromUserInfoEndpoint);
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyConfigurations_RemovesAllSchemes()
    {
        // Arrange
        _schemes.Add(new AuthenticationScheme("Scheme1", "Scheme1", typeof(OpenIdConnectHandler)));
        _schemes.Add(new AuthenticationScheme("Scheme2", "Scheme2", typeof(OpenIdConnectHandler)));

        var configurations = new Dictionary<string, OpenIdConnectConfiguration>();

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
    public async Task UpdateAsync_OnlyRemovesOpenIdConnectSchemes()
    {
        // Arrange
        _schemes.Add(new AuthenticationScheme("OidcScheme", "OidcScheme", typeof(OpenIdConnectHandler)));
        // Use a valid auth handler type
        _schemes.Add(new AuthenticationScheme("OtherScheme", "OtherScheme", typeof(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationHandler)));

        var configurations = new Dictionary<string, OpenIdConnectConfiguration>();

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        _mockSchemeProvider.Verify(x => x.RemoveScheme("OidcScheme"), Times.Once);
        _mockSchemeProvider.Verify(x => x.RemoveScheme("OtherScheme"), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithComplexConfiguration_ConfiguresAllProperties()
    {
        // Arrange
        var configurations = new Dictionary<string, OpenIdConnectConfiguration>
        {
            ["ComplexScheme"] = new()
            {
                Authority = "https://auth.example.com",
                ClientId = "complex-client",
                ClientSecret = "complex-secret",
                ResponseType = "code id_token",
                RequireHttpsMetadata = false,
                SaveTokens = false,
                GetClaimsFromUserInfoEndpoint = false,
                Scope = new List<string> { "openid", "profile", "email", "api", "offline_access" },
                ClockSkew = 300
            }
        };

        OpenIdConnectOptions? capturedOptions = null;
        _mockOptionsCache
            .Setup(x => x.TryAdd(It.IsAny<string>(), It.IsAny<OpenIdConnectOptions>()))
            .Callback<string, OpenIdConnectOptions>((_, options) => capturedOptions = options)
            .Returns(true);

        // Act
        await _provider.UpdateAsync(configurations, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.Equal("https://auth.example.com", capturedOptions.Authority);
        Assert.Equal("complex-client", capturedOptions.ClientId);
        Assert.Equal("complex-secret", capturedOptions.ClientSecret);
        Assert.Equal("code id_token", capturedOptions.ResponseType);
        Assert.False(capturedOptions.RequireHttpsMetadata);
        Assert.False(capturedOptions.SaveTokens);
        Assert.False(capturedOptions.GetClaimsFromUserInfoEndpoint);
        Assert.Equal(5, capturedOptions.Scope.Count);
        Assert.NotNull(capturedOptions.TokenValidationParameters);
        Assert.Equal(TimeSpan.FromMinutes(5), capturedOptions.TokenValidationParameters.ClockSkew);
    }
}

