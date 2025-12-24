namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Represents a paginated list of videos from a channel.
/// </summary>
public sealed record VideosPage
{
    /// <summary>
    /// Empty page for error cases or when no results are found.
    /// </summary>
    public static VideosPage Empty(string channelId, string tab = "videos") => new()
    {
        ChannelId = channelId,
        Tab = tab,
        Videos = [],
        ContinuationToken = null
    };

    /// <summary>
    /// The channel this page belongs to.
    /// </summary>
    public required string ChannelId { get; init; }

    /// <summary>
    /// The tab this page was retrieved from (e.g., "videos", "shorts", "live").
    /// </summary>
    public string Tab { get; init; } = "videos";

    /// <summary>
    /// List of videos in this page.
    /// </summary>
    public IReadOnlyList<VideoMetadata> Videos { get; init; } = [];

    /// <summary>
    /// Continuation token for fetching the next page.
    /// Null if this is the last page.
    /// </summary>
    public string? ContinuationToken { get; init; }

    /// <summary>
    /// Whether there are more videos to load.
    /// </summary>
    public bool HasMore => !string.IsNullOrEmpty(ContinuationToken);

    /// <summary>
    /// Total number of videos in the channel (if known).
    /// </summary>
    public int? TotalVideoCount { get; init; }

    /// <summary>
    /// The page number (if using page-based pagination).
    /// </summary>
    public int? PageNumber { get; init; }
}
