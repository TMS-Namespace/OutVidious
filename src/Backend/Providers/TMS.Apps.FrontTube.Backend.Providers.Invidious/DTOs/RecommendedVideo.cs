namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Recommended video from the Invidious API.
/// </summary>
internal sealed record RecommendedVideo
{
    public string VideoId { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public IReadOnlyList<VideoThumbnail> VideoThumbnails { get; init; } = [];

    public string Author { get; init; } = string.Empty;

    public string AuthorUrl { get; init; } = string.Empty;

    public string? AuthorId { get; init; }

    public bool AuthorVerified { get; init; }

    public IReadOnlyList<AuthorThumbnail> AuthorThumbnails { get; init; } = [];

    public int LengthSeconds { get; init; }

    public long ViewCount { get; init; }

    public string? ViewCountText { get; init; }
}
