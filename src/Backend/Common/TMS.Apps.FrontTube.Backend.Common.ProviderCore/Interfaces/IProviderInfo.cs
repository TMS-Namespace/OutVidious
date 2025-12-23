namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;

/// <summary>
/// Metadata about a video provider.
/// </summary>
public interface IProviderInfo
{
    /// <summary>
    /// Unique identifier for the provider.
    /// </summary>
    string ProviderId { get; }

    /// <summary>
    /// Display name of the provider.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Description of the provider.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Base URL of the provider instance.
    /// </summary>
    Uri BaseUrl { get; }

    /// <summary>
    /// Whether the provider is currently configured and ready.
    /// </summary>
    bool IsConfigured { get; }
}
