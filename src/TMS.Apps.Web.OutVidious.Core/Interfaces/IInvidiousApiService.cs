using TMS.Apps.Web.OutVidious.Core.Models;

namespace TMS.Apps.Web.OutVidious.Core.Interfaces;

/// <summary>
/// Interface for interacting with the Invidious API.
/// </summary>
public interface IInvidiousApiService
{
    /// <summary>
    /// Gets the base URL of the Invidious instance.
    /// </summary>
    string BaseUrl { get; }

    /// <summary>
    /// Gets detailed information about a video.
    /// </summary>
    /// <param name="videoId">The YouTube video ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Video details or null if not found.</returns>
    Task<VideoDetails?> GetVideoDetailsAsync(string videoId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the embed URL for a video.
    /// </summary>
    /// <param name="videoId">The YouTube video ID.</param>
    /// <returns>The embed URL.</returns>
    string GetEmbedUrl(string videoId);

    /// <summary>
    /// Gets the watch URL for a video.
    /// </summary>
    /// <param name="videoId">The YouTube video ID.</param>
    /// <returns>The watch URL.</returns>
    string GetWatchUrl(string videoId);

    /// <summary>
    /// Gets the DASH manifest URL for a video.
    /// This manifest allows playback of higher quality streams (1080p+) with separate audio and video.
    /// </summary>
    /// <param name="videoId">The YouTube video ID.</param>
    /// <returns>The DASH manifest URL (direct to Invidious).</returns>
    string GetDashManifestUrl(string videoId);

    /// <summary>
    /// Gets the proxied DASH manifest URL for a video.
    /// Uses a local proxy to avoid CORS issues.
    /// </summary>
    /// <param name="videoId">The YouTube video ID.</param>
    /// <returns>The proxied DASH manifest URL.</returns>
    string GetProxiedDashManifestUrl(string videoId);
}
