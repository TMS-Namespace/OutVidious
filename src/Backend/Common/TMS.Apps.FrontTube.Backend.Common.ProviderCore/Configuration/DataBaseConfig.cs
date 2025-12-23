namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Configuration;

/// <summary>
/// Configuration for PostgreSQL database connection.
/// </summary>
public sealed record DataBaseConfig
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
    /// Default: ftube.
    /// </summary>
    public string Database { get; init; } = "ftube";

    /// <summary>
    /// Database username.
    /// Default: root.
    /// </summary>
    public string Username { get; init; } = "root";

    /// <summary>
    /// Database password.
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Whether to include error details in exceptions.
    /// Default: false (for production safety).
    /// </summary>
    public bool IncludeErrorDetail { get; init; } = false;

    /// <summary>
    /// Connection pool minimum size.
    /// Default: 1.
    /// </summary>
    public int MinPoolSize { get; init; } = 1;

    /// <summary>
    /// Connection pool maximum size.
    /// Default: 20.
    /// </summary>
    public int MaxPoolSize { get; init; } = 20;

    /// <summary>
    /// Connection timeout in seconds.
    /// Default: 30.
    /// </summary>
    public int ConnectionTimeoutSeconds { get; init; } = 30;

    /// <summary>
    /// Whether development mode is enabled.
    /// When true, a development user will be seeded.
    /// Default: true.
    /// </summary>
    public bool IsDevMode { get; init; } = true;

    /// <summary>
    /// Builds a PostgreSQL connection string from the configured properties.
    /// </summary>
    public string BuildConnectionString()
    {
        return $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};" +
               $"Minimum Pool Size={MinPoolSize};Maximum Pool Size={MaxPoolSize};Timeout={ConnectionTimeoutSeconds};" +
               $"Include Error Detail={IncludeErrorDetail}";
    }
}
