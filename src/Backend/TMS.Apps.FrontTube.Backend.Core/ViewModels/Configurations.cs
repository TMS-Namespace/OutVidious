using TMS.Apps.FrontTube.Backend.Repository.Data.Contracts.Configuration;

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
    public DatabaseConfig DataBase { get; set; } = new DatabaseConfig() with 
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
        //BaseUri = new Uri("https://inv.perditum.com/"),
        BypassSslValidation = true,
    };
}
