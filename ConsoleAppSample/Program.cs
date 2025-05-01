var options = new LiteDbDistributedCacheOptions
{
    DatabasePath = "console-cache.db",
    CollectionName = "cache"   
};
var cacheOptions = new OptionsWrapper<LiteDbDistributedCacheOptions>(options);
var cache = new LiteDbDistributedCache(cacheOptions);

const string key = "welcome-message";
const string message = "Hello from LiteDb cache!";

await cache.SetStringAsync(key, message, new DistributedCacheEntryOptions
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
});

var cached = await cache.GetStringAsync(key);
Console.WriteLine($"Cached: {cached}");