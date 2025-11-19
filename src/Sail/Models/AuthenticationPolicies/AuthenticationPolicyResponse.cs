using Sail.Core.Entities;

namespace Sail.Models.AuthenticationPolicies;

public record AuthenticationPolicyResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public AuthenticationSchemeType Type { get; init; }
    public bool Enabled { get; init; }
    public string? Description { get; init; }
    public JwtBearerConfigResponse? JwtBearer { get; init; }
    public OpenIdConnectConfigResponse? OpenIdConnect { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public record JwtBearerConfigResponse
{
    public string Authority { get; init; }
    public string Audience { get; init; }
    public bool RequireHttpsMetadata { get; init; }
    public bool SaveToken { get; init; }
    public List<string>? ValidIssuers { get; init; }
    public List<string>? ValidAudiences { get; init; }
    public bool ValidateIssuer { get; init; }
    public bool ValidateAudience { get; init; }
    public bool ValidateLifetime { get; init; }
    public bool ValidateIssuerSigningKey { get; init; }
    public int? ClockSkew { get; init; }
}

public record OpenIdConnectConfigResponse
{
    public string Authority { get; init; }
    public string ClientId { get; init; }
    public string ClientSecret { get; init; }
    public string? ResponseType { get; init; }
    public bool RequireHttpsMetadata { get; init; }
    public bool SaveTokens { get; init; }
    public bool GetClaimsFromUserInfoEndpoint { get; init; }
    public List<string>? Scope { get; init; }
    public int? ClockSkew { get; init; }
}

