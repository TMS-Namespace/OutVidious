using TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;

/// <summary>
/// Represents an image stored in the database.
/// Can be a thumbnail, avatar, or banner.
/// </summary>
public class ImageEntity : TrackableEntitiesBase, ICacheableEntity
{
    //public int Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? LastSyncedAt { get; set; }

    /// <summary>
    /// XxHash64 hash of the absolute remote URL for unique lookup.
    /// </summary>
    public required long Hash { get; set; }

    /// <summary>
    /// Absolute URL to the original image source (e.g., https://i.ytimg.com/...).
    /// Used as the unique identifier for the image.
    /// </summary>
    public required string AbsoluteRemoteUrl { get; set; }

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
    /// MIME type of the image (e.g., "image/jpeg", "image/webp").
    /// </summary>
    public string? MimeType { get; set; }

    // Navigation properties
    public ICollection<ChannelAvatarMapEntity> ChannelAvatars { get; set; } = [];

    public ICollection<ChannelBannerMapEntity> ChannelBanners { get; set; } = [];

    public ICollection<VideoThumbnailMapEntity> VideoThumbnails { get; set; } = [];
}
