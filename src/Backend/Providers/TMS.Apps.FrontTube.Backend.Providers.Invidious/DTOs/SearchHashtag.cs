namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Hashtag search result from the Invidious API.
/// </summary>
internal sealed record SearchHashtag
{
    public string Type { get; init; } = "hashtag";

    public string Title { get; init; } = string.Empty;

    public string Url { get; init; } = string.Empty;

    public int ChannelCount { get; init; }

    public int VideoCount { get; init; }
}
