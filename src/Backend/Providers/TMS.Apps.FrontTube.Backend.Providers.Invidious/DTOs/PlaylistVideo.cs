namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Video in a playlist from the Invidious API.
/// </summary>
internal sealed record PlaylistVideo
{
    public string Title { get; init; } = string.Empty;

    public string VideoId { get; init; } = string.Empty;

    public string Author { get; init; } = string.Empty;

    public string AuthorId { get; init; } = string.Empty;

    public string AuthorUrl { get; init; } = string.Empty;

    public IReadOnlyList<VideoThumbnail> VideoThumbnails { get; init; } = [];

    public int Index { get; init; }

    public int LengthSeconds { get; init; }
}
