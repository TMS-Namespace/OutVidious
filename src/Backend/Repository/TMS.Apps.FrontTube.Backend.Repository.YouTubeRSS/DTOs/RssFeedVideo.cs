namespace TMS.Apps.FrontTube.Backend.Repository.YouTubeRSS.DTOs;

/// <summary>
/// Represents a video entry from a YouTube channel RSS feed.
/// Maps the Atom feed entry structure from YouTube's RSS.
/// </summary>
internal sealed record RssFeedVideo
{
    /// <summary>
    /// The YouTube video ID.
    /// </summary>
    public required string VideoId { get; init; }

    /// <summary>
    /// The title of the video.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// The absolute URL to the video on YouTube.
    /// </summary>
    public required string VideoUrl { get; init; }

    /// <summary>
    /// When the video was published (UTC).
    /// </summary>
    public required DateTimeOffset PublishedAtUtc { get; init; }

    /// <summary>
    /// When the video was last updated (UTC).
    /// </summary>
    public DateTimeOffset? UpdatedAtUtc { get; init; }

    /// <summary>
    /// The video description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The thumbnail information.
    /// </summary>
    public RssFeedThumbnail? Thumbnail { get; init; }

    /// <summary>
    /// The view count of the video (from media:statistics).
    /// </summary>
    public long? ViewCount { get; init; }

    /// <summary>
    /// Star rating of the video (from media:starRating).
    /// </summary>
    public double? StarRating { get; init; }
}
