using System.Linq.Expressions;
using System.Reflection;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Configuration;

namespace TMS.Apps.FrontTube.Backend.Repository.DataBase;

/// <summary>
/// Provides a pool of <see cref="DataBaseContext"/> instances for efficient database operations.
/// </summary>
public sealed class DataBaseContextPool : IAsyncDisposable
{
    //private readonly DbContextOptions<DataBaseContext> _options;
    private readonly ILogger<DataBaseContextPool> _logger;
    //private bool _disposed;

    // public DataBaseContextPool(DataBaseConfig config, ILoggerFactory loggerFactory)
    // {
    //     ArgumentNullException.ThrowIfNull(config);
    //     ArgumentNullException.ThrowIfNull(loggerFactory);

    //     _logger = loggerFactory.CreateLogger<DataBaseContextPool>();

    //     var connectionString = config.BuildConnectionString();

    //     _logger.LogInformation(
    //         "Creating DataBaseContextPool with pool size {MaxPoolSize} for database {DatabaseName}",
    //         config.MaxConnectionsPoolSize,
    //         config.DatabaseName);

    //     var optionsBuilder = new DbContextOptionsBuilder<DataBaseContext>()
    //         .UseNpgsql(connectionString);

    //     // Only enable EF Core logging if LogAllQueries is true
    //     if (config.LogAllQueries)
    //     {
    //         optionsBuilder.UseLoggerFactory(loggerFactory);
    //         _logger.LogInformation("Database query logging enabled");
    //     }

    //     // Enable sensitive data logging if configured
    //     if (config.SensitiveDataLogging)
    //     {
    //         optionsBuilder.EnableSensitiveDataLogging();
    //         _logger.LogInformation("Sensitive data logging enabled (parameters and error details will be logged)");
    //     }

    //     _options = optionsBuilder.Options;

    //     _logger.LogInformation("DataBaseContextPool created successfully");
    // }

    // /// <summary>
    // /// Gets a <see cref="DataBaseContext"/> instance from the pool.
    // /// </summary>
    // /// <param name="cancellationToken">Cancellation token.</param>
    // /// <returns>A pooled context instance.</returns>
    // public Task<DataBaseContext> GetContextAsync(CancellationToken cancellationToken = default)
    // {
    //     ObjectDisposedException.ThrowIf(_disposed, this);

    //     _logger.LogDebug("Creating context from pool");

    //     var context = new DataBaseContext(_options);

    //     _logger.LogDebug("Context created from pool");

    //     return Task.FromResult(context);
    // }

    // public DataBaseContext GetContext()
    // {
    //     ObjectDisposedException.ThrowIf(_disposed, this);

    //     _logger.LogDebug("Creating context from pool");

    //     var context = new DataBaseContext(_options);

    //     _logger.LogDebug("Context created from pool");

    //     return context;
    // }

    // public void Dispose()
    // {
    //     if (_disposed)
    //     {
    //         return;
    //     }

    //     _logger.LogInformation("Disposing DataBaseContextPool");

    //     _disposed = true;

    //     _logger.LogInformation("DataBaseContextPool disposed");
    // }

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

        //var options = optionsBuilder.Options;
        //var activator = DbContextActivator<DataBaseContext>.Create();

        // _factory = new PooledDbContextFactory<DataBaseContext>(
        //     options,
        //     dbConfig.DbContextPoolSize,
        //      activator);

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
                options.CacheAllQueries( (CacheExpirationMode) secondLevelCacheConfig.CacheAllQueriesExpirationMode, secondLevelCacheConfig.CacheAllQueriesTimeout);
        });

        return services.BuildServiceProvider(validateScopes: false);
    }
}
