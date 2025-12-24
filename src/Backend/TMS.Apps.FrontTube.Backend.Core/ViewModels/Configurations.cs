using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Configuration;

namespace TMS.Apps.FrontTube.Backend.Core.ViewModels;

/// <summary>
/// View model for managing application configurations.
/// Provides centralized access to all configuration objects with default values.
/// </summary>
public sealed class Configurations
{
    /// <summary>
    /// Configuration for cache behavior and staleness thresholds.
    /// </summary>
    public CacheConfig Cache { get; set; } = new();

    /// <summary>
    /// Configuration for database connection.
    /// </summary>
    public DataBaseConfig DataBase { get; set; } = new DataBaseConfig() with 
    {
        Host= "localhost",
        Port = 5656,
        DatabaseName = "front_tube",
        Username = "root",
        Password = "password",
    };

    /// <summary>
    /// Configuration for external video providers.
    /// </summary>
    public ProviderConfig Provider { get; set; } = new ProviderConfig() with 
    { 
        BaseUri = new Uri("https://youtube.srv1.tms.com"),
        BypassSslValidation = true,
    };
}
