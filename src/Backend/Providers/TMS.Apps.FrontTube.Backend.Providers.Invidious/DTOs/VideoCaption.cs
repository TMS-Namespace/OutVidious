using System.Text.Json.Serialization;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Raw video caption/subtitle DTO from the Invidious API.
/// </summary>
internal sealed record VideoCaption
{
    public string Label { get; init; } = string.Empty;

    [JsonPropertyName("language_code")]
    public string LanguageCode { get; init; } = string.Empty;

    public string Url { get; init; } = string.Empty;
}
