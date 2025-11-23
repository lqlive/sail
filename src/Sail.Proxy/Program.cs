using Consul.AspNetCore;
using Microsoft.AspNetCore.Http;
using Sail.Compass.Management;
using Sail.Core.Management;
using Sail.Core.RateLimiter;
using Sail.Core.Https;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseCertificateSelector();

builder.Services.AddSailCore();
builder.Services.AddServerCertificateSelector();
builder.Services.AddReverseProxy()
    .LoadFromMessages()
    .AddServiceDiscoveryDestinationResolver();
builder.Services.AddSailCors();
builder.Services.AddSailRateLimiter();
builder.Services.AddSailAuthentication();
builder.Services.AddSailTimeout();
builder.Services.AddRouteHttpsRedirection(options =>
{
    options.HttpsPort = 443;
    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
});
builder.Services.AddCertificateUpdater();
builder.Services.AddCorsPolicyUpdater();
builder.Services.AddRateLimiterPolicyUpdater();
builder.Services.AddAuthenticationPolicyUpdater();
builder.Services.AddTimeoutPolicyUpdater();

builder.Services.AddConsul(o =>
{
    o.Address = new Uri("http://127.0.0.1:8500");
});

var app = builder.Build();

app.Services.UseCompassUpdaters();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRequestTimeouts();

app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline.UseMiddleware<HttpsRedirectionMiddleware>();
    proxyPipeline.UseMiddleware<RateLimiterMiddleware>();
});

await app.RunAsync();