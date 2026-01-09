namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Music track information from the Invidious API.
/// </summary>
internal sealed record MusicTrack
{
    public string Song { get; init; } = string.Empty;

    public string Artist { get; init; } = string.Empty;

    public string Album { get; init; } = string.Empty;

    public string License { get; init; } = string.Empty;
}
