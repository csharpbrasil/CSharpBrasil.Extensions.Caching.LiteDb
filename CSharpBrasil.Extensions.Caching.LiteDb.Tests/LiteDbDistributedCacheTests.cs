using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CSharpBrasil.Extensions.Caching.LiteDb.Tests;

public class LiteDbDistributedCacheTests : IDisposable
{
    private readonly string _dbPath = "test-cache.db";
    private readonly LiteDbDistributedCache _cache;
    private readonly IHostApplicationLifetime _lifetime;

    public LiteDbDistributedCacheTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IHostApplicationLifetime, TestHostApplicationLifetime>();
        var provider = services.BuildServiceProvider();

        _lifetime = provider.GetRequiredService<IHostApplicationLifetime>();

        var options = new OptionsWrapper<LiteDbDistributedCacheOptions>(new LiteDbDistributedCacheOptions
        {
            DatabasePath = _dbPath,
            CollectionName = "cache",
            EnableAutoCleanup = false
        });

        _cache = new LiteDbDistributedCache(options, _lifetime);
    }

    [Fact]
    public void SetAndGet_ShouldReturnValue()
    {
        var key = "test-key";
        var value = Encoding.UTF8.GetBytes("hello");

        _cache.Set(key, value, new DistributedCacheEntryOptions());
        var result = _cache.Get(key);

        result.Should().NotBeNull();
        Encoding.UTF8.GetString(result!).Should().Be("hello");
    }

    [Fact]
    public void Get_ShouldReturnNull_WhenKeyDoesNotExist()
    {
        var result = _cache.Get("non-existent-key");
        result.Should().BeNull();
    }

    [Fact]
    public void Set_WithAbsoluteExpiration_ShouldExpire()
    {
        var key = "expire-key";
        var value = Encoding.UTF8.GetBytes("will expire");

        _cache.Set(key, value, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(300)
        });

        Thread.Sleep(1000);
        var result = _cache.Get(key);
        result.Should().BeNull();
    }

    [Fact]
    public void Remove_ShouldDeleteEntry()
    {
        var key = "remove-key";
        var value = Encoding.UTF8.GetBytes("to remove");

        _cache.Set(key, value, new DistributedCacheEntryOptions());
        _cache.Remove(key);

        var result = _cache.Get(key);
        result.Should().BeNull();
    }

    public void Dispose()
    {
        _cache.Dispose();
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }

    private class TestHostApplicationLifetime : IHostApplicationLifetime
    {
        public CancellationToken ApplicationStarted => CancellationToken.None;
        public CancellationToken ApplicationStopping => CancellationToken.None;
        public CancellationToken ApplicationStopped => CancellationToken.None;

        public void StopApplication() { }
    }
}