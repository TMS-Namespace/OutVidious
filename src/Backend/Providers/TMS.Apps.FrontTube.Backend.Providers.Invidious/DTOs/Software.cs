namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Software information from instance stats.
/// </summary>
internal sealed record Software
{
    public string Name { get; init; } = string.Empty;

    public string Version { get; init; } = string.Empty;

    public string Branch { get; init; } = string.Empty;
}
