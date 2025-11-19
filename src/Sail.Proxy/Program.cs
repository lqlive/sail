using Consul.AspNetCore;

using Sail.Compass.Management;
using Sail.Core.Management;
using Sail.Core.RateLimiter;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseCertificateSelector();

builder.Services.AddSailCore();
builder.Services.AddServerCertificateSelector();
builder.Services.AddDynamicCors();
builder.Services.AddDynamicRateLimiter();
builder.Services.AddCertificateUpdater();
builder.Services.AddCorsPolicyUpdater();
builder.Services.AddRateLimiterPolicyUpdater();
builder.Services.AddReverseProxy()
    .LoadFromMessages();
 
builder.Services.AddConsul(o =>
{
    o.Address = new Uri("http://127.0.0.1:8500");
});

var app = builder.Build();

app.UseCors();

app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline.UseMiddleware<RateLimiterMiddleware>();
});


await app.RunAsync();