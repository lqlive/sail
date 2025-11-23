using Microsoft.Extensions.Logging;
using Moq;
using Sail.Core.Retry;

namespace Sail.Core.Tests.Retry;

public class SailRetryPolicyProviderTests
{
    private readonly Mock<ILogger<SailRetryPolicyProvider>> _mockLogger;
    private readonly SailRetryPolicyProvider _provider;

    public SailRetryPolicyProviderTests()
    {
        _mockLogger = new Mock<ILogger<SailRetryPolicyProvider>>();
        _provider = new SailRetryPolicyProvider(_mockLogger.Object);
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
        var configs = new List<RetryPolicyConfig>
        {
            new()
            {
                Name = "TestPolicy",
                MaxRetryAttempts = 3,
                RetryDelayMilliseconds = 1000,
                UseExponentialBackoff = false,
                RetryStatusCodes = [500, 502, 503]
            }
        };

        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Act
        var result = _provider.GetPolicy("TestPolicy");

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Config);
        Assert.NotNull(result.Pipeline);
        Assert.Equal("TestPolicy", result.Config.Name);
        Assert.Equal(3, result.Config.MaxRetryAttempts);
        Assert.Equal(1000, result.Config.RetryDelayMilliseconds);
        Assert.False(result.Config.UseExponentialBackoff);
    }

    [Fact]
    public async Task GetPolicy_IsCaseInsensitive()
    {
        // Arrange
        var configs = new List<RetryPolicyConfig>
        {
            new()
            {
                Name = "TestPolicy",
                MaxRetryAttempts = 3,
                RetryDelayMilliseconds = 1000,
                RetryStatusCodes = [500]
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
        Assert.Equal(result1.Config.Name, result2.Config.Name);
        Assert.Equal(result2.Config.Name, result3.Config.Name);
    }

    [Fact]
    public async Task UpdateAsync_WithValidConfig_AddsPolicies()
    {
        // Arrange
        var configs = new List<RetryPolicyConfig>
        {
            new()
            {
                Name = "Policy1",
                MaxRetryAttempts = 3,
                RetryDelayMilliseconds = 1000,
                RetryStatusCodes = [500, 502]
            },
            new()
            {
                Name = "Policy2",
                MaxRetryAttempts = 5,
                RetryDelayMilliseconds = 2000,
                UseExponentialBackoff = true,
                RetryStatusCodes = [500, 502, 503, 504]
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Assert
        Assert.NotNull(_provider.GetPolicy("Policy1"));
        Assert.NotNull(_provider.GetPolicy("Policy2"));
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateNames_OnlyFirstIsAdded()
    {
        // Arrange
        var configs = new List<RetryPolicyConfig>
        {
            new()
            {
                Name = "DuplicatePolicy",
                MaxRetryAttempts = 3,
                RetryDelayMilliseconds = 1000,
                RetryStatusCodes = [500]
            },
            new()
            {
                Name = "DuplicatePolicy",
                MaxRetryAttempts = 5,
                RetryDelayMilliseconds = 2000,
                RetryStatusCodes = [500, 502]
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Assert
        var policy = _provider.GetPolicy("DuplicatePolicy");
        Assert.NotNull(policy);
        Assert.Equal(3, policy.Config.MaxRetryAttempts); // First one
    }

    [Fact]
    public async Task UpdateAsync_ReplacesExistingPolicies()
    {
        // Arrange
        var initialConfigs = new List<RetryPolicyConfig>
        {
            new()
            {
                Name = "Policy1",
                MaxRetryAttempts = 3,
                RetryDelayMilliseconds = 1000,
                RetryStatusCodes = [500]
            }
        };

        var updatedConfigs = new List<RetryPolicyConfig>
        {
            new()
            {
                Name = "Policy2",
                MaxRetryAttempts = 5,
                RetryDelayMilliseconds = 2000,
                RetryStatusCodes = [500, 502]
            }
        };

        // Act
        await _provider.UpdateAsync(initialConfigs, CancellationToken.None);
        await _provider.UpdateAsync(updatedConfigs, CancellationToken.None);

        // Assert
        Assert.Null(_provider.GetPolicy("Policy1"));
        Assert.NotNull(_provider.GetPolicy("Policy2"));
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyConfigs_ClearsPolicies()
    {
        // Arrange
        var initialConfigs = new List<RetryPolicyConfig>
        {
            new()
            {
                Name = "Policy1",
                MaxRetryAttempts = 3,
                RetryDelayMilliseconds = 1000,
                RetryStatusCodes = [500]
            }
        };

        await _provider.UpdateAsync(initialConfigs, CancellationToken.None);

        // Act
        await _provider.UpdateAsync(new List<RetryPolicyConfig>(), CancellationToken.None);

        // Assert
        Assert.Null(_provider.GetPolicy("Policy1"));
    }

    [Fact]
    public async Task UpdateAsync_WithExponentialBackoff_CreatesCorrectPolicy()
    {
        // Arrange
        var configs = new List<RetryPolicyConfig>
        {
            new()
            {
                Name = "ExponentialPolicy",
                MaxRetryAttempts = 5,
                RetryDelayMilliseconds = 1000,
                UseExponentialBackoff = true,
                RetryStatusCodes = [500, 502, 503, 504]
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Assert
        var policy = _provider.GetPolicy("ExponentialPolicy");
        Assert.NotNull(policy);
        Assert.True(policy.Config.UseExponentialBackoff);
        Assert.Equal(5, policy.Config.MaxRetryAttempts);
    }

    [Fact]
    public async Task UpdateAsync_WithLinearBackoff_CreatesCorrectPolicy()
    {
        // Arrange
        var configs = new List<RetryPolicyConfig>
        {
            new()
            {
                Name = "LinearPolicy",
                MaxRetryAttempts = 3,
                RetryDelayMilliseconds = 2000,
                UseExponentialBackoff = false,
                RetryStatusCodes = [500, 502]
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Assert
        var policy = _provider.GetPolicy("LinearPolicy");
        Assert.NotNull(policy);
        Assert.False(policy.Config.UseExponentialBackoff);
        Assert.Equal(2000, policy.Config.RetryDelayMilliseconds);
    }

    [Fact]
    public async Task UpdateAsync_WithCustomStatusCodes_CreatesCorrectPolicy()
    {
        // Arrange
        var configs = new List<RetryPolicyConfig>
        {
            new()
            {
                Name = "CustomStatusPolicy",
                MaxRetryAttempts = 3,
                RetryDelayMilliseconds = 1000,
                RetryStatusCodes = [429, 500, 502, 503, 504]
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Assert
        var policy = _provider.GetPolicy("CustomStatusPolicy");
        Assert.NotNull(policy);
        Assert.Equal(5, policy.Config.RetryStatusCodes.Length);
        Assert.Contains(429, policy.Config.RetryStatusCodes);
    }

    [Fact]
    public async Task UpdateAsync_WithChangedValues_UpdatesPolicy()
    {
        // Arrange
        var initialConfigs = new List<RetryPolicyConfig>
        {
            new()
            {
                Name = "Policy1",
                MaxRetryAttempts = 3,
                RetryDelayMilliseconds = 1000,
                UseExponentialBackoff = false,
                RetryStatusCodes = [500]
            }
        };

        var updatedConfigs = new List<RetryPolicyConfig>
        {
            new()
            {
                Name = "Policy1",
                MaxRetryAttempts = 5,
                RetryDelayMilliseconds = 2000,
                UseExponentialBackoff = true,
                RetryStatusCodes = [500, 502, 503]
            }
        };

        // Act
        await _provider.UpdateAsync(initialConfigs, CancellationToken.None);
        await _provider.UpdateAsync(updatedConfigs, CancellationToken.None);

        // Assert
        var policy = _provider.GetPolicy("Policy1");
        Assert.NotNull(policy);
        Assert.Equal(5, policy.Config.MaxRetryAttempts);
        Assert.Equal(2000, policy.Config.RetryDelayMilliseconds);
        Assert.True(policy.Config.UseExponentialBackoff);
    }

    [Fact]
    public async Task UpdateAsync_WithMultiplePolicies_AllAccessible()
    {
        // Arrange
        var configs = new List<RetryPolicyConfig>
        {
            new()
            {
                Name = "QuickRetry",
                MaxRetryAttempts = 2,
                RetryDelayMilliseconds = 500,
                RetryStatusCodes = [500]
            },
            new()
            {
                Name = "StandardRetry",
                MaxRetryAttempts = 3,
                RetryDelayMilliseconds = 1000,
                RetryStatusCodes = [500, 502, 503]
            },
            new()
            {
                Name = "AggressiveRetry",
                MaxRetryAttempts = 10,
                RetryDelayMilliseconds = 100,
                UseExponentialBackoff = true,
                RetryStatusCodes = [500, 502, 503, 504]
            }
        };

        // Act
        await _provider.UpdateAsync(configs, CancellationToken.None);

        // Assert
        Assert.NotNull(_provider.GetPolicy("QuickRetry"));
        Assert.NotNull(_provider.GetPolicy("StandardRetry"));
        Assert.NotNull(_provider.GetPolicy("AggressiveRetry"));

        Assert.Equal(2, _provider.GetPolicy("QuickRetry")!.Config.MaxRetryAttempts);
        Assert.Equal(3, _provider.GetPolicy("StandardRetry")!.Config.MaxRetryAttempts);
        Assert.Equal(10, _provider.GetPolicy("AggressiveRetry")!.Config.MaxRetryAttempts);
    }
}

