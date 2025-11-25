using Sail.Compass.Management;
using Sail.Core.Management;
using Sail.Core.RateLimiter;
using Sail.Core.Https;
using Sail.Proxy.Extensions;
using Sail.Core.Retry;
using Sail.Core.ServiceDiscovery;
using Sail.Proxy.Apis;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseCertificateSelector();

builder.Services.AddSailCore();
builder.Services.AddServerCertificateSelector();
builder.Services.AddSailServiceDiscovery(builder.Configuration);
builder.Services.AddReverseProxy()
    .LoadFromMessages()
    .AddServiceDiscoveryDestinationResolver();
builder.Services.AddSailCors();
builder.Services.AddSailRateLimiter();
builder.Services.AddSailAuthentication();
builder.Services.AddSailTimeout();
builder.Services.AddSailRetry();
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
builder.Services.AddRetryPolicyUpdater();

var app = builder.Build();

app.MapRuntimeV1();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRequestTimeouts();

app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline.UseMiddleware<HttpsRedirectionMiddleware>();
    proxyPipeline.UseMiddleware<RateLimiterMiddleware>();
    proxyPipeline.UseMiddleware<RetryMiddleware>();
});

await app.RunAsync();