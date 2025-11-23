using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Sail.Core.Retry;
using Yarp.ReverseProxy.Model;

namespace Sail.Core.Tests.Retry;

public class RetryMiddlewareTests
{
    private readonly Mock<IRetryPolicyProvider> _mockPolicyProvider;
    private readonly Mock<ILogger<RetryMiddleware>> _mockLogger;
    private bool _nextCalled;

    public RetryMiddlewareTests()
    {
        _mockPolicyProvider = new Mock<IRetryPolicyProvider>();
        _mockLogger = new Mock<ILogger<RetryMiddleware>>();
        _nextCalled = false;
    }

    private Task NextDelegate(HttpContext context)
    {
        _nextCalled = true;
        return Task.CompletedTask;
    }

    [Fact]
    public async Task InvokeAsync_WithoutReverseProxyFeature_CallsNext()
    {
        // Arrange
        var middleware = new RetryMiddleware(
            NextDelegate,
            _mockPolicyProvider.Object,
            _mockLogger.Object);

        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(_nextCalled);
    }
}

