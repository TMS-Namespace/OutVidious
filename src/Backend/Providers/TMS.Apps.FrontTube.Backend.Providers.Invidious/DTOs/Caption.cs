namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Caption track from the Invidious API.
/// </summary>
internal sealed record Caption
{
    public string Label { get; init; } = string.Empty;

    public string LanguageCode { get; init; } = string.Empty;

    public string Url { get; init; } = string.Empty;
}
