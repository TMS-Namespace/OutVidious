using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Configuration;

namespace TMS.Apps.FrontTube.Backend.Repository.DataBase;

/// <summary>
/// Provides a pool of <see cref="DataBaseContext"/> instances for efficient database operations.
/// </summary>
public sealed class DataBaseContextPool : IDisposable
{
    private readonly DbContextOptions<DataBaseContext> _options;
    private readonly ILogger<DataBaseContextPool> _logger;
    private bool _disposed;

    public DataBaseContextPool(DataBaseConfig config, ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _logger = loggerFactory.CreateLogger<DataBaseContextPool>();

        var connectionString = config.BuildConnectionString();

        _logger.LogInformation(
            "Creating DataBaseContextPool with pool size {MaxPoolSize} for database {DatabaseName}",
            config.MaxPoolSize,
            config.DatabaseName);

        var optionsBuilder = new DbContextOptionsBuilder<DataBaseContext>()
            .UseNpgsql(connectionString);

        // Only enable EF Core logging if LogAllQueries is true
        if (config.LogAllQueries)
        {
            optionsBuilder.UseLoggerFactory(loggerFactory);
            _logger.LogInformation("Database query logging enabled");
        }

        // Enable sensitive data logging if configured
        if (config.SensitiveDataLogging)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            _logger.LogInformation("Sensitive data logging enabled (parameters and error details will be logged)");
        }

        _options = optionsBuilder.Options;

        _logger.LogInformation("DataBaseContextPool created successfully");
    }

    /// <summary>
    /// Gets a <see cref="DataBaseContext"/> instance from the pool.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A pooled context instance.</returns>
    public Task<DataBaseContext> GetContextAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _logger.LogDebug("Creating context from pool");

        var context = new DataBaseContext(_options);

        _logger.LogDebug("Context created from pool");

        return Task.FromResult(context);
    }

    public DataBaseContext GetContext()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _logger.LogDebug("Creating context from pool");

        var context = new DataBaseContext(_options);

        _logger.LogDebug("Context created from pool");

        return context;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _logger.LogInformation("Disposing DataBaseContextPool");

        _disposed = true;

        _logger.LogInformation("DataBaseContextPool disposed");
    }
}
