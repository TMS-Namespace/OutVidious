using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Enums;

namespace TMS.Apps.Web.OutVidious.Common.ProvidersCore.Contracts;

/// <summary>
/// Represents a thumbnail image with strongly typed dimensions and quality.
/// </summary>
public sealed record ThumbnailInfo
{
    /// <summary>
    /// Quality level of the thumbnail.
    /// </summary>
    public required ThumbnailQuality Quality { get; init; }

    /// <summary>
    /// URL to the original image source (e.g., YouTube CDN URL like https://i.ytimg.com/...).
    /// This is the unique identifier for the image.
    /// </summary>
    public required Uri Url { get; init; }

    /// <summary>
    /// Width of the thumbnail in pixels.
    /// </summary>
    public required int Width { get; init; }

    /// <summary>
    /// Height of the thumbnail in pixels.
    /// </summary>
    public required int Height { get; init; }
}
