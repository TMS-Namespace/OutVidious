namespace TMS.Apps.FrontTube.Backend.Core.ViewModels;

/// <summary>
/// Represents a paginated list of videos from a channel.
/// </summary>
public sealed record VideosPage
{
    private readonly Super _super;
    internal VideosPage(Super super, Common.ProviderCore.Contracts.VideosPage commonModel, List<Video> videos, Channel channel)
    {
        _super = super;

        Channel = channel;
        Tab = commonModel.Tab;
        Videos = videos;
        ContinuationToken = commonModel.ContinuationToken;
        TotalVideoCount = commonModel.TotalVideoCount;
        PageNumber = commonModel.PageNumber;
    }

    /// <summary>
    /// The channel this page belongs to.
    /// </summary>
    public Channel Channel { get; init; }

    /// <summary>
    /// The tab this page was retrieved from (e.g., "videos", "shorts", "live").
    /// </summary>
    public string Tab { get; init; } = "videos";

    /// <summary>
    /// List of videos in this page.
    /// </summary>
    public IReadOnlyList<Video> Videos { get; init; } = [];

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
