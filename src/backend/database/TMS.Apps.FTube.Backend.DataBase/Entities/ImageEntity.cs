namespace TMS.Apps.FTube.Backend.DataBase.Entities;

/// <summary>
/// Represents an image stored in the database.
/// Can be a thumbnail, avatar, or banner.
/// </summary>
public class ImageEntity
{
    public int Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? LastSyncedAt { get; set; }

    /// <summary>
    /// Remote identifier (e.g., YouTube thumbnail ID).
    /// </summary>
    public string? RemoteId { get; set; }

    /// <summary>
    /// URL to the original image (for remote images).
    /// </summary>
    public string? RemoteUrl { get; set; }

    /// <summary>
    /// Binary image data (if cached locally).
    /// </summary>
    public byte[]? Data { get; set; }

    /// <summary>
    /// Image width in pixels.
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Image height in pixels.
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Quality identifier (e.g., "default", "medium", "high", "maxres").
    /// </summary>
    public string? Quality { get; set; }

    /// <summary>
    /// MIME type of the image (e.g., "image/jpeg", "image/webp").
    /// </summary>
    public string? MimeType { get; set; }

    // Navigation properties
    public ICollection<ChannelAvatarMapEntity> ChannelAvatars { get; set; } = [];

    public ICollection<ChannelBannerMapEntity> ChannelBanners { get; set; } = [];

    public ICollection<VideoThumbnailMapEntity> VideoThumbnails { get; set; } = [];
}
