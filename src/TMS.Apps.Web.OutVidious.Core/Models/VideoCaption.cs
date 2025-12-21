using System.Text.Json.Serialization;

namespace TMS.Apps.Web.OutVidious.Core.Models;

/// <summary>
/// Represents a video caption/subtitle from the Invidious API.
/// </summary>
public sealed record VideoCaption
{
    public string Label { get; init; } = string.Empty;

    [JsonPropertyName("language_code")]
    public string LanguageCode { get; init; } = string.Empty;

    public string Url { get; init; } = string.Empty;
}
