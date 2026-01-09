namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Instance statistics from the Invidious API.
/// </summary>
internal sealed record InstanceStats
{
    public string Version { get; init; } = string.Empty;

    public Software Software { get; init; } = new();

    public bool OpenRegistrations { get; init; }

    public Usage Usage { get; init; } = new();

    public Metadata Metadata { get; init; } = new();
}
