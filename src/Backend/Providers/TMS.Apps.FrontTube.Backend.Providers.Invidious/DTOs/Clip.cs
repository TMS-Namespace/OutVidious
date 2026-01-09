namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Clip video from the Invidious API.
/// </summary>
internal sealed record Clip
{
    public string ClipId { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string VideoId { get; init; } = string.Empty;

    public int StartTimeSeconds { get; init; }

    public int EndTimeSeconds { get; init; }

    public IReadOnlyList<VideoThumbnail> VideoThumbnails { get; init; } = [];

    public string Author { get; init; } = string.Empty;

    public string AuthorId { get; init; } = string.Empty;

    public string AuthorUrl { get; init; } = string.Empty;
}
