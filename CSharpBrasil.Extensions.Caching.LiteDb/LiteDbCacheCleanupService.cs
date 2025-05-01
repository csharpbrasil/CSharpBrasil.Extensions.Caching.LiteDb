using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CSharpBrasil.Extensions.Caching.LiteDb;

public class LiteDbCacheCleanupService : BackgroundService
{
    private readonly LiteDbDistributedCacheOptions _options;
    private readonly LiteDbDistributedCache _cache;

    public LiteDbCacheCleanupService(IOptions<LiteDbDistributedCacheOptions> options, LiteDbDistributedCache cache)
    {
        _options = options.Value;
        _cache = cache;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.EnableAutoCleanup) return;

        while (!stoppingToken.IsCancellationRequested)
        {
            var col = _cache.Db.GetCollection<CacheEntry>(_options.CollectionName);
            var now = DateTimeOffset.UtcNow;

            var expired = col.FindAll().Where(e =>
                (e.AbsoluteExpiration.HasValue && e.AbsoluteExpiration <= now) ||
                (e.SlidingExpiration.HasValue && e.CreatedAt.Add(e.SlidingExpiration.Value) <= now)
            );

            foreach (var e in expired)
                col.Delete(e.Id);

            await Task.Delay(_options.CleanupInterval, stoppingToken);
        }
    }
}