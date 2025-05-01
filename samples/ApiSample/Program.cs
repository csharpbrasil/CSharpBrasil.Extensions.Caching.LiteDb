using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using CSharpBrasil.Extensions.Caching.LiteDb;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLiteDbDistributedCache(options =>
{
    options.DatabasePath = Path.Combine(AppContext.BaseDirectory, "api-cache.db");
    options.CollectionName = "cache";
    options.EnableAutoCleanup = true;
    options.CleanupInterval = TimeSpan.FromMinutes(2);
});

var app = builder.Build();

app.MapPost("/cache", async ([FromBody] KeyValuePair<string, string> value, IDistributedCache cache) =>
{
    await cache.SetStringAsync(value.Key, value.Value, new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
    });

    return Results.Ok("Cached.");
});

app.MapGet("/cache", async ([FromQuery] string key, IDistributedCache cache) =>
{
    var value = await cache.GetStringAsync(key);
    return value is not null ? Results.Ok(value) : Results.NotFound();
});

app.Run();
