using System.Text.Json.Serialization;
using TMS.Apps.Web.OutVidious.Providers.Invidious.Converters;

namespace TMS.Apps.Web.OutVidious.Providers.Invidious.ApiModels;

/// <summary>
/// Raw format stream (video+audio combined) DTO from the Invidious API.
/// </summary>
public sealed record InvidiousFormatStreamDto
{
    public string Url { get; init; } = string.Empty;

    [JsonConverter(typeof(FlexibleStringConverter))]
    public string Itag { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public string Quality { get; init; } = string.Empty;

    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? Bitrate { get; init; }

    public string Container { get; init; } = string.Empty;

    public string Encoding { get; init; } = string.Empty;

    public string QualityLabel { get; init; } = string.Empty;

    public string Resolution { get; init; } = string.Empty;

    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? Size { get; init; }
}
