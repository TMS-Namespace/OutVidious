namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Usage statistics from instance stats.
/// </summary>
internal sealed record Usage
{
    public UsageUsers Users { get; init; } = new();
}
