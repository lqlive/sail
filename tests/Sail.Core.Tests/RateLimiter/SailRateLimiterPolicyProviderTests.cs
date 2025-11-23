using Microsoft.Extensions.Logging;
using Moq;
using Sail.Core.RateLimiter;

namespace Sail.Core.Tests.RateLimiter;

public class SailRateLimiterPolicyProviderTests
{
    private readonly Mock<ILogger<SailRateLimiterPolicyProvider>> _mockLogger;
    private readonly SailRateLimiterPolicyProvider _provider;

    public SailRateLimiterPolicyProviderTests()
    {
        _mockLogger = new Mock<ILogger<SailRateLimiterPolicyProvider>>();
        _provider = new SailRateLimiterPolicyProvider(_mockLogger.Object);
    }

    [Fact]
    public void GetPolicy_WithNullKey_ReturnsNull()
    {
        // Act
        var result = _provider.GetPolicy(null!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetPolicy_WithEmptyKey_ReturnsNull()
    {
        // Act
        var result = _provider.GetPolicy(string.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetPolicy_WithNonExistentKey_ReturnsNull()
    {
        // Act
        var result = _provider.GetPolicy("NonExistentPolicy");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPolicy_AfterUpdate_ReturnsPolicy()
    {
        // Arrange
        var configs = new List<RateLimiterPolicyConfig>
        {
            new()
            {
                Name = "TestPolicy",
                PermitLimit = 100,
                Window = 60,
                QueueLimit = 10
            }
        };

        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Act
        var result = _provider.GetPolicy("TestPolicy");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestPolicy", result.Name);
        Assert.Equal(100, result.PermitLimit);
        Assert.Equal(60, result.Window);
        Assert.Equal(10, result.QueueLimit);
    }

    [Fact]
    public async Task GetPolicy_IsCaseInsensitive()
    {
        // Arrange
        var configs = new List<RateLimiterPolicyConfig>
        {
            new()
            {
                Name = "TestPolicy",
                PermitLimit = 100,
                Window = 60,
                QueueLimit = 10
            }
        };

        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Act
        var result1 = _provider.GetPolicy("TestPolicy");
        var result2 = _provider.GetPolicy("testpolicy");
        var result3 = _provider.GetPolicy("TESTPOLICY");

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotNull(result3);
        Assert.Equal(result1.Name, result2.Name);
        Assert.Equal(result2.Name, result3.Name);
    }

    [Fact]
    public async Task UpdateAsync_WithValidConfig_AddsPolicies()
    {
        // Arrange
        var configs = new List<RateLimiterPolicyConfig>
        {
            new()
            {
                Name = "Policy1",
                PermitLimit = 100,
                Window = 60,
                QueueLimit = 10
            },
            new()
            {
                Name = "Policy2",
                PermitLimit = 50,
                Window = 30,
                QueueLimit = 5
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Assert
        Assert.Equal(2, _provider.Policies.Count);
        Assert.NotNull(_provider.GetPolicy("Policy1"));
        Assert.NotNull(_provider.GetPolicy("Policy2"));
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidConfig_SkipsInvalid()
    {
        // Arrange
        var configs = new List<RateLimiterPolicyConfig>
        {
            new()
            {
                Name = "ValidPolicy",
                PermitLimit = 100,
                Window = 60,
                QueueLimit = 10
            },
            new()
            {
                Name = "InvalidPolicy1",
                PermitLimit = 0, // Invalid: must be > 0
                Window = 60,
                QueueLimit = 10
            },
            new()
            {
                Name = "InvalidPolicy2",
                PermitLimit = 100,
                Window = 0, // Invalid: must be > 0
                QueueLimit = 10
            },
            new()
            {
                Name = "InvalidPolicy3",
                PermitLimit = 100,
                Window = 60,
                QueueLimit = -1 // Invalid: must be >= 0
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Assert
        Assert.Single(_provider.Policies);
        Assert.NotNull(_provider.GetPolicy("ValidPolicy"));
        Assert.Null(_provider.GetPolicy("InvalidPolicy1"));
        Assert.Null(_provider.GetPolicy("InvalidPolicy2"));
        Assert.Null(_provider.GetPolicy("InvalidPolicy3"));
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateNames_OnlyFirstIsAdded()
    {
        // Arrange
        var configs = new List<RateLimiterPolicyConfig>
        {
            new()
            {
                Name = "DuplicatePolicy",
                PermitLimit = 100,
                Window = 60,
                QueueLimit = 10
            },
            new()
            {
                Name = "DuplicatePolicy",
                PermitLimit = 200,
                Window = 120,
                QueueLimit = 20
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Assert
        Assert.Single(_provider.Policies);
        var policy = _provider.GetPolicy("DuplicatePolicy");
        Assert.NotNull(policy);
        Assert.Equal(100, policy.PermitLimit); // First one
    }

    [Fact]
    public async Task UpdateAsync_ReplacesExistingPolicies()
    {
        // Arrange
        var initialConfigs = new List<RateLimiterPolicyConfig>
        {
            new()
            {
                Name = "Policy1",
                PermitLimit = 100,
                Window = 60,
                QueueLimit = 10
            }
        };

        var updatedConfigs = new List<RateLimiterPolicyConfig>
        {
            new()
            {
                Name = "Policy2",
                PermitLimit = 200,
                Window = 120,
                QueueLimit = 20
            }
        };

        // Act
        await _provider.UpdateAsync(initialConfigs, CancellationToken.None);
        await _provider.UpdateAsync(updatedConfigs, CancellationToken.None);

        // Assert
        Assert.Single(_provider.Policies);
        Assert.Null(_provider.GetPolicy("Policy1"));
        Assert.NotNull(_provider.GetPolicy("Policy2"));
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyConfigs_ClearsPolicies()
    {
        // Arrange
        var initialConfigs = new List<RateLimiterPolicyConfig>
        {
            new()
            {
                Name = "Policy1",
                PermitLimit = 100,
                Window = 60,
                QueueLimit = 10
            }
        };

        await _provider.UpdateAsync(initialConfigs, CancellationToken.None);

        // Act
        await _provider.UpdateAsync(new List<RateLimiterPolicyConfig>(), CancellationToken.None);

        // Assert
        Assert.Empty(_provider.Policies);
    }

    [Fact]
    public async Task UpdateAsync_WithSamePolicies_NoChange()
    {
        // Arrange
        var configs = new List<RateLimiterPolicyConfig>
        {
            new()
            {
                Name = "Policy1",
                PermitLimit = 100,
                Window = 60,
                QueueLimit = 10
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
    public async Task UpdateAsync_WithChangedValues_DetectsChange()
    {
        // Arrange
        var initialConfigs = new List<RateLimiterPolicyConfig>
        {
            new()
            {
                Name = "Policy1",
                PermitLimit = 100,
                Window = 60,
                QueueLimit = 10
            }
        };

        var updatedConfigs = new List<RateLimiterPolicyConfig>
        {
            new()
            {
                Name = "Policy1",
                PermitLimit = 200, // Changed
                Window = 60,
                QueueLimit = 10
            }
        };

        // Act
        await _provider.UpdateAsync(initialConfigs, CancellationToken.None);
        await _provider.UpdateAsync(updatedConfigs, CancellationToken.None);

        // Assert
        var policy = _provider.GetPolicy("Policy1");
        Assert.NotNull(policy);
        Assert.Equal(200, policy.PermitLimit);
    }

    [Fact]
    public async Task UpdateAsync_WithZeroQueueLimit_IsValid()
    {
        // Arrange
        var configs = new List<RateLimiterPolicyConfig>
        {
            new()
            {
                Name = "NoQueuePolicy",
                PermitLimit = 100,
                Window = 60,
                QueueLimit = 0 // Valid: can be 0
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Assert
        Assert.Single(_provider.Policies);
        var policy = _provider.GetPolicy("NoQueuePolicy");
        Assert.NotNull(policy);
        Assert.Equal(0, policy.QueueLimit);
    }

    [Fact]
    public async Task UpdateAsync_WithVariousPermitLimits_ConfiguresCorrectly()
    {
        // Arrange
        var configs = new List<RateLimiterPolicyConfig>
        {
            new()
            {
                Name = "VeryRestrictive",
                PermitLimit = 1,
                Window = 1,
                QueueLimit = 0
            },
            new()
            {
                Name = "Moderate",
                PermitLimit = 100,
                Window = 60,
                QueueLimit = 10
            },
            new()
            {
                Name = "Generous",
                PermitLimit = 10000,
                Window = 3600,
                QueueLimit = 1000
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Assert
        Assert.Equal(3, _provider.Policies.Count);
        Assert.Equal(1, _provider.GetPolicy("VeryRestrictive")!.PermitLimit);
        Assert.Equal(100, _provider.GetPolicy("Moderate")!.PermitLimit);
        Assert.Equal(10000, _provider.GetPolicy("Generous")!.PermitLimit);
    }

    [Fact]
    public async Task Policies_Property_ReturnsReadOnlyDictionary()
    {
        // Arrange
        var configs = new List<RateLimiterPolicyConfig>
        {
            new()
            {
                Name = "TestPolicy",
                PermitLimit = 100,
                Window = 60,
                QueueLimit = 10
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Assert
        Assert.IsAssignableFrom<IReadOnlyDictionary<string, RateLimiterPolicyConfig>>(_provider.Policies);
        Assert.Single(_provider.Policies);
    }

    [Fact]
    public async Task UpdateAsync_WithComplexScenario_HandlesCorrectly()
    {
        // Arrange
        var configs = new List<RateLimiterPolicyConfig>
        {
            new()
            {
                Name = "API_Standard",
                PermitLimit = 100,
                Window = 60,
                QueueLimit = 10
            },
            new()
            {
                Name = "API_Premium",
                PermitLimit = 1000,
                Window = 60,
                QueueLimit = 100
            },
            new()
            {
                Name = "API_Burst",
                PermitLimit = 50,
                Window = 1,
                QueueLimit = 0
            },
            new()
            {
                Name = "API_Invalid",
                PermitLimit = -1, // Invalid
                Window = 60,
                QueueLimit = 10
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Assert
        Assert.Equal(3, _provider.Policies.Count);
        Assert.NotNull(_provider.GetPolicy("API_Standard"));
        Assert.NotNull(_provider.GetPolicy("API_Premium"));
        Assert.NotNull(_provider.GetPolicy("API_Burst"));
        Assert.Null(_provider.GetPolicy("API_Invalid"));
    }

    [Fact]
    public async Task UpdateAsync_DetectsChangeInPermitLimit()
    {
        // Arrange
        var config1 = new List<RateLimiterPolicyConfig>
        {
            new() { Name = "Test", PermitLimit = 100, Window = 60, QueueLimit = 10 }
        };
        var config2 = new List<RateLimiterPolicyConfig>
        {
            new() { Name = "Test", PermitLimit = 200, Window = 60, QueueLimit = 10 }
        };

        // Act
        await _provider.UpdateAsync(config1, CancellationToken.None);
        var before = _provider.GetPolicy("Test")!.PermitLimit;

        await _provider.UpdateAsync(config2, CancellationToken.None);
        var after = _provider.GetPolicy("Test")!.PermitLimit;

        // Assert
        Assert.Equal(100, before);
        Assert.Equal(200, after);
    }

    [Fact]
    public async Task UpdateAsync_DetectsChangeInWindow()
    {
        // Arrange
        var config1 = new List<RateLimiterPolicyConfig>
        {
            new() { Name = "Test", PermitLimit = 100, Window = 60, QueueLimit = 10 }
        };
        var config2 = new List<RateLimiterPolicyConfig>
        {
            new() { Name = "Test", PermitLimit = 100, Window = 120, QueueLimit = 10 }
        };

        // Act
        await _provider.UpdateAsync(config1, CancellationToken.None);
        var before = _provider.GetPolicy("Test")!.Window;

        await _provider.UpdateAsync(config2, CancellationToken.None);
        var after = _provider.GetPolicy("Test")!.Window;

        // Assert
        Assert.Equal(60, before);
        Assert.Equal(120, after);
    }

    [Fact]
    public async Task UpdateAsync_DetectsChangeInQueueLimit()
    {
        // Arrange
        var config1 = new List<RateLimiterPolicyConfig>
        {
            new() { Name = "Test", PermitLimit = 100, Window = 60, QueueLimit = 10 }
        };
        var config2 = new List<RateLimiterPolicyConfig>
        {
            new() { Name = "Test", PermitLimit = 100, Window = 60, QueueLimit = 20 }
        };

        // Act
        await _provider.UpdateAsync(config1, CancellationToken.None);
        var before = _provider.GetPolicy("Test")!.QueueLimit;

        await _provider.UpdateAsync(config2, CancellationToken.None);
        var after = _provider.GetPolicy("Test")!.QueueLimit;

        // Assert
        Assert.Equal(10, before);
        Assert.Equal(20, after);
    }
}

