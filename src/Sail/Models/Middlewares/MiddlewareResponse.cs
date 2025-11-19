using Sail.Core.Entities;

namespace Sail.Models.Middlewares;

public record MiddlewareResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public MiddlewareType Type { get; init; }
    public bool Enabled { get; init; }
    public CorsResponse? Cors { get; init; }
    public RateLimiterResponse? RateLimiter { get; init; }
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

