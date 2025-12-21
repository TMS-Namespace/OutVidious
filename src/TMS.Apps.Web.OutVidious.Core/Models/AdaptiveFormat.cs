using System.Text.Json.Serialization;
using TMS.Apps.Web.OutVidious.Core.Converters;

namespace TMS.Apps.Web.OutVidious.Core.Models;

/// <summary>
/// Represents an adaptive format stream (audio-only or video-only) from the Invidious API.
/// </summary>
public sealed record AdaptiveFormat
{
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string Index { get; init; } = string.Empty;

    [JsonConverter(typeof(FlexibleStringConverter))]
    public string Bitrate { get; init; } = string.Empty;

    [JsonConverter(typeof(FlexibleStringConverter))]
    public string Init { get; init; } = string.Empty;

    public string Url { get; init; } = string.Empty;

    [JsonConverter(typeof(FlexibleStringConverter))]
    public string Itag { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    [JsonConverter(typeof(FlexibleStringConverter))]
    public string Clen { get; init; } = string.Empty;

    [JsonConverter(typeof(FlexibleStringConverter))]
    public string Lmt { get; init; } = string.Empty;

    public string? ProjectionType { get; init; }

    public string? Container { get; init; }

    public string? Encoding { get; init; }

    public string? QualityLabel { get; init; }

    public string? Resolution { get; init; }

    public int? Fps { get; init; }

    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? Size { get; init; }

    public string? AudioQuality { get; init; }

    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? AudioSampleRate { get; init; }

    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? AudioChannels { get; init; }
}
