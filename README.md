# CSharpBrasil.Extensions.Caching.LiteDb

A lightweight and embeddable implementation of `IDistributedCache` using [LiteDB](https://www.litedb.org/), designed for .NET applications that require local distributed caching without external dependencies such as Redis or SQL Server.

## Features

- Local persistent storage using a single `.db` file
- Supports absolute and sliding expiration
- Fully managed and dependency-free in .NET
- Compatible with ASP.NET Core and .NET 6/7/8+
- Background cleanup of expired cache entries
- Easy integration via `IServiceCollection`

## Installation

Install via NuGet:

```bash
dotnet add package CSharpBrasil.Extensions.Caching.LiteDb
```

## Usage

### Configuration (Program.cs)

```csharp
builder.Services.AddLiteDbDistributedCache(options =>
{
    options.DatabasePath = "cache.db";
    options.CollectionName = "cache";
    options.EnableAutoCleanup = true;
    options.CleanupInterval = TimeSpan.FromMinutes(5);
});
```

### Example

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

## License

This project is licensed under the [MIT License](https://chatgpt.com/g/g-p-680e91f517d48191aaeede7438598dd6-c-brasil/c/LICENSE) © C# Brasil.

---
