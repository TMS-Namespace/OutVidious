namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Full playlist details from the Invidious API.
/// </summary>
internal sealed record Playlist
{
    public string Title { get; init; } = string.Empty;

    public string PlaylistId { get; init; } = string.Empty;

    public string Author { get; init; } = string.Empty;

    public string AuthorId { get; init; } = string.Empty;

    public IReadOnlyList<AuthorThumbnail> AuthorThumbnails { get; init; } = [];

    public string Description { get; init; } = string.Empty;

    public string DescriptionHtml { get; init; } = string.Empty;

    public int VideoCount { get; init; }

    public long ViewCount { get; init; }

    public string? ViewCountText { get; init; }

    public long Updated { get; init; }

    public IReadOnlyList<PlaylistVideo> Videos { get; init; } = [];
}
