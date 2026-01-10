namespace TMS.Apps.FrontTube.Backend.Repository.YouTubeRSS.DTOs;

/// <summary>
/// Represents thumbnail information from the YouTube RSS media:thumbnail element.
/// </summary>
internal sealed record RssFeedThumbnail
{
    /// <summary>
    /// The URL of the thumbnail image.
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// The width of the thumbnail in pixels.
    /// </summary>
    public required int Width { get; init; }

    /// <summary>
    /// The height of the thumbnail in pixels.
    /// </summary>
    public required int Height { get; init; }
}
