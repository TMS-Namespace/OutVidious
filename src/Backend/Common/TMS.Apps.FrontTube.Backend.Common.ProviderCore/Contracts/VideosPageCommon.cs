using TMS.Apps.FrontTube.Backend.Common.DataEnums.Enums;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Represents a paginated list of videos from a channel.
/// </summary>
public sealed record VideosPageCommon : ICommonContract
{
    /// <summary>
    /// The channel this page belongs to.
    /// </summary>
    public required RemoteIdentityCommon ChannelRemoteIdentity { get; init; }

    /// <summary>
    /// The tab this page was retrieved from.
    /// </summary>
    public ChannelTabType Tab { get; init; } = ChannelTabType.Videos;

    /// <summary>
    /// List of videos in this page.
    /// </summary>
    public IReadOnlyList<VideoMetadataCommon> Videos { get; init; } = [];

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
