using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Contracts;

namespace TMS.Apps.Web.OutVidious.Common.ProvidersCore.Interfaces;

/// <summary>
/// Interface for video providers that can fetch video information and generate playback URLs.
/// All providers must implement this interface to be used by the application.
/// </summary>
public interface IVideoProvider : IProviderInfo, IDisposable
{
    /// <summary>
    /// Gets detailed information about a video.
    /// </summary>
    /// <param name="videoId">The video identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Video information or null if not found.</returns>
    Task<VideoInfo?> GetVideoInfoAsync(string videoId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the embed URL for a video.
    /// </summary>
    /// <param name="videoId">The video identifier.</param>
    /// <returns>The embed URL.</returns>
    Uri GetEmbedUrl(string videoId);

    /// <summary>
    /// Gets the watch/player URL for a video.
    /// </summary>
    /// <param name="videoId">The video identifier.</param>
    /// <returns>The watch URL.</returns>
    Uri GetWatchUrl(string videoId);

    /// <summary>
    /// Gets the DASH manifest URL for adaptive streaming.
    /// </summary>
    /// <param name="videoId">The video identifier.</param>
    /// <returns>The DASH manifest URL or null if not supported.</returns>
    Uri? GetDashManifestUrl(string videoId);

    /// <summary>
    /// Gets the HLS manifest URL for adaptive streaming.
    /// </summary>
    /// <param name="videoId">The video identifier.</param>
    /// <returns>The HLS manifest URL or null if not supported.</returns>
    Uri? GetHlsManifestUrl(string videoId);

    /// <summary>
    /// Gets a proxied DASH manifest URL that bypasses CORS restrictions.
    /// </summary>
    /// <param name="videoId">The video identifier.</param>
    /// <returns>The proxied DASH manifest URL or null if not supported.</returns>
    Uri? GetProxiedDashManifestUrl(string videoId);

    /// <summary>
    /// Validates whether a video ID is in the correct format for this provider.
    /// </summary>
    /// <param name="videoId">The video identifier to validate.</param>
    /// <returns>True if the video ID format is valid.</returns>
    bool IsValidVideoId(string videoId);
}
