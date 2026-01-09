using System.Text.Json.Serialization;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.Tools;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Raw channel video DTO from the Invidious API.
/// Used for videos in channel listings.
/// </summary>
internal sealed record ChannelVideo
{
    public string Type { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string VideoId { get; init; } = string.Empty;

    public string Author { get; init; } = string.Empty;

    public string AuthorId { get; init; } = string.Empty;

    public string AuthorUrl { get; init; } = string.Empty;

    public IReadOnlyList<VideoThumbnail> VideoThumbnails { get; init; } = [];

    public string? Description { get; init; }

    public string? DescriptionHtml { get; init; }

    public long ViewCount { get; init; }

    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? ViewCountText { get; init; }

    public long Published { get; init; }

    public string PublishedText { get; init; } = string.Empty;

    public int LengthSeconds { get; init; }

    public bool LiveNow { get; init; }

    public bool Premium { get; init; }

    public bool IsUpcoming { get; init; }
}
