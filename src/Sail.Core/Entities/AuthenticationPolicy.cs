namespace Sail.Core.Entities;

public class AuthenticationPolicy
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AuthenticationSchemeType Type { get; set; }
    public bool Enabled { get; set; } = true;
    public string? Description { get; set; }

    public JwtBearerConfig? JwtBearer { get; set; }
    public OpenIdConnectConfig? OpenIdConnect { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public enum AuthenticationSchemeType
{
    JwtBearer,
    OpenIdConnect
}

public class JwtBearerConfig
{
    public string Authority { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public bool RequireHttpsMetadata { get; set; } = true;
    public bool SaveToken { get; set; }
    public List<string>? ValidIssuers { get; set; }
    public List<string>? ValidAudiences { get; set; }
    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateAudience { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;
    public bool ValidateIssuerSigningKey { get; set; } = true;
    public int? ClockSkew { get; set; }
}

public class OpenIdConnectConfig
{
    public string Authority { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string? ResponseType { get; set; } = "code";
    public bool RequireHttpsMetadata { get; set; } = true;
    public bool SaveTokens { get; set; } = true;
    public bool GetClaimsFromUserInfoEndpoint { get; set; } = true;
    public List<string>? Scope { get; set; }
    public int? ClockSkew { get; set; }
}

