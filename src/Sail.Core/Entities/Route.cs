namespace Sail.Core.Entities;

public class Route
{
    public Guid Id { get; set; }
    public Guid? ClusterId { get; init; }
    public required string Name { get; init; }
    public required RouteMatch Match { get; init; }
    public int Order { get; init; }
    public string? AuthorizationPolicy { get; init; }
    public string? RateLimiterPolicy { get; init; }
    public string? CorsPolicy { get; init; }
    public string? TimeoutPolicy { get; init; }
    public TimeSpan? Timeout { get; init; }
    public long? MaxRequestBodySize { get; init; }
    public bool HttpsRedirect { get; init; }
    public IReadOnlyList<IReadOnlyDictionary<string, string>>? Transforms { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
}