using Sail.Core.Entities;

namespace Sail.Models.AuthenticationPolicies;

public record AuthenticationPolicyRequest
{
    public string Name { get; init; }
    public AuthenticationSchemeType Type { get; init; }
    public bool Enabled { get; init; } = true;
    public string? Description { get; init; }
    public JwtBearerConfigRequest? JwtBearer { get; init; }
    public OpenIdConnectConfigRequest? OpenIdConnect { get; init; }
}

public record JwtBearerConfigRequest
{
    public string Authority { get; init; }
    public string Audience { get; init; }
    public bool RequireHttpsMetadata { get; init; } = true;
    public bool SaveToken { get; init; }
    public List<string>? ValidIssuers { get; init; }
    public List<string>? ValidAudiences { get; init; }
    public bool ValidateIssuer { get; init; } = true;
    public bool ValidateAudience { get; init; } = true;
    public bool ValidateLifetime { get; init; } = true;
    public bool ValidateIssuerSigningKey { get; init; } = true;
    public int? ClockSkew { get; init; }
}

public record OpenIdConnectConfigRequest
{
    public string Authority { get; init; }
    public string ClientId { get; init; }
    public string ClientSecret { get; init; }
    public string? ResponseType { get; init; } = "code";
    public bool RequireHttpsMetadata { get; init; } = true;
    public bool SaveTokens { get; init; } = true;
    public bool GetClaimsFromUserInfoEndpoint { get; init; } = true;
    public List<string>? Scope { get; init; }
    public int? ClockSkew { get; init; }
}

