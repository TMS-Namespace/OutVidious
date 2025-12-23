namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.ApiModels;

/// <summary>
/// Raw channel banner DTO from the Invidious API.
/// </summary>
public sealed record InvidiousChannelBannerDto
{
    public string Url { get; init; } = string.Empty;

    public int Width { get; init; }

    public int Height { get; init; }
}
