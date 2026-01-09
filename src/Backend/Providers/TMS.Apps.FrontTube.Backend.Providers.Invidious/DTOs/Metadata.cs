namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Metadata from instance stats.
/// </summary>
internal sealed record Metadata
{
    public long UpdatedAt { get; init; }

    public long LastChannelRefreshedAt { get; init; }
}
