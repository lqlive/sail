using Consul;
using Consul.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddConsul(o =>
{
    o.Address = new Uri("http://127.0.0.1:8500");
});

var app = builder.Build();

var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
var consulClient = app.Services.GetRequiredService<IConsulClient>();

var serviceName = "yarp-service-discovery-sample";
var serviceId = $"{serviceName}-{Environment.MachineName}-{Environment.ProcessId}";
var serviceAddress = "127.0.0.1";
var servicePort = 5000;

var registration = new AgentServiceRegistration
{
    ID = serviceId,
    Name = serviceName,
    Address = serviceAddress,
    Port = servicePort,
    Tags = new[] { "yarp", "sample", "service-discovery" },
    Check = new AgentServiceCheck
    {
        HTTP = $"http://192.168.8.90:5000/health",
        Interval = TimeSpan.FromSeconds(10),
        Timeout = TimeSpan.FromSeconds(5),
        DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
    }
};

lifetime.ApplicationStarted.Register(async () =>
{
    await consulClient.Agent.ServiceRegister(registration);
    app.Logger.LogInformation("Service registered to Consul: {ServiceId}", serviceId);
});

lifetime.ApplicationStopping.Register(async () =>
{
    await consulClient.Agent.ServiceDeregister(serviceId);
    app.Logger.LogInformation("Service deregistered from Consul: {ServiceId}", serviceId);
});

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = serviceName }));

app.Run();