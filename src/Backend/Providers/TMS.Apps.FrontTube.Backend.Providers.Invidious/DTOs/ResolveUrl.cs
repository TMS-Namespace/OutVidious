namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Resolve URL response from the Invidious API.
/// Used to resolve YouTube URLs to their underlying resources.
/// </summary>
internal sealed record ResolveUrl
{
    public string Type { get; init; } = string.Empty;

    public string? VideoId { get; init; }

    public string? PlaylistId { get; init; }

    public string? ChannelId { get; init; }

    public int? StartTimeSeconds { get; init; }
}
