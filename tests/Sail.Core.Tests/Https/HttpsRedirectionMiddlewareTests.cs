using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Sail.Core.Https;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Model;

namespace Sail.Core.Tests.Https;

public class HttpsRedirectionMiddlewareTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;
    private readonly Mock<ILogger<HttpsRedirectionMiddleware>> _mockLogger;
    private bool _nextCalled;

    public HttpsRedirectionMiddlewareTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLoggerFactory = new Mock<ILoggerFactory>();
        _mockLogger = new Mock<ILogger<HttpsRedirectionMiddleware>>();
        _mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(_mockLogger.Object);
        _nextCalled = false;
    }

    [Fact]
    public async Task Invoke_WithHttpsRequest_CallsNext()
    {
        // Arrange
        var options = Microsoft.Extensions.Options.Options.Create(new HttpsRedirectionOptions());
        var middleware = new HttpsRedirectionMiddleware(
            NextDelegate,
            options,
            _mockConfiguration.Object,
            _mockLoggerFactory.Object);

        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("example.com");

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.True(_nextCalled);
        Assert.Equal(200, context.Response.StatusCode); // Default status, not redirected
    }

    [Fact]
    public async Task Invoke_WithHttpRequest_NoReverseProxyFeature_CallsNext()
    {
        // Arrange
        var options = Microsoft.Extensions.Options.Options.Create(new HttpsRedirectionOptions { HttpsPort = 443 });
        var middleware = new HttpsRedirectionMiddleware(
            NextDelegate,
            options,
            _mockConfiguration.Object,
            _mockLoggerFactory.Object);

        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("example.com");

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.True(_nextCalled);
        Assert.Equal(200, context.Response.StatusCode);
    }

    [Fact]
    public async Task Invoke_WithHttpRequest_HttpsRedirectFalse_CallsNext()
    {
        // Arrange
        var options = Microsoft.Extensions.Options.Options.Create(new HttpsRedirectionOptions { HttpsPort = 443 });
        var middleware = new HttpsRedirectionMiddleware(
            NextDelegate,
            options,
            _mockConfiguration.Object,
            _mockLoggerFactory.Object);

        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("example.com");
        context.Request.Path = "/api/test";

        var mockRoute = CreateMockReverseProxyFeature(httpsRedirect: "false");
        context.Features.Set<IReverseProxyFeature>(mockRoute.Object);

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.True(_nextCalled);
        Assert.Equal(200, context.Response.StatusCode);
    }

    [Fact]
    public async Task Invoke_WithHttpRequest_HttpsRedirectTrue_RedirectsToHttps()
    {
        // Arrange
        var options = Microsoft.Extensions.Options.Options.Create(new HttpsRedirectionOptions { HttpsPort = 443 });
        var middleware = new HttpsRedirectionMiddleware(
            NextDelegate,
            options,
            _mockConfiguration.Object,
            _mockLoggerFactory.Object);

        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("example.com");
        context.Request.Path = "/api/test";
        context.Request.QueryString = new QueryString("?key=value");

        var mockRoute = CreateMockReverseProxyFeature(httpsRedirect: "true");
        context.Features.Set<IReverseProxyFeature>(mockRoute.Object);

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.False(_nextCalled);
        Assert.Equal(308, context.Response.StatusCode);
        Assert.Equal("https://example.com/api/test?key=value", context.Response.Headers.Location.ToString());
    }

    [Fact]
    public async Task Invoke_WithCustomPort_IncludesPortInRedirect()
    {
        // Arrange
        var options = Microsoft.Extensions.Options.Options.Create(new HttpsRedirectionOptions { HttpsPort = 8443 });
        var middleware = new HttpsRedirectionMiddleware(
            NextDelegate,
            options,
            _mockConfiguration.Object,
            _mockLoggerFactory.Object);

        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("example.com", 8080);
        context.Request.Path = "/api/test";

        var mockRoute = CreateMockReverseProxyFeature(httpsRedirect: "true");
        context.Features.Set<IReverseProxyFeature>(mockRoute.Object);

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.False(_nextCalled);
        Assert.Equal(308, context.Response.StatusCode);
        Assert.Equal("https://example.com:8443/api/test", context.Response.Headers.Location.ToString());
    }

    [Fact]
    public async Task Invoke_WithPort443_OmitsPortInRedirect()
    {
        // Arrange
        var options = Microsoft.Extensions.Options.Options.Create(new HttpsRedirectionOptions { HttpsPort = 443 });
        var middleware = new HttpsRedirectionMiddleware(
            NextDelegate,
            options,
            _mockConfiguration.Object,
            _mockLoggerFactory.Object);

        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("example.com", 80);
        context.Request.Path = "/api/test";

        var mockRoute = CreateMockReverseProxyFeature(httpsRedirect: "true");
        context.Features.Set<IReverseProxyFeature>(mockRoute.Object);

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.False(_nextCalled);
        Assert.Equal(308, context.Response.StatusCode);
        Assert.Equal("https://example.com/api/test", context.Response.Headers.Location.ToString());
    }

    [Fact]
    public async Task Invoke_WithCustomStatusCode_UsesCustomStatusCode()
    {
        // Arrange
        var options = Microsoft.Extensions.Options.Options.Create(new HttpsRedirectionOptions
        {
            HttpsPort = 443,
            RedirectStatusCode = 301
        });
        var middleware = new HttpsRedirectionMiddleware(
            NextDelegate,
            options,
            _mockConfiguration.Object,
            _mockLoggerFactory.Object);

        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("example.com");
        context.Request.Path = "/api/test";

        var mockRoute = CreateMockReverseProxyFeature(httpsRedirect: "true");
        context.Features.Set<IReverseProxyFeature>(mockRoute.Object);

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.False(_nextCalled);
        Assert.Equal(301, context.Response.StatusCode);
        Assert.Equal("https://example.com/api/test", context.Response.Headers.Location.ToString());
    }

    [Fact]
    public async Task Invoke_WithPathBase_IncludesPathBaseInRedirect()
    {
        // Arrange
        var options = Microsoft.Extensions.Options.Options.Create(new HttpsRedirectionOptions { HttpsPort = 443 });
        var middleware = new HttpsRedirectionMiddleware(
            NextDelegate,
            options,
            _mockConfiguration.Object,
            _mockLoggerFactory.Object);

        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("example.com");
        context.Request.PathBase = "/basepath";
        context.Request.Path = "/api/test";

        var mockRoute = CreateMockReverseProxyFeature(httpsRedirect: "true");
        context.Features.Set<IReverseProxyFeature>(mockRoute.Object);

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.False(_nextCalled);
        Assert.Equal(308, context.Response.StatusCode);
        Assert.Equal("https://example.com/basepath/api/test", context.Response.Headers.Location.ToString());
    }

    [Fact]
    public async Task Invoke_WithQueryString_PreservesQueryString()
    {
        // Arrange
        var options = Microsoft.Extensions.Options.Options.Create(new HttpsRedirectionOptions { HttpsPort = 443 });
        var middleware = new HttpsRedirectionMiddleware(
            NextDelegate,
            options,
            _mockConfiguration.Object,
            _mockLoggerFactory.Object);

        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("example.com");
        context.Request.Path = "/api/test";
        context.Request.QueryString = new QueryString("?param1=value1&param2=value2");

        var mockRoute = CreateMockReverseProxyFeature(httpsRedirect: "true");
        context.Features.Set<IReverseProxyFeature>(mockRoute.Object);

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.False(_nextCalled);
        Assert.Equal(308, context.Response.StatusCode);
        Assert.Equal("https://example.com/api/test?param1=value1&param2=value2",
            context.Response.Headers.Location.ToString());
    }

    [Fact]
    public async Task Invoke_WithHttpsPortFromConfiguration_UsesConfiguredPort()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["HTTPS_PORT"]).Returns("5001");

        var options = Microsoft.Extensions.Options.Options.Create(new HttpsRedirectionOptions());
        var middleware = new HttpsRedirectionMiddleware(
            NextDelegate,
            options,
            _mockConfiguration.Object,
            _mockLoggerFactory.Object);

        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("example.com");
        context.Request.Path = "/api/test";

        var mockRoute = CreateMockReverseProxyFeature(httpsRedirect: "true");
        context.Features.Set<IReverseProxyFeature>(mockRoute.Object);

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.False(_nextCalled);
        Assert.Equal(308, context.Response.StatusCode);
        Assert.Equal("https://example.com:5001/api/test", context.Response.Headers.Location.ToString());
    }

    [Fact]
    public async Task Invoke_WithServerAddressesFeature_DiscoversHttpsPort()
    {
        // Arrange
        var mockServerAddresses = new Mock<IServerAddressesFeature>();
        mockServerAddresses.Setup(x => x.Addresses).Returns(new List<string>
        {
            "http://localhost:5000",
            "https://localhost:5001"
        });

        var options = Microsoft.Extensions.Options.Options.Create(new HttpsRedirectionOptions());
        var middleware = new HttpsRedirectionMiddleware(
            NextDelegate,
            options,
            _mockConfiguration.Object,
            _mockLoggerFactory.Object,
            mockServerAddresses.Object);

        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("example.com");
        context.Request.Path = "/api/test";

        var mockRoute = CreateMockReverseProxyFeature(httpsRedirect: "true");
        context.Features.Set<IReverseProxyFeature>(mockRoute.Object);

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.False(_nextCalled);
        Assert.Equal(308, context.Response.StatusCode);
        Assert.Equal("https://example.com:5001/api/test", context.Response.Headers.Location.ToString());
    }

    [Fact]
    public async Task Invoke_WithComplexUrl_RedirectsCorrectly()
    {
        // Arrange
        var options = Microsoft.Extensions.Options.Options.Create(new HttpsRedirectionOptions { HttpsPort = 443 });
        var middleware = new HttpsRedirectionMiddleware(
            NextDelegate,
            options,
            _mockConfiguration.Object,
            _mockLoggerFactory.Object);

        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("api.example.com");
        context.Request.PathBase = "/v1";
        context.Request.Path = "/users/123/profile";
        context.Request.QueryString = new QueryString("?includeDetails=true&format=json");

        var mockRoute = CreateMockReverseProxyFeature(httpsRedirect: "true");
        context.Features.Set<IReverseProxyFeature>(mockRoute.Object);

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.False(_nextCalled);
        Assert.Equal(308, context.Response.StatusCode);
        Assert.Equal("https://api.example.com/v1/users/123/profile?includeDetails=true&format=json",
            context.Response.Headers.Location.ToString());
    }

    [Fact]
    public async Task Invoke_WithNoHttpsPortConfigured_CallsNext()
    {
        // Arrange
        var options = Microsoft.Extensions.Options.Options.Create(new HttpsRedirectionOptions());
        var middleware = new HttpsRedirectionMiddleware(
            NextDelegate,
            options,
            _mockConfiguration.Object,
            _mockLoggerFactory.Object);

        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("example.com");
        context.Request.Path = "/api/test";

        var mockRoute = CreateMockReverseProxyFeature(httpsRedirect: "true");
        context.Features.Set<IReverseProxyFeature>(mockRoute.Object);

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.True(_nextCalled);
        Assert.Equal(200, context.Response.StatusCode);
    }

    [Fact]
    public async Task Invoke_WithRootPath_RedirectsCorrectly()
    {
        // Arrange
        var options = Microsoft.Extensions.Options.Options.Create(new HttpsRedirectionOptions { HttpsPort = 443 });
        var middleware = new HttpsRedirectionMiddleware(
            NextDelegate,
            options,
            _mockConfiguration.Object,
            _mockLoggerFactory.Object);

        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("example.com");
        context.Request.Path = "/";

        var mockRoute = CreateMockReverseProxyFeature(httpsRedirect: "true");
        context.Features.Set<IReverseProxyFeature>(mockRoute.Object);

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.False(_nextCalled);
        Assert.Equal(308, context.Response.StatusCode);
        Assert.Equal("https://example.com/", context.Response.Headers.Location.ToString());
    }

    private Task NextDelegate(HttpContext context)
    {
        _nextCalled = true;
        context.Response.StatusCode = 200;
        return Task.CompletedTask;
    }

    private Mock<IReverseProxyFeature> CreateMockReverseProxyFeature(string httpsRedirect)
    {
        var mockRoute = new Mock<IReverseProxyFeature>();

        var metadata = new Dictionary<string, string>
        {
            { "HttpsRedirect", httpsRedirect }
        };

        var routeConfig = new RouteConfig
        {
            RouteId = "test-route",
            Match = new RouteMatch(),
            ClusterId = "test-cluster",
            Metadata = metadata
        };

        // Create actual RouteModel since it's sealed
        var routeModel = new RouteModel(routeConfig, null!, HttpTransformer.Default);
        mockRoute.Setup(x => x.Route).Returns(routeModel);

        return mockRoute;
    }
}

