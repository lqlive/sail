namespace Sail.Cluster.Models;

public record SessionAffinityCookieRequest
{
    public string? Path { get; init; }
    public string? Domain { get; init; }
    public bool? HttpOnly { get; init; }
    public CookieSecurePolicy? SecurePolicy { get; init; }
    public SameSiteMode? SameSite { get; init; }
    public TimeSpan? Expiration { get; init; }
    public TimeSpan? MaxAge { get; init; }
    public bool? IsEssential { get; init; }
}
