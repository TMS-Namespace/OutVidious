using TMS.Apps.FrontTube.Backend.Repository.Data.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Repository.Data.Contracts;

/// <summary>
/// Represents an image stored in the repository.
/// </summary>
public sealed class ImageDomain : ICacheableDomain
{
    public int Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? LastSyncedAt { get; set; }

    /// <summary>
    /// XxHash64 hash of the absolute remote URL for unique lookup.
    /// </summary>
    public long Hash { get; set; }

    /// <summary>
    /// Absolute URL to the original image source.
    /// </summary>
    public string AbsoluteRemoteUrl { get; set; } = string.Empty;

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

    public string? FetchingError { get; set; }
}
