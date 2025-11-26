using ErrorOr;
using Sail.Core.Entities;
using Sail.Core.Stores;
using Sail.Models.Middlewares;
using TimeoutEntity = Sail.Core.Entities.Timeout;
using RetryEntity = Sail.Core.Entities.Retry;

namespace Sail.Services;

public class MiddlewareService(IMiddlewareStore store)
{
    public async Task<MiddlewareResponse?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var middleware = await store.GetAsync(id, cancellationToken);
        return middleware != null ? MapToResponse(middleware) : null;
    }

    public async Task<IEnumerable<MiddlewareResponse>> ListAsync(string? keywords, CancellationToken cancellationToken)
    {
        var middlewares = await store.GetAsync(cancellationToken);

        if (!string.IsNullOrEmpty(keywords))
        {
            middlewares = middlewares
                .Where(m => m.Name.Contains(keywords, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return middlewares.Select(MapToResponse);
    }

    public async Task<ErrorOr<Guid>> CreateAsync(MiddlewareRequest request, CancellationToken cancellationToken)
    {
        var middleware = new Middleware
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            Enabled = request.Enabled,
            Cors = request.Cors != null ? new Cors
            {
                Name = request.Cors.Name,
                AllowOrigins = request.Cors.AllowOrigins,
                AllowMethods = request.Cors.AllowMethods,
                AllowHeaders = request.Cors.AllowHeaders,
                ExposeHeaders = request.Cors.ExposeHeaders,
                AllowCredentials = request.Cors.AllowCredentials,
                MaxAge = request.Cors.MaxAge
            } : null,
            RateLimiter = request.RateLimiter != null ? new RateLimiter
            {
                Name = request.RateLimiter.Name,
                PermitLimit = request.RateLimiter.PermitLimit,
                Window = request.RateLimiter.Window,
                QueueLimit = request.RateLimiter.QueueLimit
            } : null,
            Timeout = request.Timeout != null ? new TimeoutEntity
            {
                Name = request.Timeout.Name,
                Seconds = request.Timeout.Seconds,
                TimeoutStatusCode = request.Timeout.TimeoutStatusCode
            } : null,
            Retry = request.Retry != null ? new RetryEntity
            {
                Name = request.Retry.Name,
                MaxRetryAttempts = request.Retry.MaxRetryAttempts,
                RetryStatusCodes = request.Retry.RetryStatusCodes,
                RetryDelayMilliseconds = request.Retry.RetryDelayMilliseconds,
                UseExponentialBackoff = request.Retry.UseExponentialBackoff
            } : null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await store.CreateAsync(middleware, cancellationToken);
        return middleware.Id;
    }

    public async Task<ErrorOr<Updated>> UpdateAsync(Guid id, MiddlewareRequest request, CancellationToken cancellationToken)
    {
        var existing = await store.GetAsync(id, cancellationToken);
        if (existing == null)
        {
            return Error.NotFound(description: "Middleware not found");
        }

        // Update the tracked entity directly
        existing.Name = request.Name;
        existing.Description = request.Description;
        existing.Type = request.Type;
        existing.Enabled = request.Enabled;
        existing.Cors = request.Cors != null ? new Cors
        {
            Name = request.Cors.Name,
            AllowOrigins = request.Cors.AllowOrigins,
            AllowMethods = request.Cors.AllowMethods,
            AllowHeaders = request.Cors.AllowHeaders,
            ExposeHeaders = request.Cors.ExposeHeaders,
            AllowCredentials = request.Cors.AllowCredentials,
            MaxAge = request.Cors.MaxAge
        } : null;
        existing.RateLimiter = request.RateLimiter != null ? new RateLimiter
        {
            Name = request.RateLimiter.Name,
            PermitLimit = request.RateLimiter.PermitLimit,
            Window = request.RateLimiter.Window,
            QueueLimit = request.RateLimiter.QueueLimit
        } : null;
        existing.Timeout = request.Timeout != null ? new TimeoutEntity
        {
            Name = request.Timeout.Name,
            Seconds = request.Timeout.Seconds,
            TimeoutStatusCode = request.Timeout.TimeoutStatusCode
        } : null;
        existing.Retry = request.Retry != null ? new RetryEntity
        {
            Name = request.Retry.Name,
            MaxRetryAttempts = request.Retry.MaxRetryAttempts,
            RetryStatusCodes = request.Retry.RetryStatusCodes,
            RetryDelayMilliseconds = request.Retry.RetryDelayMilliseconds,
            UseExponentialBackoff = request.Retry.UseExponentialBackoff
        } : null;
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        await store.UpdateAsync(existing, cancellationToken);
        return Result.Updated;
    }

    public async Task<ErrorOr<Deleted>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var existing = await store.GetAsync(id, cancellationToken);
        if (existing == null)
        {
            return Error.NotFound(description: "Middleware not found");
        }

        await store.DeleteAsync(id, cancellationToken);
        return Result.Deleted;
    }

    private static MiddlewareResponse MapToResponse(Middleware middleware)
    {
        return new MiddlewareResponse
        {
            Id = middleware.Id,
            Name = middleware.Name,
            Description = middleware.Description,
            Type = middleware.Type,
            Enabled = middleware.Enabled,
            Cors = middleware.Cors != null ? new CorsResponse
            {
                Name = middleware.Cors.Name,
                AllowOrigins = middleware.Cors.AllowOrigins,
                AllowMethods = middleware.Cors.AllowMethods,
                AllowHeaders = middleware.Cors.AllowHeaders,
                ExposeHeaders = middleware.Cors.ExposeHeaders,
                AllowCredentials = middleware.Cors.AllowCredentials,
                MaxAge = middleware.Cors.MaxAge
            } : null,
            RateLimiter = middleware.RateLimiter != null ? new RateLimiterResponse
            {
                Name = middleware.RateLimiter.Name,
                PermitLimit = middleware.RateLimiter.PermitLimit,
                Window = middleware.RateLimiter.Window,
                QueueLimit = middleware.RateLimiter.QueueLimit
            } : null,
            Timeout = middleware.Timeout != null ? new TimeoutResponse
            {
                Name = middleware.Timeout.Name,
                Seconds = middleware.Timeout.Seconds,
                TimeoutStatusCode = middleware.Timeout.TimeoutStatusCode
            } : null,
            Retry = middleware.Retry != null ? new RetryResponse
            {
                Name = middleware.Retry.Name,
                MaxRetryAttempts = middleware.Retry.MaxRetryAttempts,
                RetryStatusCodes = middleware.Retry.RetryStatusCodes,
                RetryDelayMilliseconds = middleware.Retry.RetryDelayMilliseconds,
                UseExponentialBackoff = middleware.Retry.UseExponentialBackoff
            } : null,
            CreatedAt = middleware.CreatedAt,
            UpdatedAt = middleware.UpdatedAt
        };
    }
}

