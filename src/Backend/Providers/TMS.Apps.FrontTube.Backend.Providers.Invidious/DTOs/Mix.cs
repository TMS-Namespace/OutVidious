namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Mix (auto-generated playlist) from the Invidious API.
/// </summary>
internal sealed record Mix
{
    public string Title { get; init; } = string.Empty;

    public string MixId { get; init; } = string.Empty;

    public IReadOnlyList<PlaylistVideo> Videos { get; init; } = [];
}
