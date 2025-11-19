namespace Sail.Core.Authentication.JwtBearer;

public class JwtBearerConfiguration
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

