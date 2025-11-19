namespace Sail.Core.Cors;

public class CorsPolicyConfig
{
    public required string Name { get; init; }
    public List<string>? AllowOrigins { get; init; }
    public List<string>? AllowMethods { get; init; }
    public List<string>? AllowHeaders { get; init; }
    public List<string>? ExposeHeaders { get; init; }
    public bool AllowCredentials { get; init; }
    public int? MaxAge { get; init; }
}
