using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Sail.Core.Authentication;

namespace Sail.Core.Tests.Authentication;

public class SailAuthorizationPolicyProviderTests
{
    private readonly SailAuthorizationPolicyProvider _provider;
    private readonly IOptions<AuthorizationOptions> _options;

    public SailAuthorizationPolicyProviderTests()
    {
        _options = Microsoft.Extensions.Options.Options.Create(new AuthorizationOptions());
        _provider = new SailAuthorizationPolicyProvider(_options);
    }

    [Fact]
    public async Task GetPolicyAsync_WithNonExistentPolicy_ReturnsNull()
    {
        // Arrange
        var policyName = "NonExistentPolicy";

        // Act
        var result = await _provider.GetPolicyAsync(policyName);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddOrUpdatePolicy_NewPolicy_AddsPolicy()
    {
        // Arrange
        var policyName = "TestPolicy";
        var schemeName = "TestScheme";

        // Act
        _provider.AddOrUpdatePolicy(policyName, schemeName);
        var result = await _provider.GetPolicyAsync(policyName);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.AuthenticationSchemes);
        Assert.Contains(schemeName, result.AuthenticationSchemes);
        Assert.NotEmpty(result.Requirements);
    }

    [Fact]
    public async Task AddOrUpdatePolicy_ExistingPolicy_UpdatesPolicy()
    {
        // Arrange
        var policyName = "TestPolicy";
        var firstSchemeName = "FirstScheme";
        var secondSchemeName = "SecondScheme";

        // Act
        _provider.AddOrUpdatePolicy(policyName, firstSchemeName);
        _provider.AddOrUpdatePolicy(policyName, secondSchemeName);
        var result = await _provider.GetPolicyAsync(policyName);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.AuthenticationSchemes);
        Assert.Contains(secondSchemeName, result.AuthenticationSchemes);
        Assert.DoesNotContain(firstSchemeName, result.AuthenticationSchemes);
    }

    [Fact]
    public async Task AddOrUpdatePolicy_MultiplePolicies_AllAccessible()
    {
        // Arrange
        var policy1Name = "Policy1";
        var policy2Name = "Policy2";
        var policy3Name = "Policy3";
        var scheme1 = "Scheme1";
        var scheme2 = "Scheme2";
        var scheme3 = "Scheme3";

        // Act
        _provider.AddOrUpdatePolicy(policy1Name, scheme1);
        _provider.AddOrUpdatePolicy(policy2Name, scheme2);
        _provider.AddOrUpdatePolicy(policy3Name, scheme3);

        var result1 = await _provider.GetPolicyAsync(policy1Name);
        var result2 = await _provider.GetPolicyAsync(policy2Name);
        var result3 = await _provider.GetPolicyAsync(policy3Name);

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotNull(result3);
        Assert.Contains(scheme1, result1.AuthenticationSchemes);
        Assert.Contains(scheme2, result2.AuthenticationSchemes);
        Assert.Contains(scheme3, result3.AuthenticationSchemes);
    }

    [Fact]
    public async Task RemovePolicy_ExistingPolicy_RemovesPolicy()
    {
        // Arrange
        var policyName = "TestPolicy";
        var schemeName = "TestScheme";
        _provider.AddOrUpdatePolicy(policyName, schemeName);

        // Act
        _provider.RemovePolicy(policyName);
        var result = await _provider.GetPolicyAsync(policyName);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RemovePolicy_NonExistentPolicy_DoesNotThrow()
    {
        // Arrange
        var policyName = "NonExistentPolicy";

        // Act & Assert
        var exception = Record.Exception(() => _provider.RemovePolicy(policyName));
        Assert.Null(exception);
        
        var result = await _provider.GetPolicyAsync(policyName);
        Assert.Null(result);
    }

    [Fact]
    public async Task RemovePolicy_OnePolicyOfMany_OnlyRemovesSpecified()
    {
        // Arrange
        var policy1Name = "Policy1";
        var policy2Name = "Policy2";
        var policy3Name = "Policy3";
        _provider.AddOrUpdatePolicy(policy1Name, "Scheme1");
        _provider.AddOrUpdatePolicy(policy2Name, "Scheme2");
        _provider.AddOrUpdatePolicy(policy3Name, "Scheme3");

        // Act
        _provider.RemovePolicy(policy2Name);

        var result1 = await _provider.GetPolicyAsync(policy1Name);
        var result2 = await _provider.GetPolicyAsync(policy2Name);
        var result3 = await _provider.GetPolicyAsync(policy3Name);

        // Assert
        Assert.NotNull(result1);
        Assert.Null(result2);
        Assert.NotNull(result3);
    }

    [Fact]
    public async Task GetDefaultPolicyAsync_ReturnsDefaultPolicy()
    {
        // Act
        var result = await _provider.GetDefaultPolicyAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Requirements);
    }

    [Fact]
    public async Task GetFallbackPolicyAsync_ReturnsNull()
    {
        // Act
        var result = await _provider.GetFallbackPolicyAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddOrUpdatePolicy_PolicyRequiresAuthenticatedUser()
    {
        // Arrange
        var policyName = "AuthPolicy";
        var schemeName = "TestScheme";

        // Act
        _provider.AddOrUpdatePolicy(policyName, schemeName);
        var result = await _provider.GetPolicyAsync(policyName);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Requirements);
        // The policy should have authentication requirements
        Assert.True(result.Requirements.Any());
    }

    [Fact]
    public async Task AddOrUpdatePolicy_WithDifferentSchemes_CreatesDistinctPolicies()
    {
        // Arrange
        var jwtPolicyName = "JwtPolicy";
        var oidcPolicyName = "OidcPolicy";
        var jwtScheme = "Bearer";
        var oidcScheme = "OpenIdConnect";

        // Act
        _provider.AddOrUpdatePolicy(jwtPolicyName, jwtScheme);
        _provider.AddOrUpdatePolicy(oidcPolicyName, oidcScheme);

        var jwtResult = await _provider.GetPolicyAsync(jwtPolicyName);
        var oidcResult = await _provider.GetPolicyAsync(oidcPolicyName);

        // Assert
        Assert.NotNull(jwtResult);
        Assert.NotNull(oidcResult);
        Assert.Contains(jwtScheme, jwtResult.AuthenticationSchemes);
        Assert.Contains(oidcScheme, oidcResult.AuthenticationSchemes);
        Assert.DoesNotContain(oidcScheme, jwtResult.AuthenticationSchemes);
        Assert.DoesNotContain(jwtScheme, oidcResult.AuthenticationSchemes);
    }

    [Fact]
    public async Task AddOrUpdatePolicy_CalledMultipleTimes_UpdatesPolicy()
    {
        // Arrange
        var policyName = "UpdatablePolicy";
        var schemes = new[] { "Scheme1", "Scheme2", "Scheme3" };

        // Act & Assert
        foreach (var scheme in schemes)
        {
            _provider.AddOrUpdatePolicy(policyName, scheme);
            var result = await _provider.GetPolicyAsync(policyName);
            
            Assert.NotNull(result);
            Assert.Contains(scheme, result.AuthenticationSchemes);
        }

        // Final check - should have the last scheme
        var finalResult = await _provider.GetPolicyAsync(policyName);
        Assert.Contains("Scheme3", finalResult!.AuthenticationSchemes);
    }

    [Fact]
    public async Task GetPolicyAsync_AfterAddAndRemove_ReturnsNull()
    {
        // Arrange
        var policyName = "TemporaryPolicy";
        var schemeName = "TempScheme";

        // Act
        _provider.AddOrUpdatePolicy(policyName, schemeName);
        var resultBeforeRemove = await _provider.GetPolicyAsync(policyName);
        
        _provider.RemovePolicy(policyName);
        var resultAfterRemove = await _provider.GetPolicyAsync(policyName);

        // Assert
        Assert.NotNull(resultBeforeRemove);
        Assert.Null(resultAfterRemove);
    }

    [Fact]
    public async Task AddOrUpdatePolicy_WithEmptyPolicyName_DoesNotThrow()
    {
        // Arrange
        var policyName = string.Empty;
        var schemeName = "TestScheme";

        // Act
        _provider.AddOrUpdatePolicy(policyName, schemeName);
        var result = await _provider.GetPolicyAsync(policyName);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task AddOrUpdatePolicy_WithEmptySchemeName_CreatesPolicy()
    {
        // Arrange
        var policyName = "PolicyWithEmptyScheme";
        var schemeName = string.Empty;

        // Act
        _provider.AddOrUpdatePolicy(policyName, schemeName);
        var result = await _provider.GetPolicyAsync(policyName);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.AuthenticationSchemes);
        Assert.Contains(string.Empty, result.AuthenticationSchemes);
    }

    [Fact]
    public async Task ConcurrentAddOrUpdate_HandlesMultipleThreads()
    {
        // Arrange
        var tasks = new List<Task>();
        var policyCount = 100;

        // Act
        for (int i = 0; i < policyCount; i++)
        {
            var index = i;
            tasks.Add(Task.Run(() => _provider.AddOrUpdatePolicy($"Policy{index}", $"Scheme{index}")));
        }

        await Task.WhenAll(tasks);

        // Assert
        for (int i = 0; i < policyCount; i++)
        {
            var result = await _provider.GetPolicyAsync($"Policy{i}");
            Assert.NotNull(result);
            Assert.Contains($"Scheme{i}", result.AuthenticationSchemes);
        }
    }

    [Fact]
    public async Task ConcurrentRemove_HandlesMultipleThreads()
    {
        // Arrange
        var policyCount = 100;
        for (int i = 0; i < policyCount; i++)
        {
            _provider.AddOrUpdatePolicy($"Policy{i}", $"Scheme{i}");
        }

        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < policyCount; i++)
        {
            var index = i;
            tasks.Add(Task.Run(() => _provider.RemovePolicy($"Policy{index}")));
        }

        await Task.WhenAll(tasks);

        // Assert
        for (int i = 0; i < policyCount; i++)
        {
            var result = await _provider.GetPolicyAsync($"Policy{i}");
            Assert.Null(result);
        }
    }

    [Fact]
    public async Task GetPolicyAsync_FallsBackToDefaultProvider_WhenPolicyNotFound()
    {
        // Arrange
        var policyName = "NonExistentPolicy";

        // Act
        var result = await _provider.GetPolicyAsync(policyName);

        // Assert
        // Should return null as the default provider doesn't have this policy either
        Assert.Null(result);
    }
}

