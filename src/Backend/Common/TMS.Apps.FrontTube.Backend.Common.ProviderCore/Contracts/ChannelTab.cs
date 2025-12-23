namespace TMS.Apps.Web.OutVidious.Common.ProvidersCore.Contracts;

/// <summary>
/// Represents a channel tab type (Videos, Shorts, Live, Playlists, etc.).
/// </summary>
public sealed record ChannelTab
{
    /// <summary>
    /// Tab identifier used in API calls.
    /// </summary>
    public required string TabId { get; init; }

    /// <summary>
    /// Display name of the tab.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Whether this tab is currently available.
    /// </summary>
    public bool IsAvailable { get; init; } = true;
}
