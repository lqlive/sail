using Sail.Core.Https;

namespace Sail.Core.Tests.Https;

public class BindingAddressTests
{
    [Fact]
    public void Parse_WithHttpScheme_ParsesCorrectly()
    {
        // Arrange
        var address = "http://localhost:5000";

        // Act
        var result = BindingAddress.Parse(address);

        // Assert
        Assert.Equal("http", result.Scheme);
        Assert.Equal("localhost", result.Host);
        Assert.Equal(5000, result.Port);
    }

    [Fact]
    public void Parse_WithHttpsScheme_ParsesCorrectly()
    {
        // Arrange
        var address = "https://example.com:5001";

        // Act
        var result = BindingAddress.Parse(address);

        // Assert
        Assert.Equal("https", result.Scheme);
        Assert.Equal("example.com", result.Host);
        Assert.Equal(5001, result.Port);
    }

    [Fact]
    public void Parse_WithDefaultHttpPort_ParsesCorrectly()
    {
        // Arrange
        var address = "http://localhost";

        // Act
        var result = BindingAddress.Parse(address);

        // Assert
        Assert.Equal("http", result.Scheme);
        Assert.Equal("localhost", result.Host);
        Assert.Equal(80, result.Port); // Default HTTP port
    }

    [Fact]
    public void Parse_WithDefaultHttpsPort_ParsesCorrectly()
    {
        // Arrange
        var address = "https://example.com";

        // Act
        var result = BindingAddress.Parse(address);

        // Assert
        Assert.Equal("https", result.Scheme);
        Assert.Equal("example.com", result.Host);
        Assert.Equal(443, result.Port); // Default HTTPS port
    }

    [Fact]
    public void Parse_WithPathAndQuery_ParsesPathBase()
    {
        // Arrange
        var address = "http://localhost:5000/api/v1";

        // Act
        var result = BindingAddress.Parse(address);

        // Assert
        Assert.Equal("http", result.Scheme);
        Assert.Equal("localhost", result.Host);
        Assert.Equal(5000, result.Port);
        Assert.Equal("/api/v1", result.PathBase);
    }

    [Fact]
    public void Parse_WithQueryString_ParsesPathBase()
    {
        // Arrange
        var address = "http://localhost:5000/path?query=value";

        // Act
        var result = BindingAddress.Parse(address);

        // Assert
        Assert.Equal("http", result.Scheme);
        Assert.Equal("localhost", result.Host);
        Assert.Equal(5000, result.Port);
        Assert.Equal("/path?query=value", result.PathBase);
    }

    [Fact]
    public void Parse_WithRootPath_ParsesEmptyPathBase()
    {
        // Arrange
        var address = "http://localhost:5000/";

        // Act
        var result = BindingAddress.Parse(address);

        // Assert
        Assert.Equal("http", result.Scheme);
        Assert.Equal("localhost", result.Host);
        Assert.Equal(5000, result.Port);
        Assert.Equal("/", result.PathBase);
    }

    [Fact]
    public void Parse_WithIPv4Address_ParsesCorrectly()
    {
        // Arrange
        var address = "http://127.0.0.1:8080";

        // Act
        var result = BindingAddress.Parse(address);

        // Assert
        Assert.Equal("http", result.Scheme);
        Assert.Equal("127.0.0.1", result.Host);
        Assert.Equal(8080, result.Port);
    }

    [Fact]
    public void Parse_WithIPv6Address_ParsesCorrectly()
    {
        // Arrange
        var address = "http://[::1]:5000";

        // Act
        var result = BindingAddress.Parse(address);

        // Assert
        Assert.Equal("http", result.Scheme);
        Assert.Equal("[::1]", result.Host); // Uri.Host preserves brackets for IPv6
        Assert.Equal(5000, result.Port);
    }

    [Fact]
    public void Parse_WithIPv6AddressAndBrackets_ParsesCorrectly()
    {
        // Arrange
        var address = "https://[2001:db8::1]:8443";

        // Act
        var result = BindingAddress.Parse(address);

        // Assert
        Assert.Equal("https", result.Scheme);
        Assert.Equal("[2001:db8::1]", result.Host); // Uri.Host preserves brackets for IPv6
        Assert.Equal(8443, result.Port);
    }

    [Fact]
    public void Parse_WithSubdomain_ParsesCorrectly()
    {
        // Arrange
        var address = "https://api.example.com:5001";

        // Act
        var result = BindingAddress.Parse(address);

        // Assert
        Assert.Equal("https", result.Scheme);
        Assert.Equal("api.example.com", result.Host);
        Assert.Equal(5001, result.Port);
    }

    [Fact]
    public void Parse_WithMultipleSubdomains_ParsesCorrectly()
    {
        // Arrange
        var address = "https://api.v1.example.com:443";

        // Act
        var result = BindingAddress.Parse(address);

        // Assert
        Assert.Equal("https", result.Scheme);
        Assert.Equal("api.v1.example.com", result.Host);
        Assert.Equal(443, result.Port);
    }

    [Fact]
    public void Parse_WithCustomPort_ParsesCorrectly()
    {
        // Arrange
        var address = "http://localhost:8888";

        // Act
        var result = BindingAddress.Parse(address);

        // Assert
        Assert.Equal("http", result.Scheme);
        Assert.Equal("localhost", result.Host);
        Assert.Equal(8888, result.Port);
    }

    [Fact]
    public void Parse_WithComplexPath_ParsesPathBase()
    {
        // Arrange
        var address = "http://example.com:5000/api/v1/users/123";

        // Act
        var result = BindingAddress.Parse(address);

        // Assert
        Assert.Equal("http", result.Scheme);
        Assert.Equal("example.com", result.Host);
        Assert.Equal(5000, result.Port);
        Assert.Equal("/api/v1/users/123", result.PathBase);
    }

    [Fact]
    public void Parse_WithComplexQueryString_ParsesPathBase()
    {
        // Arrange
        var address = "https://example.com:5001/search?q=test&page=1&limit=10";

        // Act
        var result = BindingAddress.Parse(address);

        // Assert
        Assert.Equal("https", result.Scheme);
        Assert.Equal("example.com", result.Host);
        Assert.Equal(5001, result.Port);
        Assert.Equal("/search?q=test&page=1&limit=10", result.PathBase);
    }

    [Fact]
    public void Parse_WithFragment_ParsesPathBase()
    {
        // Arrange
        var address = "http://example.com:5000/page#section";

        // Act
        var result = BindingAddress.Parse(address);

        // Assert
        Assert.Equal("http", result.Scheme);
        Assert.Equal("example.com", result.Host);
        Assert.Equal(5000, result.Port);
        Assert.Contains("/page", result.PathBase);
    }

    [Fact]
    public void Parse_WithWildcardHost_ThrowsException()
    {
        // Arrange
        var address = "http://*:5000";

        // Act & Assert
        // Wildcard hosts are not valid URIs in .NET Uri class
        Assert.Throws<UriFormatException>(() => BindingAddress.Parse(address));
    }

    [Fact]
    public void Parse_WithPlusHost_ThrowsException()
    {
        // Arrange
        var address = "http://+:5000";

        // Act & Assert
        // Plus hosts are not valid URIs in .NET Uri class
        Assert.Throws<UriFormatException>(() => BindingAddress.Parse(address));
    }

    [Fact]
    public void Parse_WithLowPortNumber_ParsesCorrectly()
    {
        // Arrange
        var address = "http://localhost:1";

        // Act
        var result = BindingAddress.Parse(address);

        // Assert
        Assert.Equal("http", result.Scheme);
        Assert.Equal("localhost", result.Host);
        Assert.Equal(1, result.Port);
    }

    [Fact]
    public void Parse_WithHighPortNumber_ParsesCorrectly()
    {
        // Arrange
        var address = "http://localhost:65535";

        // Act
        var result = BindingAddress.Parse(address);

        // Assert
        Assert.Equal("http", result.Scheme);
        Assert.Equal("localhost", result.Host);
        Assert.Equal(65535, result.Port);
    }

    [Fact]
    public void Parse_WithInvalidAddress_ThrowsException()
    {
        // Arrange
        var address = "not-a-valid-url";

        // Act & Assert
        Assert.Throws<UriFormatException>(() => BindingAddress.Parse(address));
    }

    [Fact]
    public void Parse_WithEmptyString_ThrowsException()
    {
        // Arrange
        var address = string.Empty;

        // Act & Assert
        Assert.Throws<UriFormatException>(() => BindingAddress.Parse(address));
    }

    [Fact]
    public void Parse_WithRelativeUrl_ThrowsException()
    {
        // Arrange
        var address = "/relative/path";

        // Act & Assert
        Assert.Throws<UriFormatException>(() => BindingAddress.Parse(address));
    }

    [Fact]
    public void Parse_WithHttpsAndStandardPort_ParsesCorrectly()
    {
        // Arrange
        var address = "https://example.com:443";

        // Act
        var result = BindingAddress.Parse(address);

        // Assert
        Assert.Equal("https", result.Scheme);
        Assert.Equal("example.com", result.Host);
        Assert.Equal(443, result.Port);
    }

    [Fact]
    public void Parse_WithHttpAndStandardPort_ParsesCorrectly()
    {
        // Arrange
        var address = "http://example.com:80";

        // Act
        var result = BindingAddress.Parse(address);

        // Assert
        Assert.Equal("http", result.Scheme);
        Assert.Equal("example.com", result.Host);
        Assert.Equal(80, result.Port);
    }

    [Fact]
    public void Parse_WithEncodedPath_ParsesPathBase()
    {
        // Arrange
        var address = "http://example.com:5000/path%20with%20spaces";

        // Act
        var result = BindingAddress.Parse(address);

        // Assert
        Assert.Equal("http", result.Scheme);
        Assert.Equal("example.com", result.Host);
        Assert.Equal(5000, result.Port);
        Assert.Contains("/path", result.PathBase);
    }

    [Fact]
    public void Parse_WithMultipleAddresses_EachParsesIndependently()
    {
        // Arrange
        var addresses = new[]
        {
            "http://localhost:5000",
            "https://localhost:5001",
            "http://0.0.0.0:8080"
        };

        // Act
        var results = addresses.Select(BindingAddress.Parse).ToList();

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal("http", results[0].Scheme);
        Assert.Equal("https", results[1].Scheme);
        Assert.Equal(5000, results[0].Port);
        Assert.Equal(5001, results[1].Port);
        Assert.Equal(8080, results[2].Port);
    }

    [Fact]
    public void Parse_WithDifferentSchemes_ParsesCorrectly()
    {
        // Arrange
        var httpAddress = "http://example.com:80";
        var httpsAddress = "https://example.com:443";

        // Act
        var httpResult = BindingAddress.Parse(httpAddress);
        var httpsResult = BindingAddress.Parse(httpsAddress);

        // Assert
        Assert.Equal("http", httpResult.Scheme);
        Assert.Equal("https", httpsResult.Scheme);
        Assert.Equal(80, httpResult.Port);
        Assert.Equal(443, httpsResult.Port);
    }
}

