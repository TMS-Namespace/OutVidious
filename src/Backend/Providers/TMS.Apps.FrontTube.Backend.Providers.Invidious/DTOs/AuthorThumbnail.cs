namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Raw author/channel thumbnail DTO from the Invidious API.
/// </summary>
internal sealed record AuthorThumbnail
{
    public string Url { get; init; } = string.Empty;

    public int Width { get; init; }

    public int Height { get; init; }
}
