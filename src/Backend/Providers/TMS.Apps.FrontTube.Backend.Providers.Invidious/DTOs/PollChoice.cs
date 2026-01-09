namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Poll choice for community post polls.
/// </summary>
internal sealed record PollChoice
{
    public string Text { get; init; } = string.Empty;

    public IReadOnlyList<VideoThumbnail> Image { get; init; } = [];
}
