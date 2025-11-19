using Sail.Core.Entities;
using Sail.Core.Stores;
using Sail.Models.AuthenticationPolicies;

namespace Sail.Services;

public class AuthenticationPolicyService
{
    private readonly IAuthenticationPolicyStore _store;

    public AuthenticationPolicyService(IAuthenticationPolicyStore store)
    {
        _store = store;
    }

    public async Task<AuthenticationPolicyResponse> CreateAsync(
        AuthenticationPolicyRequest request,
        CancellationToken cancellationToken = default)
    {
        var policy = CreatePolicyFromRequest(request);
        await _store.CreateAsync(policy, cancellationToken);
        return MapToResponse(policy);
    }

    public async Task<List<AuthenticationPolicyResponse>> GetAsync(CancellationToken cancellationToken = default)
    {
        var policies = await _store.GetAsync(cancellationToken);
        return policies.Select(MapToResponse).ToList();
    }

    public async Task<AuthenticationPolicyResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var policy = await _store.GetAsync(id, cancellationToken);
        return policy == null ? null : MapToResponse(policy);
    }

    public async Task<AuthenticationPolicyResponse> UpdateAsync(
        Guid id,
        AuthenticationPolicyRequest request,
        CancellationToken cancellationToken = default)
    {
        var existingPolicy = await _store.GetAsync(id, cancellationToken);
        if (existingPolicy == null)
        {
            throw new InvalidOperationException($"Authentication policy with ID {id} not found.");
        }

        var updatedPolicy = new AuthenticationPolicy
        {
            Id = id,
            Name = request.Name,
            Type = request.Type,
            Enabled = request.Enabled,
            Description = request.Description,
            JwtBearer = request.JwtBearer == null ? null : new JwtBearerConfig
            {
                Authority = request.JwtBearer.Authority,
                Audience = request.JwtBearer.Audience,
                RequireHttpsMetadata = request.JwtBearer.RequireHttpsMetadata,
                SaveToken = request.JwtBearer.SaveToken,
                ValidIssuers = request.JwtBearer.ValidIssuers,
                ValidAudiences = request.JwtBearer.ValidAudiences,
                ValidateIssuer = request.JwtBearer.ValidateIssuer,
                ValidateAudience = request.JwtBearer.ValidateAudience,
                ValidateLifetime = request.JwtBearer.ValidateLifetime,
                ValidateIssuerSigningKey = request.JwtBearer.ValidateIssuerSigningKey,
                ClockSkew = request.JwtBearer.ClockSkew
            },
            OpenIdConnect = request.OpenIdConnect == null ? null : new OpenIdConnectConfig
            {
                Authority = request.OpenIdConnect.Authority,
                ClientId = request.OpenIdConnect.ClientId,
                ClientSecret = request.OpenIdConnect.ClientSecret,
                ResponseType = request.OpenIdConnect.ResponseType,
                RequireHttpsMetadata = request.OpenIdConnect.RequireHttpsMetadata,
                SaveTokens = request.OpenIdConnect.SaveTokens,
                GetClaimsFromUserInfoEndpoint = request.OpenIdConnect.GetClaimsFromUserInfoEndpoint,
                Scope = request.OpenIdConnect.Scope,
                ClockSkew = request.OpenIdConnect.ClockSkew
            },
            CreatedAt = existingPolicy.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _store.UpdateAsync(updatedPolicy, cancellationToken);
        return MapToResponse(updatedPolicy);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _store.DeleteAsync(id, cancellationToken);
    }

    private static AuthenticationPolicy CreatePolicyFromRequest(AuthenticationPolicyRequest request)
    {
        return new AuthenticationPolicy
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Type = request.Type,
            Enabled = request.Enabled,
            Description = request.Description,
            JwtBearer = request.JwtBearer == null ? null : new JwtBearerConfig
            {
                Authority = request.JwtBearer.Authority,
                Audience = request.JwtBearer.Audience,
                RequireHttpsMetadata = request.JwtBearer.RequireHttpsMetadata,
                SaveToken = request.JwtBearer.SaveToken,
                ValidIssuers = request.JwtBearer.ValidIssuers,
                ValidAudiences = request.JwtBearer.ValidAudiences,
                ValidateIssuer = request.JwtBearer.ValidateIssuer,
                ValidateAudience = request.JwtBearer.ValidateAudience,
                ValidateLifetime = request.JwtBearer.ValidateLifetime,
                ValidateIssuerSigningKey = request.JwtBearer.ValidateIssuerSigningKey,
                ClockSkew = request.JwtBearer.ClockSkew
            },
            OpenIdConnect = request.OpenIdConnect == null ? null : new OpenIdConnectConfig
            {
                Authority = request.OpenIdConnect.Authority,
                ClientId = request.OpenIdConnect.ClientId,
                ClientSecret = request.OpenIdConnect.ClientSecret,
                ResponseType = request.OpenIdConnect.ResponseType,
                RequireHttpsMetadata = request.OpenIdConnect.RequireHttpsMetadata,
                SaveTokens = request.OpenIdConnect.SaveTokens,
                GetClaimsFromUserInfoEndpoint = request.OpenIdConnect.GetClaimsFromUserInfoEndpoint,
                Scope = request.OpenIdConnect.Scope,
                ClockSkew = request.OpenIdConnect.ClockSkew
            }
        };
    }

    private static AuthenticationPolicyResponse MapToResponse(AuthenticationPolicy policy)
    {
        return new AuthenticationPolicyResponse
        {
            Id = policy.Id,
            Name = policy.Name,
            Type = policy.Type,
            Enabled = policy.Enabled,
            Description = policy.Description,
            JwtBearer = policy.JwtBearer == null ? null : new JwtBearerConfigResponse
            {
                Authority = policy.JwtBearer.Authority,
                Audience = policy.JwtBearer.Audience,
                RequireHttpsMetadata = policy.JwtBearer.RequireHttpsMetadata,
                SaveToken = policy.JwtBearer.SaveToken,
                ValidIssuers = policy.JwtBearer.ValidIssuers,
                ValidAudiences = policy.JwtBearer.ValidAudiences,
                ValidateIssuer = policy.JwtBearer.ValidateIssuer,
                ValidateAudience = policy.JwtBearer.ValidateAudience,
                ValidateLifetime = policy.JwtBearer.ValidateLifetime,
                ValidateIssuerSigningKey = policy.JwtBearer.ValidateIssuerSigningKey,
                ClockSkew = policy.JwtBearer.ClockSkew
            },
            OpenIdConnect = policy.OpenIdConnect == null ? null : new OpenIdConnectConfigResponse
            {
                Authority = policy.OpenIdConnect.Authority,
                ClientId = policy.OpenIdConnect.ClientId,
                ClientSecret = policy.OpenIdConnect.ClientSecret,
                ResponseType = policy.OpenIdConnect.ResponseType,
                RequireHttpsMetadata = policy.OpenIdConnect.RequireHttpsMetadata,
                SaveTokens = policy.OpenIdConnect.SaveTokens,
                GetClaimsFromUserInfoEndpoint = policy.OpenIdConnect.GetClaimsFromUserInfoEndpoint,
                Scope = policy.OpenIdConnect.Scope,
                ClockSkew = policy.OpenIdConnect.ClockSkew
            },
            CreatedAt = policy.CreatedAt,
            UpdatedAt = policy.UpdatedAt
        };
    }
}

