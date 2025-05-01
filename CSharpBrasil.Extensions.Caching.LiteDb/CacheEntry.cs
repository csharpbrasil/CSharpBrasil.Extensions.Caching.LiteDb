using LiteDB;

namespace CSharpBrasil.Extensions.Caching.LiteDb;

public class CacheEntry
{
    [BsonId]
    public string Id { get; set; } = default!;
    public byte[]? Value { get; set; }
    public DateTimeOffset? AbsoluteExpiration { get; set; }
    public TimeSpan? SlidingExpiration { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
