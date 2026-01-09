namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Playlist search result item.
/// </summary>
public sealed record SearchResultPlaylistCommon : SearchResultItemCommon
{
    /// <summary>
    /// Playlist ID.
    /// </summary>
    public required string PlaylistId { get; init; }

    /// <summary>
    /// Playlist title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Playlist thumbnail URL.
    /// </summary>
    public string? ThumbnailUrl { get; init; }

    /// <summary>
    /// Author/owner of the playlist.
    /// </summary>
    public required string AuthorName { get; init; }

    /// <summary>
    /// Author channel ID.
    /// </summary>
    public required string AuthorId { get; init; }

    /// <summary>
    /// Number of videos in the playlist.
    /// </summary>
    public int VideoCount { get; init; }

    /// <summary>
    /// First few videos in the playlist.
    /// </summary>
    public IReadOnlyList<VideoMetadataCommon> Videos { get; init; } = [];
}
