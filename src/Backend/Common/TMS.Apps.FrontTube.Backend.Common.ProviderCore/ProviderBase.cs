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
    public abstract Task<Video?> GetVideoInfoAsync(string videoId, CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Uri GetEmbedUrl(string videoId);

    /// <inheritdoc />
    public abstract Uri GetWatchUrl(string videoId);

    /// <inheritdoc />
    public abstract Uri? GetDashManifestUrl(string videoId);

    /// <inheritdoc />
    public abstract Uri? GetHlsManifestUrl(string videoId);

    /// <inheritdoc />
    public abstract Uri? GetProxiedDashManifestUrl(string videoId);

    /// <inheritdoc />
    public abstract bool IsValidVideoId(string videoId);

    /// <inheritdoc />
    public abstract Task<Channel?> GetChannelDetailsAsync(string channelId, CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Task<VideosPage?> GetChannelVideosAsync(
        string channelId,
        string tab = "videos",
        string? continuationToken = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract Uri GetChannelUrl(string channelId);

    /// <inheritdoc />
    public abstract bool IsValidChannelId(string channelId);

    /// <inheritdoc />
    public abstract Uri GetImageFetchUrl(Uri originalUrl);

    /// <summary>
    /// Validates that a video ID is not null or empty.
    /// </summary>
    /// <param name="videoId">The video ID to validate.</param>
    /// <exception cref="ArgumentException">Thrown when video ID is null or empty.</exception>
    protected static void ValidateVideoIdNotEmpty(string videoId)
    {
        if (string.IsNullOrWhiteSpace(videoId))
        {
            throw new ArgumentException("Video ID cannot be empty.", nameof(videoId));
        }
    }

    /// <summary>
    /// Validates that a channel ID is not null or empty.
    /// </summary>
    /// <param name="channelId">The channel ID to validate.</param>
    /// <exception cref="ArgumentException">Thrown when channel ID is null or empty.</exception>
    protected static void ValidateChannelIdNotEmpty(string channelId)
    {
        if (string.IsNullOrWhiteSpace(channelId))
        {
            throw new ArgumentException("Channel ID cannot be empty.", nameof(channelId));
        }
    }

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
