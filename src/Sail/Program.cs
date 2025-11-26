using System.Text.Json.Serialization;
using Sail.Apis;
using Sail.Core.Management;
using Sail.Database.MongoDB.Management;
using Sail.Extensions;
using Sail.Grpc;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var apiVersioning = builder.Services.AddApiVersioning();

builder.AddServiceDefaults();
builder.AddDefaultOpenApi(apiVersioning);
builder.AddApplicationServices();
builder.Services.AddSailCore()
    .AddDatabaseStore();
builder.Services.AddGrpc();

var app = builder.Build();

var endpoint = app.NewVersionedApi();

endpoint.MapRouteApiV1();
endpoint.MapClusterApiV1();
endpoint.MapCertificateApiV1();
endpoint.MapMiddlewareApiV1();
endpoint.MapAuthenticationPolicyApi();

app.UseDefaultOpenApi();

app.MapGrpcService<RouteGrpcService>();
app.MapGrpcService<ClusterGrpcService>();
app.MapGrpcService<CertificateGrpcService>();
app.MapGrpcService<MiddlewareGrpcService>();
app.MapGrpcService<AuthenticationPolicyGrpcService>();

await app.RunAsync();
