namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Represents a channel tab type (Videos, Shorts, Live, Playlists, etc.).
/// </summary>
public sealed record ChannelTab // TODO: not needed, just return list of tab names as strings
{
    /// <summary>
    /// Tab identifier used in API calls.
    /// </summary>
    [Obsolete("Use Name instead")]
    public required string RemoteTabId { get; init; }

    /// <summary>
    /// Display name of the tab.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Whether this tab is currently available.
    /// </summary>
    public bool IsAvailable { get; init; } = true;
}
