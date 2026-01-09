namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Video search result from the Invidious API.
/// </summary>
internal sealed record SearchVideo
{
    public string Type { get; init; } = "video";

    public string Title { get; init; } = string.Empty;

    public string VideoId { get; init; } = string.Empty;

    public string Author { get; init; } = string.Empty;

    public string AuthorId { get; init; } = string.Empty;

    public string AuthorUrl { get; init; } = string.Empty;

    public bool AuthorVerified { get; init; }

    public IReadOnlyList<VideoThumbnail> VideoThumbnails { get; init; } = [];

    public string Description { get; init; } = string.Empty;

    public string DescriptionHtml { get; init; } = string.Empty;

    public long ViewCount { get; init; }

    public string? ViewCountText { get; init; }

    public long Published { get; init; }

    public string PublishedText { get; init; } = string.Empty;

    public int LengthSeconds { get; init; }

    public bool LiveNow { get; init; }

    public bool Paid { get; init; }

    public bool Premium { get; init; }

    public bool IsUpcoming { get; init; }

    public long? PremiereTimestamp { get; init; }

    public bool? IsNew { get; init; }

    public bool? Is4k { get; init; }

    public bool? Is8k { get; init; }

    public bool? IsVr180 { get; init; }

    public bool? IsVr360 { get; init; }

    public bool? Is3d { get; init; }

    public bool? HasCaptions { get; init; }
}
