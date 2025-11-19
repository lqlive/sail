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

