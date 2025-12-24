using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Represents a thumbnail image with strongly typed dimensions and quality.
/// </summary>
public sealed record Image
{
    /// <summary>
    /// Quality level of the thumbnail.
    /// </summary>
    public required ImageQuality Quality { get; init; }

    /// <summary>
    /// URL to the original image source (e.g., YouTube CDN URL like https://i.ytimg.com/...).
    /// This is the unique identifier for the image.
    /// </summary>
    public required Uri RemoteUrl { get; init; }

    /// <summary>
    /// Width of the thumbnail in pixels.
    /// </summary>
    public required int Width { get; init; }

    /// <summary>
    /// Height of the thumbnail in pixels.
    /// </summary>
    public required int Height { get; init; }
}
