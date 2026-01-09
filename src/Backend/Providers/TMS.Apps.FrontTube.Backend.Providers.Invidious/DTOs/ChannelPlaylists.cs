namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Channel playlists tab response from the Invidious API.
/// </summary>
internal sealed record ChannelPlaylists
{
    public IReadOnlyList<ChannelPlaylistItem> Playlists { get; init; } = [];

    public string? Continuation { get; init; }
}
