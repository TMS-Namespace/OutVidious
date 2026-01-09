namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Hashtag search result item.
/// </summary>
public sealed record SearchResultHashtagCommon : SearchResultItemCommon
{
    /// <summary>
    /// The hashtag text.
    /// </summary>
    public required string Hashtag { get; init; }

    /// <summary>
    /// URL to the hashtag page.
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// Number of videos with this hashtag.
    /// </summary>
    public long VideoCount { get; init; }

    /// <summary>
    /// Number of channels using this hashtag.
    /// </summary>
    public long ChannelCount { get; init; }
}
