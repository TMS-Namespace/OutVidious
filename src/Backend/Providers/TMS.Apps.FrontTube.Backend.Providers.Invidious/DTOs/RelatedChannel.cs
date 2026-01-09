namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Raw related channel DTO from the Invidious API.
/// </summary>
internal sealed record RelatedChannel
{
    public string Author { get; init; } = string.Empty;

    public string AuthorId { get; init; } = string.Empty;

    public string AuthorUrl { get; init; } = string.Empty;

    public IReadOnlyList<AuthorThumbnail> AuthorThumbnails { get; init; } = [];
}
