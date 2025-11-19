namespace Sail.Core.Authentication.OpenIdConnect;

public class OpenIdConnectConfiguration
{
    public required string Authority { get; init; }
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public string? ResponseType { get; init; } = "code";
    public bool RequireHttpsMetadata { get; init; } = true;
    public bool SaveTokens { get; init; } = true;
    public bool GetClaimsFromUserInfoEndpoint { get; init; } = true;
    public List<string>? Scope { get; init; }
    public int? ClockSkew { get; init; }
}

