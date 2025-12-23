namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.ApiModels;

/// <summary>
/// Raw video thumbnail DTO from the Invidious API.
/// </summary>
public sealed record InvidiousVideoThumbnailDto
{
    public string Quality { get; init; } = string.Empty;

    public string Url { get; init; } = string.Empty;

    public int Width { get; init; }

    public int Height { get; init; }
}
