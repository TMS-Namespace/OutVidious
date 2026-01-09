namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Individual playlist item in channel playlists tab.
/// </summary>
internal sealed record ChannelPlaylistItem
{
    public string Type { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string PlaylistId { get; init; } = string.Empty;

    public string PlaylistThumbnail { get; init; } = string.Empty;

    public string Author { get; init; } = string.Empty;

    public string AuthorId { get; init; } = string.Empty;

    public string AuthorUrl { get; init; } = string.Empty;

    public int VideoCount { get; init; }

    public IReadOnlyList<PlaylistVideo> Videos { get; init; } = [];
}
