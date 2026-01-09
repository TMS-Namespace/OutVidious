namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Trending video from the Invidious API (same structure as search video but with fewer fields).
/// </summary>
internal sealed record TrendingVideo
{
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

    public string Description { get; init; } = string.Empty;

    public string DescriptionHtml { get; init; } = string.Empty;

    public bool LiveNow { get; init; }

    public bool Paid { get; init; }

    public bool Premium { get; init; }
}
