namespace TMS.Apps.Web.OutVidious.Providers.Invidious.ApiModels;

/// <summary>
/// Raw channel videos response DTO from the Invidious API.
/// Used for the /api/v1/channels/{ucid}/videos endpoint.
/// </summary>
public sealed record InvidiousChannelVideosResponseDto
{
    public IReadOnlyList<InvidiousChannelVideoDto> Videos { get; init; } = [];

    public string? Continuation { get; init; }
}
