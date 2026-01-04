using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore;

/// <summary>
/// Base class for video providers with common functionality.
/// </summary>
public abstract class ProviderBase : IProvider
{
    private bool _disposed;

    protected readonly HttpClient HttpClient;
    protected readonly ILogger Logger;

    protected ProviderBase(HttpClient httpClient, ILogger logger, Uri baseUrl)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
    }

    /// <inheritdoc />
    public abstract string ProviderId { get; }

    /// <inheritdoc />
    public abstract string DisplayName { get; }

    /// <inheritdoc />
    public abstract string Description { get; }

    /// <inheritdoc />
    public Uri BaseUrl { get; }

    /// <inheritdoc />
    public virtual bool IsConfigured => BaseUrl.IsAbsoluteUri;

    /// <inheritdoc />
    public abstract Task<VideoCommon?> GetVideoAsync(RemoteIdentityCommon videoIdentity, CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Uri GetEmbedUrl(RemoteIdentityCommon videoIdentity);

    /// <inheritdoc />
    public abstract Task<ChannelCommon?> GetChannelAsync(RemoteIdentityCommon channelIdentity, CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Task<VideosPageCommon?> GetChannelVideosTabAsync(
        RemoteIdentityCommon channelIdentity,
        string tab,
        string? continuationToken,
        CancellationToken cancellationToken);

    /// <summary>
    /// Creates a URI by combining the base URL with a relative path.
    /// </summary>
    /// <param name="relativePath">The relative path to append.</param>
    /// <returns>The combined URI.</returns>
    protected Uri CreateUri(string relativePath)
    {
        var baseUrlString = BaseUrl.ToString().TrimEnd('/');
        var path = relativePath.TrimStart('/');
        return new Uri($"{baseUrlString}/{path}");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Note: We don't dispose HttpClient here as it's typically injected
            // and managed by the DI container
        }

        _disposed = true;
    }
}
