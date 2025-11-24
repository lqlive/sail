using ApiSample;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<DynamicRateLimitConfigurationProvider>();
builder.Services.AddRateLimiter();

var app = builder.Build();

app.MapGet("/", async (HttpContext context) =>
{
    Console.WriteLine(Random.Shared.Next(10000, 90000));

    return Results.Ok(111);
});

app.MapGet("/test", async (HttpContext context, DynamicRateLimitConfigurationProvider provider) =>
{
    Console.WriteLine(Random.Shared.Next(10000, 90000));
    provider.AddRateLimitOptions();
    return Results.Ok(111);
});

var rateLimitOptions = app.Services.GetRequiredService<DynamicRateLimitConfigurationProvider>().GetRateLimitOptions();
app.UseRateLimiter(rateLimitOptions);
app.Run();



