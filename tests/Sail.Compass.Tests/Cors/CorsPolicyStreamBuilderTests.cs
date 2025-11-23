using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using Sail.Api.V1;
using Sail.Compass.Cors;
using Sail.Compass.Observers;
using Sail.Core.Cors;
using ObserverEventType = Sail.Compass.Observers.EventType;

namespace Sail.Compass.Tests.Cors;

public class CorsPolicyStreamBuilderTests
{
    [Fact]
    public async Task BuildCorsPolicyStream_WithNoMiddlewares_ReturnsEmptyList()
    {
        // Arrange
        var subject = new Subject<ResourceEvent<Middleware>>();
        var observer = new TestResourceObserver<Middleware>(subject);

        // Act
        var stream = CorsPolicyStreamBuilder.BuildCorsPolicyStream(observer);
        var result = await stream.FirstAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task BuildCorsPolicyStream_WithCorsMiddleware_ReturnsCorsPolicyConfig()
    {
        // Arrange
        var subject = new Subject<ResourceEvent<Middleware>>();
        var observer = new TestResourceObserver<Middleware>(subject);

        var middleware = new Middleware
        {
            MiddlewareId = "middleware1",
            Name = "cors-policy-1",
            Type = MiddlewareType.Cors,
            Enabled = true,
            Cors = new Sail.Api.V1.Cors
            {
                Name = "cors-policy-1",
                AllowOrigins = { "https://example.com" },
                AllowMethods = { "GET", "POST" },
                AllowHeaders = { "Content-Type" },
                ExposeHeaders = { "X-Custom-Header" },
                AllowCredentials = true,
                MaxAge = 3600
            }
        };

        // Act
        var stream = CorsPolicyStreamBuilder.BuildCorsPolicyStream(observer);
        var task = stream.Skip(1).FirstAsync().ToTask();

        subject.OnNext(new ResourceEvent<Middleware>(ObserverEventType.Created, middleware, default!));
        var result = await task;

        // Assert
        Assert.Single(result);
        var config = result[0];
        Assert.Equal("cors-policy-1", config.Name);
        Assert.Equal(new[] { "https://example.com" }, config.AllowOrigins);
        Assert.Equal(new[] { "GET", "POST" }, config.AllowMethods);
        Assert.Equal(new[] { "Content-Type" }, config.AllowHeaders);
        Assert.Equal(new[] { "X-Custom-Header" }, config.ExposeHeaders);
        Assert.True(config.AllowCredentials);
        Assert.Equal(3600, config.MaxAge);
    }

    [Fact]
    public async Task BuildCorsPolicyStream_WithDisabledMiddleware_FiltersOut()
    {
        // Arrange
        var subject = new Subject<ResourceEvent<Middleware>>();
        var observer = new TestResourceObserver<Middleware>(subject);

        var middleware = new Middleware
        {
            MiddlewareId = "middleware1",
            Name = "cors-policy-1",
            Type = MiddlewareType.Cors,
            Enabled = false,
            Cors = new Sail.Api.V1.Cors
            {
                Name = "cors-policy-1",
                AllowOrigins = { "https://example.com" }
            }
        };

        // Act
        var stream = CorsPolicyStreamBuilder.BuildCorsPolicyStream(observer);
        var task = stream.Skip(1).FirstAsync().ToTask();

        subject.OnNext(new ResourceEvent<Middleware>(ObserverEventType.Created, middleware, default!));
        var result = await task;

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task BuildCorsPolicyStream_WithNonCorsMiddleware_FiltersOut()
    {
        // Arrange
        var subject = new Subject<ResourceEvent<Middleware>>();
        var observer = new TestResourceObserver<Middleware>(subject);

        var middleware = new Middleware
        {
            MiddlewareId = "middleware1",
            Name = "rate-limiter-1",
            Type = MiddlewareType.RateLimiter,
            Enabled = true,
            RateLimiter = new Sail.Api.V1.RateLimiter
            {
                Name = "rate-limiter-1",
                PermitLimit = 100,
                Window = 60,
                QueueLimit = 10
            }
        };

        // Act
        var stream = CorsPolicyStreamBuilder.BuildCorsPolicyStream(observer);
        var task = stream.Skip(1).FirstAsync().ToTask();

        subject.OnNext(new ResourceEvent<Middleware>(ObserverEventType.Created, middleware, default!));
        var result = await task;

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task BuildCorsPolicyStream_WithNullCorsConfig_FiltersOut()
    {
        // Arrange
        var subject = new Subject<ResourceEvent<Middleware>>();
        var observer = new TestResourceObserver<Middleware>(subject);

        var middleware = new Middleware
        {
            MiddlewareId = "middleware1",
            Name = "cors-policy-1",
            Type = MiddlewareType.Cors,
            Enabled = true,
            Cors = null
        };

        // Act
        var stream = CorsPolicyStreamBuilder.BuildCorsPolicyStream(observer);
        var task = stream.Skip(1).FirstAsync().ToTask();

        subject.OnNext(new ResourceEvent<Middleware>(ObserverEventType.Created, middleware, default!));
        var result = await task;

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task BuildCorsPolicyStream_WithMultipleMiddlewares_ReturnsMultipleConfigs()
    {
        // Arrange
        var subject = new Subject<ResourceEvent<Middleware>>();
        var observer = new TestResourceObserver<Middleware>(subject);

        var middleware1 = new Middleware
        {
            MiddlewareId = "middleware1",
            Name = "cors-policy-1",
            Type = MiddlewareType.Cors,
            Enabled = true,
            Cors = new Sail.Api.V1.Cors
            {
                Name = "cors-policy-1",
                AllowOrigins = { "https://example1.com" }
            }
        };

        var middleware2 = new Middleware
        {
            MiddlewareId = "middleware2",
            Name = "cors-policy-2",
            Type = MiddlewareType.Cors,
            Enabled = true,
            Cors = new Sail.Api.V1.Cors
            {
                Name = "cors-policy-2",
                AllowOrigins = { "https://example2.com" }
            }
        };

        // Act
        var stream = CorsPolicyStreamBuilder.BuildCorsPolicyStream(observer);
        var task = stream.Skip(1).FirstAsync().ToTask();

        subject.OnNext(new ResourceEvent<Middleware>(ObserverEventType.Created, middleware1, default!));
        subject.OnNext(new ResourceEvent<Middleware>(ObserverEventType.Created, middleware2, default!));
        var result = await task;

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Name == "cors-policy-1");
        Assert.Contains(result, c => c.Name == "cors-policy-2");
    }

    [Fact]
    public async Task BuildCorsPolicyStream_WithUpdatedMiddleware_ReturnsUpdatedConfig()
    {
        // Arrange
        var subject = new Subject<ResourceEvent<Middleware>>();
        var observer = new TestResourceObserver<Middleware>(subject);

        var middleware = new Middleware
        {
            MiddlewareId = "middleware1",
            Name = "cors-policy-1",
            Type = MiddlewareType.Cors,
            Enabled = true,
            Cors = new Sail.Api.V1.Cors
            {
                Name = "cors-policy-1",
                AllowOrigins = { "https://example.com" }
            }
        };

        var updatedMiddleware = new Middleware
        {
            MiddlewareId = "middleware1",
            Name = "cors-policy-1",
            Type = MiddlewareType.Cors,
            Enabled = true,
            Cors = new Sail.Api.V1.Cors
            {
                Name = "cors-policy-1",
                AllowOrigins = { "https://updated.com" }
            }
        };

        // Act
        var stream = CorsPolicyStreamBuilder.BuildCorsPolicyStream(observer);
        var task = stream.Skip(2).FirstAsync().ToTask();

        subject.OnNext(new ResourceEvent<Middleware>(ObserverEventType.Created, middleware, default!));
        subject.OnNext(new ResourceEvent<Middleware>(ObserverEventType.Updated, updatedMiddleware, middleware));
        var result = await task;

        // Assert
        Assert.Single(result);
        Assert.Equal("https://updated.com", result[0].AllowOrigins![0]);
    }

    [Fact]
    public async Task BuildCorsPolicyStream_WithDeletedMiddleware_RemovesConfig()
    {
        // Arrange
        var subject = new Subject<ResourceEvent<Middleware>>();
        var observer = new TestResourceObserver<Middleware>(subject);

        var middleware = new Middleware
        {
            MiddlewareId = "middleware1",
            Name = "cors-policy-1",
            Type = MiddlewareType.Cors,
            Enabled = true,
            Cors = new Sail.Api.V1.Cors
            {
                Name = "cors-policy-1",
                AllowOrigins = { "https://example.com" }
            }
        };

        // Act
        var stream = CorsPolicyStreamBuilder.BuildCorsPolicyStream(observer);
        var task = stream.Skip(2).FirstAsync().ToTask();

        subject.OnNext(new ResourceEvent<Middleware>(ObserverEventType.Created, middleware, default!));
        subject.OnNext(new ResourceEvent<Middleware>(ObserverEventType.Deleted, middleware, default!));
        var result = await task;

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task BuildCorsPolicyStream_WithListEvent_ProcessesCorrectly()
    {
        // Arrange
        var subject = new Subject<ResourceEvent<Middleware>>();
        var observer = new TestResourceObserver<Middleware>(subject);

        var middleware = new Middleware
        {
            MiddlewareId = "middleware1",
            Name = "cors-policy-1",
            Type = MiddlewareType.Cors,
            Enabled = true,
            Cors = new Sail.Api.V1.Cors
            {
                Name = "cors-policy-1",
                AllowOrigins = { "https://example.com" }
            }
        };

        // Act
        var stream = CorsPolicyStreamBuilder.BuildCorsPolicyStream(observer);
        var task = stream.Skip(1).FirstAsync().ToTask();

        subject.OnNext(new ResourceEvent<Middleware>(ObserverEventType.List, middleware, default!));
        var result = await task;

        // Assert
        Assert.Single(result);
        Assert.Equal("cors-policy-1", result[0].Name);
    }

    private class TestResourceObserver<T> : ResourceObserver<T>
    {
        private readonly IObservable<ResourceEvent<T>> _observable;

        public TestResourceObserver(IObservable<ResourceEvent<T>> observable)
        {
            _observable = observable;
        }

        public override IObservable<ResourceEvent<T>> GetObservable(bool watch = false)
        {
            return _observable;
        }
    }
}

