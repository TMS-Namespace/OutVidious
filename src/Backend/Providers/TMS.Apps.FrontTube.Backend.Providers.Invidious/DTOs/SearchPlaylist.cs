namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Playlist search result from the Invidious API.
/// </summary>
internal sealed record SearchPlaylist
{
    public string Type { get; init; } = "playlist";

    public string Title { get; init; } = string.Empty;

    public string PlaylistId { get; init; } = string.Empty;

    public string PlaylistThumbnail { get; init; } = string.Empty;

    public string Author { get; init; } = string.Empty;

    public string AuthorId { get; init; } = string.Empty;

    public string AuthorUrl { get; init; } = string.Empty;

    public bool AuthorVerified { get; init; }

    public int VideoCount { get; init; }

    public IReadOnlyList<PlaylistVideo> Videos { get; init; } = [];
}
