namespace CSharpBrasil.Extensions.Caching.LiteDb;

public static class LiteDbCacheServiceCollectionExtensions
{
    public static IServiceCollection AddLiteDbDistributedCache(this IServiceCollection services, Action<LiteDbDistributedCacheOptions> configure)
    {
        services.Configure(configure);
        services.AddSingleton<LiteDbDistributedCache>();
        services.AddSingleton<IDistributedCache>(sp => sp.GetRequiredService<LiteDbDistributedCache>());
        services.AddHostedService<LiteDbCacheCleanupService>();
        return services;
    }
}