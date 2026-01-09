namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Raw channel videos response DTO from the Invidious API.
/// Used for the /api/v1/channels/{ucid}/videos endpoint.
/// </summary>
internal sealed record ChannelVideosResponse
{
    public IReadOnlyList<ChannelVideo> Videos { get; init; } = [];

    public string? Continuation { get; init; }
}
