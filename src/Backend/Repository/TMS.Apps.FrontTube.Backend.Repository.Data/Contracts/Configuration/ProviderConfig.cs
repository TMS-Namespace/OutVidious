namespace TMS.Apps.FrontTube.Backend.Repository.Data.Contracts.Configuration;

/// <summary>
/// Configuration for external video providers (e.g., Invidious instances).
/// </summary>
public sealed record ProviderConfig
{
    /// <summary>
    /// The base URI of the provider instance.
    /// </summary>
    public Uri? BaseUri { get; init; }

    /// <summary>
    /// Whether to bypass SSL certificate validation.
    /// Useful for development with self-signed certificates.
    /// Default: false (for production safety).
    /// </summary>
    public bool BypassSslValidation { get; init; } = false;

    /// <summary>
    /// Request timeout in seconds.
    /// Default: 30.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 30;
}
