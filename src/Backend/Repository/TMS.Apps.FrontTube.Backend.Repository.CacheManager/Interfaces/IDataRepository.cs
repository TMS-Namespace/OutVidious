using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;

namespace TMS.Apps.FTube.Backend.DataRepository.Interfaces;

/// <summary>
/// Represents cached image data including binary content and metadata.
/// </summary>
public sealed record CachedImage
{
    /// <summary>
    /// The binary image data.
    /// </summary>
    public required byte[] Data { get; init; }

    /// <summary>
    /// The MIME type of the image (e.g., "image/jpeg", "image/webp").
    /// </summary>
    public required string MimeType { get; init; }

    /// <summary>
    /// Image width in pixels.
    /// </summary>
    public int? Width { get; init; }

    /// <summary>
    /// Image height in pixels.
    /// </summary>
    public int? Height { get; init; }

    /// <summary>
    /// When the image was last synced from the remote source.
    /// </summary>
    public DateTime LastSyncedAt { get; init; }
}

/// <summary>
/// Repository interface for managing cached video/channel data.
/// Implements a multi-tier caching strategy: Memory → Database → Provider.
/// </summary>
public interface IDataRepository : IDisposable
{
    /// <summary>
    /// Gets video information by remote ID.
    /// Checks memory cache → database → provider in that order.
    /// Stale data triggers a background refresh from provider.
    /// </summary>
    /// <param name="remoteId">The video's remote identifier (e.g., YouTube video ID).</param>
    /// <param name="provider">The provider to fetch from if not cached or stale.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Video information or null if not found.</returns>
    Task<VideoInfo?> GetVideoAsync(string remoteId, IVideoProvider provider, CancellationToken cancellationToken);

    /// <summary>
    /// Gets channel details by remote ID.
    /// Checks memory cache → database → provider in that order.
    /// </summary>
    /// <param name="remoteId">The channel's remote identifier (e.g., YouTube channel ID).</param>
    /// <param name="provider">The provider to fetch from if not cached or stale.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Channel details or null if not found.</returns>
    Task<ChannelDetails?> GetChannelAsync(string remoteId, IVideoProvider provider, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a page of videos from a channel.
    /// Note: Pagination data is typically not cached as it changes frequently.
    /// </summary>
    /// <param name="channelId">The channel's remote identifier.</param>
    /// <param name="tab">The tab to fetch from (videos, shorts, live).</param>
    /// <param name="continuationToken">Pagination token.</param>
    /// <param name="provider">The provider to fetch from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A page of video summaries.</returns>
    Task<ChannelVideoPage?> GetChannelVideosAsync(
        string channelId,
        string tab,
        string? continuationToken,
        IVideoProvider provider,
        CancellationToken cancellationToken);

    /// <summary>
    /// Invalidates cached video data, forcing a refresh on next access.
    /// </summary>
    /// <param name="remoteId">The video's remote identifier.</param>
    void InvalidateVideo(string remoteId);

    /// <summary>
    /// Invalidates cached channel data, forcing a refresh on next access.
    /// </summary>
    /// <param name="remoteId">The channel's remote identifier.</param>
    void InvalidateChannel(string remoteId);

    /// <summary>
    /// Gets an image by its original URL with caching.
    /// Checks memory cache → database → web in that order.
    /// Images are downloaded and stored as binary data for offline access.
    /// </summary>
    /// <param name="originalUrl">The original URL of the image (e.g., YouTube CDN URL).</param>
    /// <param name="fetchUrl">The URL to fetch the image from (may be different from original, e.g., provider proxy).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Cached image data or null if not found/failed.</returns>
    Task<CachedImage?> GetImageAsync(Uri originalUrl, Uri fetchUrl, CancellationToken cancellationToken);

    /// <summary>
    /// Invalidates cached image data, forcing a refresh on next access.
    /// </summary>
    /// <param name="originalUrl">The original URL of the image.</param>
    void InvalidateImage(Uri originalUrl);

    /// <summary>
    /// Clears all memory caches.
    /// </summary>
    void ClearMemoryCache();
}
