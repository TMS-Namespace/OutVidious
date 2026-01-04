namespace TMS.Apps.FrontTube.Backend.Repository.Data.Contracts;

/// <summary>
/// Represents a paginated list of videos from a channel.
/// </summary>
public sealed class VideosPageDomain
{
    /// <summary>
    /// Empty page for error cases or when no results are found.
    /// </summary>
    public static VideosPageDomain Empty(string channelId, string tab = "videos") => new()
    {
        ChannelAbsoluteRemoteUrl = channelId,
        Tab = tab,
        Videos = [],
        ContinuationToken = null
    };

    /// <summary>
    /// The channel this page belongs to.
    /// </summary>
    public required string ChannelAbsoluteRemoteUrl { get; set; }

    /// <summary>
    /// The tab this page was retrieved from (e.g., "videos", "shorts", "live").
    /// </summary>
    public string Tab { get; set; } = "videos";

    /// <summary>
    /// List of videos in this page.
    /// </summary>
    public IReadOnlyList<VideoDomain> Videos { get; set; } = [];

    /// <summary>
    /// Continuation token for fetching the next page.
    /// Null if this is the last page.
    /// </summary>
    public string? ContinuationToken { get; set; }

    /// <summary>
    /// Whether there are more videos to load.
    /// </summary>
    public bool HasMore => !string.IsNullOrEmpty(ContinuationToken);

    /// <summary>
    /// Total number of videos in the channel (if known).
    /// </summary>
    public int? TotalVideoCount { get; set; }

    /// <summary>
    /// The page number (if using page-based pagination).
    /// </summary>
    public int? PageNumber { get; set; }

    public string? FetchingError { get; set; }
}
