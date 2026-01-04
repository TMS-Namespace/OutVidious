namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Represents detailed channel information.
/// </summary>
public sealed record ChannelCommon : ChannelMetadataCommon
{
    /// <summary>
    /// Channel description/about text.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// HTML-formatted description.
    /// </summary>
    public string? DescriptionHtml { get; init; }

    /// <summary>
    /// Total video count on the channel.
    /// </summary>
    public int? VideoCount { get; init; }

    /// <summary>
    /// Total view count across all videos.
    /// </summary>
    public long? TotalViewCount { get; init; }

    /// <summary>
    /// When the channel was created.
    /// </summary>
    public DateTimeOffset? JoinedAt { get; init; }
    /// <summary>
    /// Channel banner images.
    /// </summary>
    public IReadOnlyList<ImageMetadataCommon> Banners { get; init; } = [];

    /// <summary>
    /// Available tabs for this channel.
    /// </summary>
    public IReadOnlyList<ChannelTab> AvailableTabs { get; init; } = [];

    /// <summary>
    /// Whether the channel is verified.
    /// </summary>
    public bool IsVerified { get; init; }

    /// <summary>
    /// Keywords/tags associated with the channel.
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = [];

    public new bool IsMetaData => false;
}
