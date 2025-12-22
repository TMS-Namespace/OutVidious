namespace TMS.Apps.Web.OutVidious.Providers.Invidious.ApiModels;

/// <summary>
/// Raw author/channel thumbnail DTO from the Invidious API.
/// </summary>
public sealed record InvidiousAuthorThumbnailDto
{
    public string Url { get; init; } = string.Empty;

    public int Width { get; init; }

    public int Height { get; init; }
}
