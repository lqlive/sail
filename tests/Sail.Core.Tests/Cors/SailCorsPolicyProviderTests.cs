using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Sail.Core.Cors;

namespace Sail.Core.Tests.Cors;

public class SailCorsPolicyProviderTests
{
    private readonly Mock<ILogger<SailCorsPolicyProvider>> _mockLogger;
    private readonly SailCorsPolicyProvider _provider;

    public SailCorsPolicyProviderTests()
    {
        _mockLogger = new Mock<ILogger<SailCorsPolicyProvider>>();
        _provider = new SailCorsPolicyProvider(_mockLogger.Object);
    }

    [Fact]
    public async Task GetPolicyAsync_WithNullPolicyName_ReturnsNull()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        var result = await _provider.GetPolicyAsync(context, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPolicyAsync_WithEmptyPolicyName_ReturnsNull()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        var result = await _provider.GetPolicyAsync(context, string.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPolicyAsync_WhenPolicyNotFound_ReturnsNull()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var policyName = "NonExistentPolicy";

        // Act
        var result = await _provider.GetPolicyAsync(context, policyName);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPolicyAsync_WhenPolicyExists_ReturnsPolicy()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var policyName = "TestPolicy";
        var configs = new List<CorsPolicyConfig>
        {
            new()
            {
                Name = policyName,
                AllowOrigins = new List<string> { "https://example.com" },
                AllowMethods = new List<string> { "GET", "POST" },
                AllowHeaders = new List<string> { "Content-Type" },
                AllowCredentials = true
            }
        };

        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Act
        var result = await _provider.GetPolicyAsync(context, policyName);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Origins);
        Assert.Contains("https://example.com", result.Origins);
        Assert.Equal(2, result.Methods.Count);
        Assert.Contains("GET", result.Methods);
        Assert.Contains("POST", result.Methods);
        Assert.True(result.SupportsCredentials);
    }

    [Fact]
    public async Task GetPolicyAsync_IsCaseInsensitive()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var configs = new List<CorsPolicyConfig>
        {
            new()
            {
                Name = "TestPolicy",
                AllowOrigins = new List<string> { "https://example.com" }
            }
        };

        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Act
        var result1 = await _provider.GetPolicyAsync(context, "TestPolicy");
        var result2 = await _provider.GetPolicyAsync(context, "testpolicy");
        var result3 = await _provider.GetPolicyAsync(context, "TESTPOLICY");

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotNull(result3);
    }

    [Fact]
    public async Task UpdateAsync_WithAllowAnyOrigin_ConfiguresCorrectly()
    {
        // Arrange
        var configs = new List<CorsPolicyConfig>
        {
            new()
            {
                Name = "AnyOriginPolicy",
                AllowOrigins = new List<string> { "*" }
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Assert
        var context = new DefaultHttpContext();
        var policy = await _provider.GetPolicyAsync(context, "AnyOriginPolicy");
        Assert.NotNull(policy);
        Assert.True(policy.AllowAnyOrigin);
    }

    [Fact]
    public async Task UpdateAsync_WithNullOrigins_AllowsAnyOrigin()
    {
        // Arrange
        var configs = new List<CorsPolicyConfig>
        {
            new()
            {
                Name = "NullOriginsPolicy",
                AllowOrigins = null
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Assert
        var context = new DefaultHttpContext();
        var policy = await _provider.GetPolicyAsync(context, "NullOriginsPolicy");
        Assert.NotNull(policy);
        Assert.True(policy.AllowAnyOrigin);
    }

    [Fact]
    public async Task UpdateAsync_WithAllowAnyMethod_ConfiguresCorrectly()
    {
        // Arrange
        var configs = new List<CorsPolicyConfig>
        {
            new()
            {
                Name = "AnyMethodPolicy",
                AllowMethods = new List<string> { "*" }
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Assert
        var context = new DefaultHttpContext();
        var policy = await _provider.GetPolicyAsync(context, "AnyMethodPolicy");
        Assert.NotNull(policy);
        Assert.True(policy.AllowAnyMethod);
    }

    [Fact]
    public async Task UpdateAsync_WithAllowAnyHeader_ConfiguresCorrectly()
    {
        // Arrange
        var configs = new List<CorsPolicyConfig>
        {
            new()
            {
                Name = "AnyHeaderPolicy",
                AllowHeaders = new List<string> { "*" }
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Assert
        var context = new DefaultHttpContext();
        var policy = await _provider.GetPolicyAsync(context, "AnyHeaderPolicy");
        Assert.NotNull(policy);
        Assert.True(policy.AllowAnyHeader);
    }

    [Fact]
    public async Task UpdateAsync_WithAllowCredentials_ConfiguresCorrectly()
    {
        // Arrange
        var configs = new List<CorsPolicyConfig>
        {
            new()
            {
                Name = "CredentialsPolicy",
                AllowOrigins = new List<string> { "https://example.com" },
                AllowCredentials = true
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Assert
        var context = new DefaultHttpContext();
        var policy = await _provider.GetPolicyAsync(context, "CredentialsPolicy");
        Assert.NotNull(policy);
        Assert.True(policy.SupportsCredentials);
    }

    [Fact]
    public async Task UpdateAsync_WithMaxAge_ConfiguresCorrectly()
    {
        // Arrange
        var maxAge = 3600;
        var configs = new List<CorsPolicyConfig>
        {
            new()
            {
                Name = "MaxAgePolicy",
                MaxAge = maxAge
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Assert
        var context = new DefaultHttpContext();
        var policy = await _provider.GetPolicyAsync(context, "MaxAgePolicy");
        Assert.NotNull(policy);
        Assert.Equal(TimeSpan.FromSeconds(maxAge), policy.PreflightMaxAge);
    }

    [Fact]
    public async Task UpdateAsync_WithExposeHeaders_ConfiguresCorrectly()
    {
        // Arrange
        var configs = new List<CorsPolicyConfig>
        {
            new()
            {
                Name = "ExposeHeadersPolicy",
                ExposeHeaders = new List<string> { "X-Custom-Header", "X-Another-Header" }
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Assert
        var context = new DefaultHttpContext();
        var policy = await _provider.GetPolicyAsync(context, "ExposeHeadersPolicy");
        Assert.NotNull(policy);
        Assert.Equal(2, policy.ExposedHeaders.Count);
        Assert.Contains("X-Custom-Header", policy.ExposedHeaders);
        Assert.Contains("X-Another-Header", policy.ExposedHeaders);
    }

    [Fact]
    public async Task UpdateAsync_WithMultiplePolicies_AllAvailable()
    {
        // Arrange
        var configs = new List<CorsPolicyConfig>
        {
            new()
            {
                Name = "Policy1",
                AllowOrigins = new List<string> { "https://example1.com" }
            },
            new()
            {
                Name = "Policy2",
                AllowOrigins = new List<string> { "https://example2.com" }
            },
            new()
            {
                Name = "Policy3",
                AllowOrigins = new List<string> { "https://example3.com" }
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Assert
        Assert.Equal(3, _provider.Policies.Count);
        var context = new DefaultHttpContext();
        Assert.NotNull(await _provider.GetPolicyAsync(context, "Policy1"));
        Assert.NotNull(await _provider.GetPolicyAsync(context, "Policy2"));
        Assert.NotNull(await _provider.GetPolicyAsync(context, "Policy3"));
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicatePolicyNames_OnlyFirstIsAdded()
    {
        // Arrange
        var configs = new List<CorsPolicyConfig>
        {
            new()
            {
                Name = "DuplicatePolicy",
                AllowOrigins = new List<string> { "https://first.com" }
            },
            new()
            {
                Name = "DuplicatePolicy",
                AllowOrigins = new List<string> { "https://second.com" }
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Assert
        Assert.Single(_provider.Policies);
        var context = new DefaultHttpContext();
        var policy = await _provider.GetPolicyAsync(context, "DuplicatePolicy");
        Assert.NotNull(policy);
        Assert.Contains("https://first.com", policy.Origins);
    }

    [Fact]
    public async Task UpdateAsync_ReplacesExistingPolicies()
    {
        // Arrange
        var initialConfigs = new List<CorsPolicyConfig>
        {
            new()
            {
                Name = "Policy1",
                AllowOrigins = new List<string> { "https://old.com" }
            }
        };

        var updatedConfigs = new List<CorsPolicyConfig>
        {
            new()
            {
                Name = "Policy2",
                AllowOrigins = new List<string> { "https://new.com" }
            }
        };

        // Act
        await _provider.UpdateAsync(initialConfigs, CancellationToken.None);
        await _provider.UpdateAsync(updatedConfigs, CancellationToken.None);

        // Assert
        Assert.Single(_provider.Policies);
        var context = new DefaultHttpContext();
        Assert.Null(await _provider.GetPolicyAsync(context, "Policy1"));
        Assert.NotNull(await _provider.GetPolicyAsync(context, "Policy2"));
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyConfigs_ClearsPolicies()
    {
        // Arrange
        var initialConfigs = new List<CorsPolicyConfig>
        {
            new()
            {
                Name = "Policy1",
                AllowOrigins = new List<string> { "https://example.com" }
            }
        };

        await _provider.UpdateAsync(initialConfigs, CancellationToken.None);

        // Act
        await _provider.UpdateAsync(new List<CorsPolicyConfig>(), CancellationToken.None);

        // Assert
        Assert.Empty(_provider.Policies);
    }

    [Fact]
    public async Task UpdateAsync_WithSamePolicies_NoChange()
    {
        // Arrange
        var configs = new List<CorsPolicyConfig>
        {
            new()
            {
                Name = "Policy1",
                AllowOrigins = new List<string> { "https://example.com" }
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);
        var policiesCountBefore = _provider.Policies.Count;
        await _provider.UpdateAsync(configs, CancellationToken.None);
        var policiesCountAfter = _provider.Policies.Count;

        // Assert
        Assert.Equal(policiesCountBefore, policiesCountAfter);
    }

    [Fact]
    public async Task UpdateAsync_WithComplexPolicy_ConfiguresAllOptions()
    {
        // Arrange
        var configs = new List<CorsPolicyConfig>
        {
            new()
            {
                Name = "ComplexPolicy",
                AllowOrigins = new List<string> { "https://example.com", "https://another.com" },
                AllowMethods = new List<string> { "GET", "POST", "PUT", "DELETE" },
                AllowHeaders = new List<string> { "Content-Type", "Authorization", "X-Custom-Header" },
                ExposeHeaders = new List<string> { "X-Total-Count", "X-Page-Size" },
                AllowCredentials = true,
                MaxAge = 7200
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Assert
        var context = new DefaultHttpContext();
        var policy = await _provider.GetPolicyAsync(context, "ComplexPolicy");
        
        Assert.NotNull(policy);
        Assert.Equal(2, policy.Origins.Count);
        Assert.Contains("https://example.com", policy.Origins);
        Assert.Contains("https://another.com", policy.Origins);
        Assert.Equal(4, policy.Methods.Count);
        Assert.Equal(3, policy.Headers.Count);
        Assert.Equal(2, policy.ExposedHeaders.Count);
        Assert.True(policy.SupportsCredentials);
        Assert.Equal(TimeSpan.FromSeconds(7200), policy.PreflightMaxAge);
    }

    [Fact]
    public async Task Policies_Property_ReturnsReadOnlyDictionary()
    {
        // Arrange
        var configs = new List<CorsPolicyConfig>
        {
            new()
            {
                Name = "TestPolicy",
                AllowOrigins = new List<string> { "https://example.com" }
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Assert
        Assert.IsAssignableFrom<IReadOnlyDictionary<string, Microsoft.AspNetCore.Cors.Infrastructure.CorsPolicy>>(_provider.Policies);
        Assert.Single(_provider.Policies);
    }
}
