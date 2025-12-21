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
}
