# CSharpBrasil.Extensions.Caching.LiteDb

[![NuGet](https://img.shields.io/nuget/v/CSharpBrasil.Extensions.Caching.LiteDb.svg)](https://www.nuget.org/packages/CSharpBrasil.Extensions.Caching.LiteDb)
[![Downloads](https://img.shields.io/nuget/dt/CSharpBrasil.Extensions.Caching.LiteDb.svg)](https://www.nuget.org/packages/CSharpBrasil.Extensions.Caching.LiteDb)

![](icon.png)

A lightweight and embeddable implementation of `IDistributedCache` using [LiteDB](https://www.litedb.org/), designed for .NET applications that require local distributed caching without external dependencies such as Redis or SQL Server.

## Features

- Local persistent storage using a single `.db` file
- Supports absolute and sliding expiration
- Fully managed and dependency-free in .NET
- Compatible with ASP.NET Core, Console Apps and .NET 6/7/8+
- Background cleanup of expired cache entries
- Uses a single `LiteDatabase` instance with `FileMode.Exclusive`
- Integrates cleanly via `IServiceCollection`

## Installation

Install via NuGet:

```bash
dotnet add package CSharpBrasil.Extensions.Caching.LiteDb
```

## Configuration Options

```csharp
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
```

## ✅ Usage in ASP.NET Core (Minimal API or MVC)

```csharp
builder.Services.AddLiteDbDistributedCache(options =>
{
    options.DatabasePath = "cache.db";
    options.CollectionName = "cache";
    options.EnableAutoCleanup = true;
    options.CleanupInterval = TimeSpan.FromMinutes(5);
    options.Password = "secure123";
});
```

### Controller Example

```csharp
[ApiController]
[Route("api/cache")]
public class CacheController : ControllerBase
{
    private readonly IDistributedCache _cache;

    public CacheController(IDistributedCache cache)
    {
        _cache = cache;
    }

    [HttpPost("{key}")]
    public async Task<IActionResult> Set(string key, [FromBody] string value)
    {
        await _cache.SetStringAsync(key, value, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        });
        return Ok();
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> Get(string key)
    {
        var value = await _cache.GetStringAsync(key);
        return Ok(value ?? "(not found)");
    }
}
```

## ✅ Usage in a Console Application (with Host)

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using CSharpBrasil.Extensions.Caching.LiteDb;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddLiteDbDistributedCache(options =>
        {
            options.DatabasePath = "console-cache.db";
            options.CollectionName = "cache";
            options.Password = "secure123";
        });
    })
    .Build();

var cache = host.Services.GetRequiredService<IDistributedCache>();

await cache.SetStringAsync("message", "Olá mundo!", new DistributedCacheEntryOptions
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
});

var result = await cache.GetStringAsync("message");
Console.WriteLine($"Mensagem em cache: {result}");

await host.StopAsync();
```

## License

This project is licensed under the [MIT License](LICENSE) © [C# Brasil](https://csharpbrasil.com.br).

