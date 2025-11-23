using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Sail.Core.Timeout;

namespace Sail.Core.Tests.Timeout;

public class SailTimeoutPolicyProviderTests
{
    private readonly Mock<IOptionsMonitorCache<RequestTimeoutOptions>> _mockOptionsCache;
    private readonly Mock<ILogger<SailTimeoutPolicyProvider>> _mockLogger;
    private readonly SailTimeoutPolicyProvider _provider;
    private RequestTimeoutOptions? _cachedOptions;

    public SailTimeoutPolicyProviderTests()
    {
        _mockOptionsCache = new Mock<IOptionsMonitorCache<RequestTimeoutOptions>>();
        _mockLogger = new Mock<ILogger<SailTimeoutPolicyProvider>>();

        // Setup the cache to store options when TryAdd is called
        _mockOptionsCache
            .Setup(x => x.TryAdd(It.IsAny<string>(), It.IsAny<RequestTimeoutOptions>()))
            .Callback<string, RequestTimeoutOptions>((_, options) => _cachedOptions = options)
            .Returns(true);

        _provider = new SailTimeoutPolicyProvider(_mockOptionsCache.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task UpdateAsync_WithSinglePolicy_AddsToCache()
    {
        // Arrange
        var policies = new List<TimeoutPolicyConfig>
        {
            new()
            {
                Name = "ShortTimeout",
                Seconds = 30
            }
        };

        // Act
        await _provider.UpdateAsync(policies, CancellationToken.None);

        // Assert
        _mockOptionsCache.Verify(x => x.TryRemove(Microsoft.Extensions.Options.Options.DefaultName), Times.Once);
        _mockOptionsCache.Verify(x => x.TryAdd(Microsoft.Extensions.Options.Options.DefaultName, It.IsAny<RequestTimeoutOptions>()), Times.Once);

        Assert.NotNull(_cachedOptions);
        Assert.Single(_cachedOptions.Policies);
        Assert.True(_cachedOptions.Policies.ContainsKey("ShortTimeout"));
        Assert.Equal(TimeSpan.FromSeconds(30), _cachedOptions.Policies["ShortTimeout"].Timeout);
    }

    [Fact]
    public async Task UpdateAsync_WithMultiplePolicies_AddsAllToCache()
    {
        // Arrange
        var policies = new List<TimeoutPolicyConfig>
        {
            new()
            {
                Name = "ShortTimeout",
                Seconds = 30
            },
            new()
            {
                Name = "MediumTimeout",
                Seconds = 60
            },
            new()
            {
                Name = "LongTimeout",
                Seconds = 120
            }
        };

        // Act
        await _provider.UpdateAsync(policies, CancellationToken.None);

        // Assert
        Assert.NotNull(_cachedOptions);
        Assert.Equal(3, _cachedOptions.Policies.Count);
        Assert.True(_cachedOptions.Policies.ContainsKey("ShortTimeout"));
        Assert.True(_cachedOptions.Policies.ContainsKey("MediumTimeout"));
        Assert.True(_cachedOptions.Policies.ContainsKey("LongTimeout"));

        Assert.Equal(TimeSpan.FromSeconds(30), _cachedOptions.Policies["ShortTimeout"].Timeout);
        Assert.Equal(TimeSpan.FromSeconds(60), _cachedOptions.Policies["MediumTimeout"].Timeout);
        Assert.Equal(TimeSpan.FromSeconds(120), _cachedOptions.Policies["LongTimeout"].Timeout);
    }

    [Fact]
    public async Task UpdateAsync_WithTimeoutStatusCode_SetsStatusCode()
    {
        // Arrange
        var policies = new List<TimeoutPolicyConfig>
        {
            new()
            {
                Name = "CustomStatusTimeout",
                Seconds = 30,
                TimeoutStatusCode = 503
            }
        };

        // Act
        await _provider.UpdateAsync(policies, CancellationToken.None);

        // Assert
        Assert.NotNull(_cachedOptions);
        Assert.Single(_cachedOptions.Policies);
        Assert.Equal(503, _cachedOptions.Policies["CustomStatusTimeout"].TimeoutStatusCode);
    }

    [Fact]
    public async Task UpdateAsync_WithoutTimeoutStatusCode_UsesDefault()
    {
        // Arrange
        var policies = new List<TimeoutPolicyConfig>
        {
            new()
            {
                Name = "DefaultStatusTimeout",
                Seconds = 30,
                TimeoutStatusCode = null
            }
        };

        // Act
        await _provider.UpdateAsync(policies, CancellationToken.None);

        // Assert
        Assert.NotNull(_cachedOptions);
        Assert.Single(_cachedOptions.Policies);
        // The default TimeoutStatusCode should be null if not specified
        Assert.Null(_cachedOptions.Policies["DefaultStatusTimeout"].TimeoutStatusCode);
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyPolicies_CreatesEmptyOptions()
    {
        // Arrange
        var policies = new List<TimeoutPolicyConfig>();

        // Act
        await _provider.UpdateAsync(policies, CancellationToken.None);

        // Assert
        Assert.NotNull(_cachedOptions);
        Assert.Empty(_cachedOptions.Policies);
        _mockOptionsCache.Verify(x => x.TryRemove(Microsoft.Extensions.Options.Options.DefaultName), Times.Once);
        _mockOptionsCache.Verify(x => x.TryAdd(Microsoft.Extensions.Options.Options.DefaultName, It.IsAny<RequestTimeoutOptions>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_RemovesOldOptionsBeforeAdding()
    {
        // Arrange
        var policies = new List<TimeoutPolicyConfig>
        {
            new()
            {
                Name = "TestPolicy",
                Seconds = 30
            }
        };

        // Act
        await _provider.UpdateAsync(policies, CancellationToken.None);

        // Assert
        var invocations = _mockOptionsCache.Invocations.Select(i => i.Method.Name).ToList();
        var removeIndex = invocations.IndexOf("TryRemove");
        var addIndex = invocations.IndexOf("TryAdd");

        Assert.True(removeIndex < addIndex, "TryRemove should be called before TryAdd");
    }

    [Fact]
    public async Task UpdateAsync_WithVariousTimeoutValues_ConfiguresCorrectly()
    {
        // Arrange
        var policies = new List<TimeoutPolicyConfig>
        {
            new()
            {
                Name = "VeryShort",
                Seconds = 5
            },
            new()
            {
                Name = "Short",
                Seconds = 30
            },
            new()
            {
                Name = "Medium",
                Seconds = 60
            },
            new()
            {
                Name = "Long",
                Seconds = 300
            },
            new()
            {
                Name = "VeryLong",
                Seconds = 3600
            }
        };

        // Act
        await _provider.UpdateAsync(policies, CancellationToken.None);

        // Assert
        Assert.NotNull(_cachedOptions);
        Assert.Equal(5, _cachedOptions.Policies.Count);
        Assert.Equal(TimeSpan.FromSeconds(5), _cachedOptions.Policies["VeryShort"].Timeout);
        Assert.Equal(TimeSpan.FromSeconds(30), _cachedOptions.Policies["Short"].Timeout);
        Assert.Equal(TimeSpan.FromSeconds(60), _cachedOptions.Policies["Medium"].Timeout);
        Assert.Equal(TimeSpan.FromSeconds(300), _cachedOptions.Policies["Long"].Timeout);
        Assert.Equal(TimeSpan.FromSeconds(3600), _cachedOptions.Policies["VeryLong"].Timeout);
    }

    [Fact]
    public async Task UpdateAsync_WithDifferentStatusCodes_ConfiguresCorrectly()
    {
        // Arrange
        var policies = new List<TimeoutPolicyConfig>
        {
            new()
            {
                Name = "ServiceUnavailable",
                Seconds = 30,
                TimeoutStatusCode = 503
            },
            new()
            {
                Name = "GatewayTimeout",
                Seconds = 60,
                TimeoutStatusCode = 504
            },
            new()
            {
                Name = "RequestTimeout",
                Seconds = 45,
                TimeoutStatusCode = 408
            }
        };

        // Act
        await _provider.UpdateAsync(policies, CancellationToken.None);

        // Assert
        Assert.NotNull(_cachedOptions);
        Assert.Equal(3, _cachedOptions.Policies.Count);
        Assert.Equal(503, _cachedOptions.Policies["ServiceUnavailable"].TimeoutStatusCode);
        Assert.Equal(504, _cachedOptions.Policies["GatewayTimeout"].TimeoutStatusCode);
        Assert.Equal(408, _cachedOptions.Policies["RequestTimeout"].TimeoutStatusCode);
    }

    [Fact]
    public async Task UpdateAsync_CalledMultipleTimes_ReplacesOptions()
    {
        // Arrange
        var firstPolicies = new List<TimeoutPolicyConfig>
        {
            new()
            {
                Name = "FirstPolicy",
                Seconds = 30
            }
        };

        var secondPolicies = new List<TimeoutPolicyConfig>
        {
            new()
            {
                Name = "SecondPolicy",
                Seconds = 60
            }
        };

        // Act
        await _provider.UpdateAsync(firstPolicies, CancellationToken.None);
        await _provider.UpdateAsync(secondPolicies, CancellationToken.None);

        // Assert
        _mockOptionsCache.Verify(x => x.TryRemove(Microsoft.Extensions.Options.Options.DefaultName), Times.Exactly(2));
        _mockOptionsCache.Verify(x => x.TryAdd(Microsoft.Extensions.Options.Options.DefaultName, It.IsAny<RequestTimeoutOptions>()), Times.Exactly(2));

        // The last cached options should contain only the second policy
        Assert.NotNull(_cachedOptions);
        Assert.Single(_cachedOptions.Policies);
        Assert.True(_cachedOptions.Policies.ContainsKey("SecondPolicy"));
        Assert.False(_cachedOptions.Policies.ContainsKey("FirstPolicy"));
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicatePolicyNames_LastOneWins()
    {
        // Arrange
        var policies = new List<TimeoutPolicyConfig>
        {
            new()
            {
                Name = "DuplicatePolicy",
                Seconds = 30,
                TimeoutStatusCode = 503
            },
            new()
            {
                Name = "DuplicatePolicy",
                Seconds = 60,
                TimeoutStatusCode = 504
            }
        };

        // Act
        await _provider.UpdateAsync(policies, CancellationToken.None);

        // Assert
        Assert.NotNull(_cachedOptions);
        Assert.Single(_cachedOptions.Policies);
        // The last policy should overwrite the first
        Assert.Equal(TimeSpan.FromSeconds(60), _cachedOptions.Policies["DuplicatePolicy"].Timeout);
        Assert.Equal(504, _cachedOptions.Policies["DuplicatePolicy"].TimeoutStatusCode);
    }

    [Fact]
    public async Task UpdateAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var policies = new List<TimeoutPolicyConfig>
        {
            new()
            {
                Name = "TestPolicy",
                Seconds = 30
            }
        };
        using var cts = new CancellationTokenSource();

        // Act
        await _provider.UpdateAsync(policies, cts.Token);

        // Assert
        Assert.NotNull(_cachedOptions);
        Assert.Single(_cachedOptions.Policies);
    }

    [Fact]
    public async Task UpdateAsync_WithZeroSeconds_ConfiguresCorrectly()
    {
        // Arrange
        var policies = new List<TimeoutPolicyConfig>
        {
            new()
            {
                Name = "ZeroTimeout",
                Seconds = 0
            }
        };

        // Act
        await _provider.UpdateAsync(policies, CancellationToken.None);

        // Assert
        Assert.NotNull(_cachedOptions);
        Assert.Single(_cachedOptions.Policies);
        Assert.Equal(TimeSpan.Zero, _cachedOptions.Policies["ZeroTimeout"].Timeout);
    }

    [Fact]
    public async Task UpdateAsync_WithLargeTimeout_ConfiguresCorrectly()
    {
        // Arrange
        var policies = new List<TimeoutPolicyConfig>
        {
            new()
            {
                Name = "VeryLargeTimeout",
                Seconds = 86400 // 24 hours
            }
        };

        // Act
        await _provider.UpdateAsync(policies, CancellationToken.None);

        // Assert
        Assert.NotNull(_cachedOptions);
        Assert.Single(_cachedOptions.Policies);
        Assert.Equal(TimeSpan.FromDays(1), _cachedOptions.Policies["VeryLargeTimeout"].Timeout);
    }

    [Fact]
    public async Task UpdateAsync_UsesDefaultOptionsName()
    {
        // Arrange
        var policies = new List<TimeoutPolicyConfig>
        {
            new()
            {
                Name = "TestPolicy",
                Seconds = 30
            }
        };

        // Act
        await _provider.UpdateAsync(policies, CancellationToken.None);

        // Assert
        _mockOptionsCache.Verify(
            x => x.TryRemove(Microsoft.Extensions.Options.Options.DefaultName),
            Times.Once,
            "Should use Options.DefaultName for cache key");

        _mockOptionsCache.Verify(
            x => x.TryAdd(Microsoft.Extensions.Options.Options.DefaultName, It.IsAny<RequestTimeoutOptions>()),
            Times.Once,
            "Should use Options.DefaultName for cache key");
    }
}

