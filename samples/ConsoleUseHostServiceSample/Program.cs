var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddLiteDbDistributedCache(options =>
        {
            options.DatabasePath = "console-cache.db";
            options.CollectionName = "cache";
        });
    })
    .Build();

var cache = host.Services.GetRequiredService<IDistributedCache>();

const string key = "welcome-message";
const string message = "Hello from LiteDb cache!";

await cache.SetStringAsync(key, message, new DistributedCacheEntryOptions
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
});

var cached = await cache.GetStringAsync(key);
Console.WriteLine($"Cached: {cached}");
