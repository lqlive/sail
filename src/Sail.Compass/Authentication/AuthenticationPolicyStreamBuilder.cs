using System.Reactive.Linq;
using Sail.Api.V1;
using Sail.Compass.Observers;
using Sail.Core.Authentication;
using Sail.Core.Authentication.JwtBearer;
using Sail.Core.Authentication.OpenIdConnect;
using Sail.Core.Entities;

namespace Sail.Compass.Authentication;

public static class AuthenticationPolicyStreamBuilder
{
    public static IObservable<IReadOnlyList<AuthenticationPolicyConfig>> Build(AuthenticationPolicyObserver observer)
    {
        return observer.GetObservable(true)
            .Scan(new Dictionary<string, AuthenticationPolicyConfig>(), (policies, @event) =>
            {
                var updated = new Dictionary<string, AuthenticationPolicyConfig>(policies);

                switch (@event.EventType)
                {
                    case Observers.EventType.Created:
                    case Observers.EventType.Updated:
                    case Observers.EventType.List:
                        HandlePolicyUpdate(updated, @event.Value);
                        break;

                    case Observers.EventType.Deleted:
                        updated.Remove(@event.Value.Name);
                        break;
                }

                return updated;
            })
            .Throttle(TimeSpan.FromMilliseconds(100))
            .Select(policies => (IReadOnlyList<AuthenticationPolicyConfig>)policies.Values.ToList());
    }

    private static void HandlePolicyUpdate(Dictionary<string, AuthenticationPolicyConfig> policies, Api.V1.AuthenticationPolicy policy)
    {
        if (!policy.Enabled)
        {
            policies.Remove(policy.Name);
            return;
        }

        var config = CreatePolicyConfig(policy);
        if (config != null)
        {
            policies[policy.Name] = config;
        }
        else
        {
            policies.Remove(policy.Name);
        }
    }

    private static AuthenticationPolicyConfig? CreatePolicyConfig(Api.V1.AuthenticationPolicy policy)
    {
        return policy.JwtBearer != null
            ? CreateJwtBearerConfig(policy)
            : policy.OpenIdConnect != null
                ? CreateOpenIdConnectConfig(policy)
                : null;
    }

    private static AuthenticationPolicyConfig CreateJwtBearerConfig(Api.V1.AuthenticationPolicy policy)
    {
        return new AuthenticationPolicyConfig
        {
            Name = policy.Name,
            Type = MapAuthenticationSchemeType(policy.Type),
            JwtBearer = new JwtBearerConfiguration
            {
                Authority = policy.JwtBearer.Authority,
                Audience = policy.JwtBearer.Audience,
                RequireHttpsMetadata = policy.JwtBearer.RequireHttpsMetadata,
                SaveToken = policy.JwtBearer.SaveToken,
                ValidIssuers = policy.JwtBearer.ValidIssuers.Count > 0
                    ? policy.JwtBearer.ValidIssuers.ToList()
                    : null,
                ValidAudiences = policy.JwtBearer.ValidAudiences.Count > 0
                    ? policy.JwtBearer.ValidAudiences.ToList()
                    : null,
                ValidateIssuer = policy.JwtBearer.ValidateIssuer,
                ValidateAudience = policy.JwtBearer.ValidateAudience,
                ValidateLifetime = policy.JwtBearer.ValidateLifetime,
                ValidateIssuerSigningKey = policy.JwtBearer.ValidateIssuerSigningKey,
                ClockSkew = policy.JwtBearer.ClockSkew
            }
        };
    }

    private static AuthenticationPolicyConfig CreateOpenIdConnectConfig(Api.V1.AuthenticationPolicy policy)
    {
        return new AuthenticationPolicyConfig
        {
            Name = policy.Name,
            Type = MapAuthenticationSchemeType(policy.Type),
            OpenIdConnect = new OpenIdConnectConfiguration
            {
                Authority = policy.OpenIdConnect.Authority,
                ClientId = policy.OpenIdConnect.ClientId,
                ClientSecret = policy.OpenIdConnect.ClientSecret,
                ResponseType = policy.OpenIdConnect.ResponseType,
                RequireHttpsMetadata = policy.OpenIdConnect.RequireHttpsMetadata,
                SaveTokens = policy.OpenIdConnect.SaveTokens,
                GetClaimsFromUserInfoEndpoint = policy.OpenIdConnect.GetClaimsFromUserInfoEndpoint,
                Scope = policy.OpenIdConnect.Scope.Count > 0
                    ? policy.OpenIdConnect.Scope.ToList()
                    : null,
                ClockSkew = policy.OpenIdConnect.ClockSkew
            }
        };
    }

    private static Core.Entities.AuthenticationSchemeType MapAuthenticationSchemeType(Api.V1.AuthenticationSchemeType type)
    {
        return type == Api.V1.AuthenticationSchemeType.JwtBearer
            ? Core.Entities.AuthenticationSchemeType.JwtBearer
            : Core.Entities.AuthenticationSchemeType.OpenIdConnect;
    }
}

