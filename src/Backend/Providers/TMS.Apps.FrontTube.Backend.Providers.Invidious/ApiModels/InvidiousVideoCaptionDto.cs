using System.Text.Json.Serialization;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.ApiModels;

/// <summary>
/// Raw video caption/subtitle DTO from the Invidious API.
/// </summary>
public sealed record InvidiousVideoCaptionDto
{
    public string Label { get; init; } = string.Empty;

    [JsonPropertyName("language_code")]
    public string LanguageCode { get; init; } = string.Empty;

    public string Url { get; init; } = string.Empty;
}
