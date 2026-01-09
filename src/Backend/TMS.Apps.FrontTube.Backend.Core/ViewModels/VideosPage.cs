using TMS.Apps.FrontTube.Backend.Core.Enums;
using TMS.Apps.FrontTube.Backend.Core.Mappers;
using TMS.Apps.FrontTube.Backend.Repository.Data.Contracts;

namespace TMS.Apps.FrontTube.Backend.Core.ViewModels;

/// <summary>
/// Represents a paginated list of videos from a channel.
/// </summary>
public sealed record VideosPage
{
    internal VideosPage(Super super, VideosPageDomain domain, List<Video> videos, Channel channel)
    {
        Channel = channel;
        Tab = DomainViewModelMapper.ToViewModelChannelTab(domain.Tab);
        Videos = videos;
        ContinuationToken = domain.ContinuationToken;
        TotalVideoCount = domain.TotalVideoCount;
        PageNumber = domain.PageNumber;
    }

    /// <summary>
    /// The channel this page belongs to.
    /// </summary>
    public Channel Channel { get; init; }

    /// <summary>
    /// The tab this page was retrieved from.
    /// </summary>
    public ChannelTab Tab { get; init; } = ChannelTab.Videos;

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
