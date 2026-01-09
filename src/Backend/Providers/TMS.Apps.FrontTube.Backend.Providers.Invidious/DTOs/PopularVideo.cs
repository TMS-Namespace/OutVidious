namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Popular video from the Invidious API.
/// </summary>
internal sealed record PopularVideo
{
    public string Type { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string VideoId { get; init; } = string.Empty;

    public IReadOnlyList<VideoThumbnail> VideoThumbnails { get; init; } = [];

    public int LengthSeconds { get; init; }

    public long ViewCount { get; init; }

    public string Author { get; init; } = string.Empty;

    public string AuthorId { get; init; } = string.Empty;

    public string AuthorUrl { get; init; } = string.Empty;

    public long Published { get; init; }

    public string PublishedText { get; init; } = string.Empty;
}
