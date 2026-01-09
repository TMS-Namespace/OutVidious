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
    /// <param name="videoIdentity">The remote identity for the video.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Video information or null if not found.</returns>
    Task<VideoCommon?> GetVideoAsync(RemoteIdentityCommon videoIdentity, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the embed URL for a video.
    /// </summary>
    /// <param name="videoIdentity">The remote identity for the video.</param>
    /// <returns>The embed URL.</returns>
    Uri GetEmbedVideoPlayerUri(RemoteIdentityCommon videoIdentity);

    /// <summary>
    /// Gets detailed information about a channel.
    /// </summary>
    /// <param name="channelIdentity">The remote identity for the channel.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Channel details or null if not found.</returns>
    Task<ChannelCommon?> GetChannelAsync(RemoteIdentityCommon channelIdentity, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a page of videos from a channel.
    /// </summary>
    /// <param name="channelIdentity">The remote identity for the channel.</param>
    /// <param name="tab">The channel tab to fetch from (e.g., "videos", "shorts", "live").</param>
    /// <param name="continuationToken">Token for pagination. Null for the first page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A page of videos with continuation token for next page.</returns>
    Task<VideosPageCommon?> GetChannelVideosTabAsync(
        RemoteIdentityCommon channelIdentity,
        string tab,
        string? continuationToken,
        CancellationToken cancellationToken);
}
