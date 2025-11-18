using Consul.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceDiscovery(options =>
    {
        options.RefreshPeriod = TimeSpan.FromSeconds(10);
        options.AllowedSchemes = new List<string> { "http" };
    })
    .AddConsulSrvServiceEndpointProvider();

builder.Services.AddConsul(o =>
{
    o.Address = new Uri("http://127.0.0.1:8500");
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();
app.MapReverseProxy();
app.Run();