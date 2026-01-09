namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Raw video thumbnail DTO from the Invidious API.
/// </summary>
internal sealed record VideoThumbnail
{
    public string Quality { get; init; } = string.Empty;

    public string Url { get; init; } = string.Empty;

    public int Width { get; init; }

    public int Height { get; init; }
}
