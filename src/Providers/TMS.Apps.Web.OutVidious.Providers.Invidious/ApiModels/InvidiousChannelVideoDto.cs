using System.Text.Json.Serialization;
using TMS.Apps.Web.OutVidious.Providers.Invidious.Converters;

namespace TMS.Apps.Web.OutVidious.Providers.Invidious.ApiModels;

/// <summary>
/// Raw channel video DTO from the Invidious API.
/// Used for videos in channel listings.
/// </summary>
public sealed record InvidiousChannelVideoDto
{
    public string Type { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string VideoId { get; init; } = string.Empty;

    public string Author { get; init; } = string.Empty;

    public string AuthorId { get; init; } = string.Empty;

    public string AuthorUrl { get; init; } = string.Empty;

    public IReadOnlyList<InvidiousVideoThumbnailDto> VideoThumbnails { get; init; } = [];

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
