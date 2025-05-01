namespace CSharpBrasil.Extensions.Caching.LiteDb;

public class LiteDbDistributedCacheOptions
{
    public string DatabasePath { get; set; } = "cache.db";
    public string CollectionName { get; set; } = "cache";
    public bool EnableAutoCleanup { get; set; } = true;
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(10);
    public bool ReadOnly { get; set; } = false;
    public string? Password { get; set; }
    public bool Upgrade { get; set; } = false;
    public bool AutoRebuild { get; set; } = false;
    public long InitialSize { get; set; } = 0;
    public Collation Collation { get; set; } = Collation.Default;
}