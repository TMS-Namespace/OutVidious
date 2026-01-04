namespace TMS.Apps.FrontTube.Backend.Repository.Data.Contracts.Configuration;

/// <summary>
/// Configuration for PostgreSQL database connection.
/// </summary>
public sealed record DatabaseConfig
{
    /// <summary>
    /// Database server host address.
    /// Default: localhost.
    /// </summary>
    public string Host { get; init; } = "localhost";

    /// <summary>
    /// Database server port.
    /// Default: 5432 (PostgreSQL default).
    /// </summary>
    public int Port { get; init; } = 5432;

    /// <summary>
    /// Database name.
    /// Default: front_tube.
    /// </summary>
    public string DatabaseName { get; init; } = "front_tube";

    /// <summary>
    /// Database username.
    /// Default: root.
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Database password.
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Whether development mode is enabled.
    /// When true, a development user will be seeded.
    /// Default: true.
    /// </summary>
    public bool IsDevMode { get; init; } = true;

    /// <summary>
    /// Whether to log all database queries to the logger.
    /// Default: false (for production performance).
    /// </summary>
    public bool LogAllQueries { get; init; } = false;

    public DatabaseConnectionPoolConfig ConnectionPoolConfig { get; init; } = new();

    /// <summary>
    /// Whether to enable sensitive data logging in EF Core and PostgreSQL client.
    /// When true, EF Core will log parameter values and PostgreSQL will include error details.
    /// This may log sensitive information and should be used with caution.
    /// Default: false.
    /// </summary>
    public bool SensitiveDataLogging { get; init; } = true;

    public int DbContextPoolSize { get; init; } = 256;

    public int? CommandTimeoutSeconds { get; init; } = 5;

    public bool EnableRetryOnFailure { get; init; } = false;

    public int MaxRetryCount { get; init; } = 3;

    public TimeSpan MaxRetryDelay { get; init; } = TimeSpan.FromSeconds(3);
}
