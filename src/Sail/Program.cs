using Sail.Core.Entities;
using Sail.Core.Management;
using Sail.Extensions;
using ServiceDefaults;
using Sail.Database.MongoDB.Management;
using System.Text.Json.Serialization;
using Sail.Route.Grpc;
using Sail.Route.Http;
using Sail.Cluster.Grpc;
using Sail.Cluster.Http;
using Sail.Certificate.Grpc;
using Sail.Certificate.Http;
using Sail.Middleware.Grpc;
using Sail.Middleware.Http;
using Sail.AuthenticationPolicy.Grpc;
using Sail.AuthenticationPolicy.Http;
using Sail.Statistics.Http;

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

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<IContext>();
    await context.Database.EnsureCreatedAsync();
}

var endpoint = app.NewVersionedApi();

endpoint.MapRouteApiV1();
endpoint.MapClusterApiV1();
endpoint.MapCertificateApiV1();
endpoint.MapMiddlewareApiV1();
endpoint.MapAuthenticationPolicyApi();
endpoint.MapStatisticsApiV1();

app.UseDefaultOpenApi();
app.MapGrpcService<RouteGrpcService>();
app.MapGrpcService<ClusterGrpcService>();
app.MapGrpcService<CertificateGrpcService>();
app.MapGrpcService<MiddlewareGrpcService>();
app.MapGrpcService<AuthenticationPolicyGrpcService>();

await app.RunAsync();
