namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

internal sealed record VideoDetails
{
    public string Title { get; init; } = string.Empty;

    public string VideoId { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string DescriptionHtml { get; init; } = string.Empty;

    public long Published { get; init; }

    public string PublishedText { get; init; } = string.Empty;

    public long ViewCount { get; init; }

    public long LikeCount { get; init; }

    public long DislikeCount { get; init; }

    public bool LiveNow { get; init; }

    public bool IsUpcoming { get; init; }

    public long? PremiereTimestamp { get; init; }

    public bool IsFamilyFriendly { get; init; }

    public bool IsListed { get; init; }

    public bool AllowRatings { get; init; }

    public bool Premium { get; init; }

    public bool Paid { get; init; }

    public int LengthSeconds { get; init; }

    public string Author { get; init; } = string.Empty;

    public string AuthorId { get; init; } = string.Empty;

    public IReadOnlyList<AuthorThumbnail> AuthorThumbnails { get; init; } = [];

    public string SubCountText { get; init; } = string.Empty;

    public IReadOnlyList<VideoThumbnail> VideoThumbnails { get; init; } = [];

    public IReadOnlyList<AdaptiveFormat> AdaptiveFormats { get; init; } = [];

    public IReadOnlyList<FormatStream> FormatStreams { get; init; } = [];

    public IReadOnlyList<VideoCaption> Captions { get; init; } = [];

    public string? DashUrl { get; init; }

    public string? HlsUrl { get; init; }

    public IReadOnlyList<string> Keywords { get; init; } = [];

    public string? Genre { get; init; }

    public IReadOnlyList<string> AllowedRegions { get; init; } = [];

    public IReadOnlyList<Storyboard>? Storyboards { get; init; }

    public IReadOnlyList<MusicTrack>? MusicTracks { get; init; }

    public IReadOnlyList<RecommendedVideo>? RecommendedVideos { get; init; }
}
