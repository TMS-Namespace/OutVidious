namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.ApiModels;

/// <summary>
/// Raw video details DTO from the Invidious API.
/// </summary>
public sealed record InvidiousVideoDetailsDto
{
    public string Type { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string VideoId { get; init; } = string.Empty;

    public IReadOnlyList<InvidiousVideoThumbnailDto> VideoThumbnails { get; init; } = [];

    public string Description { get; init; } = string.Empty;

    public string DescriptionHtml { get; init; } = string.Empty;

    public long Published { get; init; }

    public string PublishedText { get; init; } = string.Empty;

    public IReadOnlyList<string> Keywords { get; init; } = [];

    public long ViewCount { get; init; }

    public int LikeCount { get; init; }

    public int DislikeCount { get; init; }

    public bool Paid { get; init; }

    public bool Premium { get; init; }

    public bool IsFamilyFriendly { get; init; }

    public IReadOnlyList<string> AllowedRegions { get; init; } = [];

    public string Genre { get; init; } = string.Empty;

    public string? GenreUrl { get; init; }

    public string Author { get; init; } = string.Empty;

    public string AuthorId { get; init; } = string.Empty;

    public string AuthorUrl { get; init; } = string.Empty;

    public IReadOnlyList<InvidiousAuthorThumbnailDto> AuthorThumbnails { get; init; } = [];

    public string? SubCountText { get; init; }

    public int LengthSeconds { get; init; }

    public bool AllowRatings { get; init; }

    public float Rating { get; init; }

    public bool IsListed { get; init; }

    public bool LiveNow { get; init; }

    public bool IsPostLiveDvr { get; init; }

    public bool IsUpcoming { get; init; }

    public string? DashUrl { get; init; }

    public long? PremiereTimestamp { get; init; }

    public string? HlsUrl { get; init; }

    public IReadOnlyList<InvidiousAdaptiveFormatDto> AdaptiveFormats { get; init; } = [];

    public IReadOnlyList<InvidiousFormatStreamDto> FormatStreams { get; init; } = [];

    public IReadOnlyList<InvidiousVideoCaptionDto> Captions { get; init; } = [];
}
