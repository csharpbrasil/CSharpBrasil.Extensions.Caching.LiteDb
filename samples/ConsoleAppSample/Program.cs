using Microsoft.Extensions.Caching.Distributed;
using CSharpBrasil.Extensions.Caching.LiteDb;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddLiteDbDistributedCache(options =>
{
    options.DatabasePath = "sample-cache.db";
    options.CollectionName = "cache";
    options.EnableAutoCleanup = true;
    options.CleanupInterval = TimeSpan.FromMinutes(1);
});

var provider = services.BuildServiceProvider();
var cache = provider.GetRequiredService<IDistributedCache>();


const string key = "welcome-message";
const string message = "Hello from LiteDb cache!";

// Set
await cache.SetStringAsync(key, message, new DistributedCacheEntryOptions
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
});

// Get
var cached = await cache.GetStringAsync(key);
Console.WriteLine($"Cached: {cached}");
