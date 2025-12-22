using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Contracts;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Interfaces;

namespace TMS.Apps.FTube.Backend.DataRepository.Interfaces;

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
    /// Clears all memory caches.
    /// </summary>
    void ClearMemoryCache();
}
