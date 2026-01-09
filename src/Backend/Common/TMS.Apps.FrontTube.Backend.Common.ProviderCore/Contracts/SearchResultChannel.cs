namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Channel search result item.
/// </summary>
public sealed record SearchResultChannelCommon : SearchResultItemCommon
{
    /// <summary>
    /// The channel metadata.
    /// </summary>
    public required ChannelMetadataCommon Channel { get; init; }

    /// <summary>
    /// Channel description snippet.
    /// </summary>
    public string? DescriptionSnippet { get; init; }

    /// <summary>
    /// HTML formatted description snippet.
    /// </summary>
    public string? DescriptionHtml { get; init; }
}
