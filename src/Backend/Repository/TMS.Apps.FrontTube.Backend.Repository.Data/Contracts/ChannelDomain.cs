using TMS.Apps.FrontTube.Backend.Repository.Data.Enums;
using TMS.Apps.FrontTube.Backend.Repository.Data.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Repository.Data.Contracts;

/// <summary>
/// Represents a channel.
/// </summary>
public sealed class ChannelDomain : ICacheableDomain
{
    public int Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? LastSyncedAt { get; set; }

    public required RemoteIdentityDomain RemoteIdentity { get; set; }

    /// <summary>
    /// Channel display name/title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Local alias for the channel.
    /// </summary>
    public string? Alias { get; set; }

    /// <summary>
    /// Channel description/about text.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// HTML-formatted description.
    /// </summary>
    public string? DescriptionHtml { get; set; }

    /// <summary>
    /// Channel handle (e.g., "@ChannelName").
    /// </summary>
    public string? Handle { get; set; }

    /// <summary>
    /// Subscriber count.
    /// </summary>
    public long? SubscriberCount { get; set; }

    /// <summary>
    /// Total video count.
    /// </summary>
    public int? VideoCount { get; set; }

    /// <summary>
    /// Total view count across all videos.
    /// </summary>
    public long? TotalViewCount { get; set; }

    /// <summary>
    /// When the channel was created on the platform.
    /// </summary>
    public DateTime? JoinedAt { get; set; }

    /// <summary>
    /// Whether the channel is verified.
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Keywords/tags associated with the channel (comma-separated).
    /// </summary>
    public string? Keywords { get; set; }

    /// <summary>
    /// Available channel tab types.
    /// </summary>
    public IReadOnlyList<ChannelTabType> AvailableTabs { get; set; } = [];

    // Navigation properties
    public ICollection<VideoDomain> Videos { get; set; } = [];

    public IReadOnlyList<ImageDomain> Avatars { get; set; } = [];

    public IReadOnlyList<ImageDomain> Banners { get; set; } = [];

    public string? FetchingError { get; set; }
}
