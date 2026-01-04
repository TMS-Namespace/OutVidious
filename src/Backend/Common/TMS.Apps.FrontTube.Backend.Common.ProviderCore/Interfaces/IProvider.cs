using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;

/// <summary>
/// Interface for video providers that can fetch video information and generate playback URLs.
/// All providers must implement this interface to be used by the application.
/// </summary>
public interface IProvider : IProviderMetadata, IDisposable
{
    /// <summary>
    /// Gets detailed information about a video.
    /// </summary>
    /// <param name="videoId">The video identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Video information or null if not found.</returns>
    Task<VideoCommon?> GetVideoAsync(string videoId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the embed URL for a video.
    /// </summary>
    /// <param name="videoId">The video identifier.</param>
    /// <returns>The embed URL.</returns>
    Uri GetEmbedUrl(string videoId);

    /// <summary>
    /// Gets detailed information about a channel.
    /// </summary>
    /// <param name="channelId">The channel identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Channel details or null if not found.</returns>
    Task<ChannelCommon?> GetChannelAsync(string channelId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a page of videos from a channel.
    /// </summary>
    /// <param name="channelId">The channel identifier.</param>
    /// <param name="tab">The channel tab to fetch from (e.g., "videos", "shorts", "live"). Defaults to "videos".</param>
    /// <param name="continuationToken">Token for pagination. Null for the first page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A page of videos with continuation token for next page.</returns>
    Task<VideosPageCommon?> GetChannelVideosTabAsync(
        string channelId,
        string tab = "videos",
        string? continuationToken = null,
        CancellationToken cancellationToken = default);
}
