namespace Sail.Route.Models;

public record RouteResponse
{
    public Guid Id { get; init; }
    public Guid? ClusterId { get; init; }
    public string Name { get; init; }
    public RouteMatchResponse Match { get; init; }
    public int Order { get; init; }
    public string? AuthorizationPolicy { get; init; }
    public string? RateLimiterPolicy { get; init; }
    public string? CorsPolicy { get; init; }
    public string? TimeoutPolicy { get; init; }
    public TimeSpan? Timeout { get; init; }
    public long? MaxRequestBodySize { get; init; }
    public bool? HttpsRedirect { get; init; }
    public List<Dictionary<string, string>>? Transforms { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}