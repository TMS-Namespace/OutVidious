namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// User statistics from instance stats.
/// </summary>
internal sealed record UsageUsers
{
    public long Total { get; init; }

    public long ActiveHalfYear { get; init; }

    public long ActiveMonth { get; init; }
}
