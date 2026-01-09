using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Result of resolving a URL to its underlying resource.
/// </summary>
public sealed record ResolvedUrlCommon : ICommonContract
{
    /// <summary>
    /// Type of the resolved resource.
    /// </summary>
    public required ResolvedUrlType Type { get; init; }

    /// <summary>
    /// Video ID (if type is Video or Clip).
    /// </summary>
    public string? VideoId { get; init; }

    /// <summary>
    /// Playlist ID (if type is Playlist).
    /// </summary>
    public string? PlaylistId { get; init; }

    /// <summary>
    /// Channel ID (if type is Channel).
    /// </summary>
    public string? ChannelId { get; init; }

    /// <summary>
    /// Start time in seconds (for timestamped video links).
    /// </summary>
    public int? StartTimeSeconds { get; init; }
}
