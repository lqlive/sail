using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MongoDB.Driver;
using Sail.Api.V1;
using Sail.Core.Stores;
using Sail.Database.MongoDB;
using Sail.Database.MongoDB.Extensions;

namespace Sail.Grpc;

public class AuthenticationPolicyGrpcService(SailContext dbContext, IAuthenticationPolicyStore policyStore) 
    : Api.V1.AuthenticationPolicyService.AuthenticationPolicyServiceBase
{
    public override async Task<ListAuthenticationPolicyResponse> List(Empty request, ServerCallContext context)
    {
        var policies = await policyStore.GetAsync(context.CancellationToken);
        
        var response = new ListAuthenticationPolicyResponse();
        foreach (var policy in policies)
        {
            response.Items.Add(ConvertToProto(policy));
        }
        
        return response;
    }

    public override async Task Watch(Empty request, IServerStreamWriter<WatchAuthenticationPolicyResponse> responseStream, ServerCallContext context)
    {
        var options = new ChangeStreamOptions
        {
            FullDocument = ChangeStreamFullDocumentOption.Required,
            FullDocumentBeforeChange = ChangeStreamFullDocumentBeforeChangeOption.Required
        };

        while (!context.CancellationToken.IsCancellationRequested)
        {
            var watch = await dbContext.AuthenticationPolicies.WatchAsync(options, context.CancellationToken);

            await foreach (var changeStreamDocument in watch.ToAsyncEnumerable())
            {
                var document = changeStreamDocument.FullDocument;
                if (changeStreamDocument.OperationType == ChangeStreamOperationType.Delete)
                {
                    document = changeStreamDocument.FullDocumentBeforeChange;
                }

                var eventType = changeStreamDocument.OperationType switch
                {
                    ChangeStreamOperationType.Insert => EventType.Create,
                    ChangeStreamOperationType.Update => EventType.Update,
                    ChangeStreamOperationType.Delete => EventType.Delete,
                    _ => EventType.Unknown
                };
                
                var response = new WatchAuthenticationPolicyResponse
                {
                    Policy = ConvertToProto(document),
                    EventType = eventType
                };
                
                await responseStream.WriteAsync(response);
            }
        }
    }

    private static Api.V1.AuthenticationPolicy ConvertToProto(Core.Entities.AuthenticationPolicy policy)
    {
        var proto = new Api.V1.AuthenticationPolicy
        {
            PolicyId = policy.Id.ToString(),
            Name = policy.Name,
            Type = policy.Type == Core.Entities.AuthenticationSchemeType.JwtBearer 
                ? Api.V1.AuthenticationSchemeType.JwtBearer 
                : Api.V1.AuthenticationSchemeType.OpenIdConnect,
            Enabled = policy.Enabled
        };

        if (!string.IsNullOrEmpty(policy.Description))
        {
            proto.Description = policy.Description;
        }

        if (policy.JwtBearer != null)
        {
            proto.JwtBearer = new Api.V1.JwtBearerConfig
            {
                Authority = policy.JwtBearer.Authority,
                Audience = policy.JwtBearer.Audience,
                RequireHttpsMetadata = policy.JwtBearer.RequireHttpsMetadata,
                SaveToken = policy.JwtBearer.SaveToken,
                ValidateIssuer = policy.JwtBearer.ValidateIssuer,
                ValidateAudience = policy.JwtBearer.ValidateAudience,
                ValidateLifetime = policy.JwtBearer.ValidateLifetime,
                ValidateIssuerSigningKey = policy.JwtBearer.ValidateIssuerSigningKey
            };

            if (policy.JwtBearer.ValidIssuers != null)
            {
                proto.JwtBearer.ValidIssuers.AddRange(policy.JwtBearer.ValidIssuers);
            }

            if (policy.JwtBearer.ValidAudiences != null)
            {
                proto.JwtBearer.ValidAudiences.AddRange(policy.JwtBearer.ValidAudiences);
            }

            if (policy.JwtBearer.ClockSkew.HasValue)
            {
                proto.JwtBearer.ClockSkew = policy.JwtBearer.ClockSkew.Value;
            }
        }

        if (policy.OpenIdConnect != null)
        {
            proto.OpenIdConnect = new Api.V1.OpenIdConnectConfig
            {
                Authority = policy.OpenIdConnect.Authority,
                ClientId = policy.OpenIdConnect.ClientId,
                ClientSecret = policy.OpenIdConnect.ClientSecret,
                RequireHttpsMetadata = policy.OpenIdConnect.RequireHttpsMetadata,
                SaveTokens = policy.OpenIdConnect.SaveTokens,
                GetClaimsFromUserInfoEndpoint = policy.OpenIdConnect.GetClaimsFromUserInfoEndpoint
            };

            if (!string.IsNullOrEmpty(policy.OpenIdConnect.ResponseType))
            {
                proto.OpenIdConnect.ResponseType = policy.OpenIdConnect.ResponseType;
            }

            if (policy.OpenIdConnect.Scope != null)
            {
                proto.OpenIdConnect.Scope.AddRange(policy.OpenIdConnect.Scope);
            }

            if (policy.OpenIdConnect.ClockSkew.HasValue)
            {
                proto.OpenIdConnect.ClockSkew = policy.OpenIdConnect.ClockSkew.Value;
            }
        }

        return proto;
    }
}

