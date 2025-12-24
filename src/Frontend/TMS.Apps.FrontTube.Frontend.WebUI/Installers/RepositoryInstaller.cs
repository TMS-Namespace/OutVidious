using TMS.Apps.FrontTube.Backend.Repository.Cache;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Interfaces;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Configuration;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Installers;

/// <summary>
/// Configures data repository and caching services.
/// </summary>
internal static class RepositoryInstaller
{
    /// <summary>
    /// Adds cache manager with database configuration.
    /// </summary>
    internal static IServiceCollection AddCacheManager(
        this IServiceCollection services,
        string host,
        int port,
        string databaseName,
        string username,
        string password)
    {
        var cacheConfig = new CacheConfig
        {
            DataBase = new DataBaseConfig
            {
                Host = host,
                Port = port,
                DatabaseName = databaseName,
                Username = username,
                Password = password
            }
        };
        
        services.AddSingleton(cacheConfig);

        services.AddSingleton<ICacheManager>(sp =>
        {
            var config = sp.GetRequiredService<CacheConfig>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            return new CacheManager(config, loggerFactory);
        });

        return services;
    }
}
