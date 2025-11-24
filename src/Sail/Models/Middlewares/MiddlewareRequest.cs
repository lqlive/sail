using Sail.Core.Entities;

namespace Sail.Models.Middlewares;

public record MiddlewareRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public MiddlewareType Type { get; init; }
    public bool Enabled { get; init; } = true;
    public CorsRequest? Cors { get; init; }
    public RateLimiterRequest? RateLimiter { get; init; }
    public TimeoutRequest? Timeout { get; init; }
    public RetryRequest? Retry { get; init; }
}

public record CorsRequest
{
    public required string Name { get; init; }
    public List<string>? AllowOrigins { get; init; }
    public List<string>? AllowMethods { get; init; }
    public List<string>? AllowHeaders { get; init; }
    public List<string>? ExposeHeaders { get; init; }
    public bool AllowCredentials { get; init; }
    public int? MaxAge { get; init; }
}

public record RateLimiterRequest
{
    public required string Name { get; init; }
    public int PermitLimit { get; init; }
    public int Window { get; init; }
    public int QueueLimit { get; init; }
}

public record TimeoutRequest
{
    public required string Name { get; init; }
    public int Seconds { get; init; }
    public int? TimeoutStatusCode { get; init; }
}

public record RetryRequest
{
    public required string Name { get; init; }
    public int MaxRetryAttempts { get; init; } = 1;
    public int[] RetryStatusCodes { get; init; } = [500, 502, 503, 504];
    public int RetryDelayMilliseconds { get; init; } = 1000;
    public bool UseExponentialBackoff { get; init; } = false;
}

