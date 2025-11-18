using Yarp.ReverseProxy.Forwarder;
using YarpSample;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor(); 

builder.Services.AddTransient<CustomDelegatingHandler>();
builder.Services.AddSingleton<IForwarderHttpClientFactory, CustomForwarderHttpClientFactory>();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();
app.MapReverseProxy();

app.UseRouting();

app.Run();
