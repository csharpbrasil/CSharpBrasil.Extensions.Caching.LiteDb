using LiteDB;
using LiteDB.Engine;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CSharpBrasil.Extensions.Caching.LiteDb;

public class LiteDbDistributedCache : IDistributedCache, IDisposable
{
    private readonly LiteDbDistributedCacheOptions _options;
    internal readonly LiteDatabase Db;

    public LiteDbDistributedCache(IOptions<LiteDbDistributedCacheOptions> options, IHostApplicationLifetime lifetime)
    {
        _options = options.Value;
        
        var engineSettings = new EngineSettings
        {
            Filename = _options.DatabasePath,
            ReadOnly = _options.ReadOnly,
            Password = _options.Password,
            Upgrade = _options.Upgrade,
            AutoRebuild = _options.AutoRebuild,
            InitialSize = _options.InitialSize,
            Collation = _options.Collation
        };
        var engine = new LiteEngine(engineSettings);
        Db = new LiteDatabase(engine, disposeOnClose: true);

        lifetime.ApplicationStopping.Register(() => Db.Dispose());
    }

    public byte[]? Get(string key)
    {
        var col = Db.GetCollection<CacheEntry>(_options.CollectionName);
        var entry = col.FindById(key);

        if (entry == null || IsExpired(entry))
        {
            col.Delete(key);
            return null;
        }

        UpdateSlidingExpiration(entry, col);
        return entry.Value;
    }

    public async Task<byte[]?> GetAsync(string key, CancellationToken token = default) => await Task.FromResult(Get(key));

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        var col = Db.GetCollection<CacheEntry>(_options.CollectionName);

        var entry = new CacheEntry
        {
            Id = key,
            Value = value,
            AbsoluteExpiration = options.AbsoluteExpiration ??
                                 (options.AbsoluteExpirationRelativeToNow.HasValue
                                  ? DateTimeOffset.UtcNow.Add(options.AbsoluteExpirationRelativeToNow.Value)
                                  : null),
            SlidingExpiration = options.SlidingExpiration,
            CreatedAt = DateTimeOffset.UtcNow
        };

        col.Upsert(entry);
    }

    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) =>
        await Task.Run(() => Set(key, value, options), token);

    public void Refresh(string key)
    {
        var col = Db.GetCollection<CacheEntry>(_options.CollectionName);
        var entry = col.FindById(key);
        if (entry != null) UpdateSlidingExpiration(entry, col);
    }

    public async Task RefreshAsync(string key, CancellationToken token = default) => await Task.Run(() => Refresh(key), token);

    public void Remove(string key)
    {
        var col = Db.GetCollection<CacheEntry>(_options.CollectionName);
        col.Delete(key);
    }

    public async Task RemoveAsync(string key, CancellationToken token = default) => await Task.Run(() => Remove(key), token);

    private bool IsExpired(CacheEntry entry)
    {
        var now = DateTimeOffset.UtcNow;
        if (entry.AbsoluteExpiration.HasValue && entry.AbsoluteExpiration <= now)
            return true;
        if (entry.SlidingExpiration.HasValue && entry.CreatedAt.Add(entry.SlidingExpiration.Value) <= now)
            return true;
        return false;
    }

    private void UpdateSlidingExpiration(CacheEntry entry, ILiteCollection<CacheEntry> col)
    {
        if (entry.SlidingExpiration.HasValue)
        {
            entry.CreatedAt = DateTimeOffset.UtcNow;
            col.Update(entry);
        }
    }

    public void Dispose()
    {
        Db.Dispose();
    }
}
