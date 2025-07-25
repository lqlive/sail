using Consul.AspNetCore;
using Sail.Core.Management;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseCertificateSelector();

builder.Services.AddSailCore();
builder.Services.AddServerCertificateSelector();
builder.Services.AddServiceDiscovery()
    .AddConsulSrvServiceEndpointProvider();
builder.Services.AddConsul(o =>
{
    o.Address = new Uri("http://127.0.0.1:8500");
});

var app = builder.Build();

app.MapReverseProxy();
await app.RunAsync();