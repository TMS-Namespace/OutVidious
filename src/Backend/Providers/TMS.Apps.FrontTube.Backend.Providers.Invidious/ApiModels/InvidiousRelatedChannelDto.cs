namespace TMS.Apps.Web.OutVidious.Providers.Invidious.ApiModels;

/// <summary>
/// Raw related channel DTO from the Invidious API.
/// </summary>
public sealed record InvidiousRelatedChannelDto
{
    public string Author { get; init; } = string.Empty;

    public string AuthorId { get; init; } = string.Empty;

    public string AuthorUrl { get; init; } = string.Empty;

    public IReadOnlyList<InvidiousAuthorThumbnailDto> AuthorThumbnails { get; init; } = [];
}
