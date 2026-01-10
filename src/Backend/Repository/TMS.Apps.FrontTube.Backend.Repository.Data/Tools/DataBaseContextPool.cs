using EFCoreSecondLevelCacheInterceptor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Configuration;
using TMS.Apps.FrontTube.Backend.Repository.DataBase;

namespace TMS.Apps.FrontTube.Backend.Repository.CacheManager.Tools;

/// <summary>
/// Provides a pool of <see cref="DataBaseContext"/> instances for efficient database operations.
/// </summary>
public sealed class DataBaseContextPool : IAsyncDisposable
{
    private readonly ILogger<DataBaseContextPool> _logger;

    private readonly IServiceProvider? _secondLevelCacheServices;

    private readonly PooledDbContextFactory<DataBaseContext> _factory;

    public DataBaseContextPool(DatabaseConfig dbConfig, CacheConfig cacheConfig, ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(dbConfig);
        ArgumentNullException.ThrowIfNull(cacheConfig);

        _logger = loggerFactory.CreateLogger<DataBaseContextPool>();

        var connectionString = BuildConnectionString(dbConfig);

        _secondLevelCacheServices = cacheConfig.SecondLevelCache.Enable
            ? BuildSecondLevelCacheServiceProvider(cacheConfig.SecondLevelCache, loggerFactory)
            : null;

        var optionsBuilder = new DbContextOptionsBuilder<DataBaseContext>()
            .UseNpgsql(connectionString, npgsql =>
            {
                if (dbConfig.CommandTimeoutSeconds is { } timeout)
                {
                    npgsql.CommandTimeout(timeout);
                }

                if (dbConfig.EnableRetryOnFailure)
                {
                    npgsql.EnableRetryOnFailure(
                    maxRetryCount: dbConfig.MaxRetryCount,
                    maxRetryDelay: dbConfig.MaxRetryDelay,
                    errorCodesToAdd: null);
                }
            });

        if (cacheConfig.SecondLevelCache.Enable)
        {
            var interceptor = _secondLevelCacheServices!.GetRequiredService<SecondLevelCacheInterceptor>();
            optionsBuilder.AddInterceptors(interceptor);

            if (cacheConfig.SecondLevelCache.EnableSensitiveDataLogging)
                optionsBuilder.EnableSensitiveDataLogging();

            if (cacheConfig.SecondLevelCache.EnableDetailedErrors)
                optionsBuilder.EnableDetailedErrors();
        }

        _factory = new PooledDbContextFactory<DataBaseContext>(
            optionsBuilder.Options,
            poolSize: dbConfig.DbContextPoolSize);
    }

    public async ValueTask<DataBaseContext> GetContextAsync(CancellationToken cancellationToken = default)
    {
        var db = await _factory.CreateDbContextAsync(cancellationToken);

        _logger.LogDebug("Created DataBaseContext from pool");
        return db;
    }

    public ValueTask DisposeAsync()
    {
        if (_secondLevelCacheServices is IAsyncDisposable ad)
            return ad.DisposeAsync();

        if (_secondLevelCacheServices is IDisposable d)
            d.Dispose();

        return ValueTask.CompletedTask;
    }

    private static string BuildConnectionString(DatabaseConfig config)
    {
        var csb = new NpgsqlConnectionStringBuilder
        {
            Host = config.Host,
            Port = config.Port,
            Database = config.DatabaseName,
            Username = config.Username,
            Password = config.Password,
            Pooling = config.ConnectionPoolConfig.Enabled
        };

        if (config.ConnectionPoolConfig.MinPoolSize is { } min)
            csb.MinPoolSize = min;

        if (config.ConnectionPoolConfig.MaxPoolSize is { } max)
            csb.MaxPoolSize = max;

        if (config.ConnectionPoolConfig.TimeoutSeconds is { } timeout)
            csb.Timeout = timeout;

        if (config.ConnectionPoolConfig.ConnectionIdleLifetimeSeconds is { } idle)
            csb.ConnectionIdleLifetime = idle;

        if (config.ConnectionPoolConfig.ConnectionPruningIntervalSeconds is { } prune)
            csb.ConnectionPruningInterval = prune;

        csb.IncludeErrorDetail = config.SensitiveDataLogging;

        return csb.ToString();
    }

    private static ServiceProvider BuildSecondLevelCacheServiceProvider(
        SecondLevelCacheConfig secondLevelCacheConfig,
        ILoggerFactory? loggerFactory)
    {
        var services = new ServiceCollection();

        services.AddSingleton(loggerFactory ?? LoggerFactory.Create(_ => { }));
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        services.AddMemoryCache();

        services.AddEFSecondLevelCache(options =>
        {
            options.UseMemoryCacheProvider()
                   .UseCacheKeyPrefix(secondLevelCacheConfig.CacheKeyPrefix)
                   .ConfigureLogging(secondLevelCacheConfig.EnableLogging);

            if (secondLevelCacheConfig.UseDbCallsIfCachingProviderIsDown)
                options.UseDbCallsIfCachingProviderIsDown(secondLevelCacheConfig.DbCallsIfCachingProviderIsDownTimeout);

            if (secondLevelCacheConfig.CacheAllQueries)
                options.CacheAllQueries((CacheExpirationMode)secondLevelCacheConfig.CacheAllQueriesExpirationMode, secondLevelCacheConfig.CacheAllQueriesTimeout);
        });

        return services.BuildServiceProvider(validateScopes: false);
    }
}
