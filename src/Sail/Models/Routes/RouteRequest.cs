namespace Sail.Models.Routes;

public record RouteRequest
{
    public Guid? ClusterId { get; init; }
    public string Name { get; init; }
    public RouteMatchRequest Match { get; init; }
    public int Order { get; init; }
    public string? AuthorizationPolicy { get; init; }
    public string? RateLimiterPolicy { get; init; }
    public string? CorsPolicy { get; init; }
    public string? TimeoutPolicy { get; init; }
    public TimeSpan? Timeout { get; init; }
    public long? MaxRequestBodySize { get; init; }
    public bool? HttpsRedirect { get; init; }
    public List<Dictionary<string, string>>? Transforms { get; set; }
}