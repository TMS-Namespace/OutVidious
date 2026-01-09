namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Captions list response from the Invidious API.
/// </summary>
internal sealed record CaptionsList
{
    public IReadOnlyList<Caption> Captions { get; init; } = [];
}
