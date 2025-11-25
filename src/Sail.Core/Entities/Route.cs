namespace Sail.Core.Entities;

public class Route
{
    public Guid Id { get; set; }
    public Guid? ClusterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public RouteMatch Match { get; set; } = new();
    public int Order { get; set; }
    public string? AuthorizationPolicy { get; set; }
    public string? RateLimiterPolicy { get; set; }
    public string? CorsPolicy { get; set; }
    public string? TimeoutPolicy { get; set; }
    public TimeSpan? Timeout { get; set; }
    public long? MaxRequestBodySize { get; set; }
    public bool? HttpsRedirect { get; set; }
    public List<Dictionary<string, string>>? Transforms { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}