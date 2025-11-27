using Sail.Core.Entities;

namespace Sail.Middleware.Models;

public record MiddlewareResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public MiddlewareType Type { get; init; }
    public bool Enabled { get; init; }
    public CorsResponse? Cors { get; init; }
    public RateLimiterResponse? RateLimiter { get; init; }
    public TimeoutResponse? Timeout { get; init; }
    public RetryResponse? Retry { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public record CorsResponse
{
    public required string Name { get; init; }
    public List<string>? AllowOrigins { get; init; }
    public List<string>? AllowMethods { get; init; }
    public List<string>? AllowHeaders { get; init; }
    public List<string>? ExposeHeaders { get; init; }
    public bool AllowCredentials { get; init; }
    public int? MaxAge { get; init; }
}

public record RateLimiterResponse
{
    public required string Name { get; init; }
    public int PermitLimit { get; init; }
    public int Window { get; init; }
    public int QueueLimit { get; init; }
}

public record TimeoutResponse
{
    public required string Name { get; init; }
    public int Seconds { get; init; }
    public int? TimeoutStatusCode { get; init; }
}

public record RetryResponse
{
    public required string Name { get; init; }
    public int MaxRetryAttempts { get; init; }
    public int[] RetryStatusCodes { get; init; }
    public int RetryDelayMilliseconds { get; init; }
    public bool UseExponentialBackoff { get; init; }
}

