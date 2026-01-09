namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Raw channel banner DTO from the Invidious API.
/// </summary>
internal sealed record ChannelBanner
{
    public string Url { get; init; } = string.Empty;

    public int Width { get; init; }

    public int Height { get; init; }
}
