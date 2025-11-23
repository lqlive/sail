namespace Sail.Core.Entities;

public class AuthenticationPolicy
{
    public Guid Id { get; set; }
    public required string Name { get; init; }
    public AuthenticationSchemeType Type { get; init; }
    public bool Enabled { get; init; } = true;
    public string? Description { get; init; }

    public JwtBearerConfig? JwtBearer { get; init; }
    public OpenIdConnectConfig? OpenIdConnect { get; init; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
}

public enum AuthenticationSchemeType
{
    JwtBearer,
    OpenIdConnect
}

public class JwtBearerConfig
{
    public required string Authority { get; init; }
    public required string Audience { get; init; }
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

public class OpenIdConnectConfig
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

