using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;

namespace TMS.Apps.FrontTube.Backend.Repository.Cache.Mappers;

/// <summary>
/// Maps between database entities and provider contracts for images/thumbnails.
/// </summary>
public static class ImageEntityMapper
{
    /// <summary>
    /// Converts an ImageEntity to a ThumbnailInfo contract.
    /// Quality is inferred from dimensions.
    /// </summary>
    public static ThumbnailInfo ToThumbnailInfo(ImageEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var width = entity.Width ?? 0;
        var height = entity.Height ?? 0;

        return new ThumbnailInfo
        {
            Quality = InferQualityFromDimensions(width, height),
            Url = new Uri(entity.RemoteUrl, UriKind.Absolute),
            Width = width,
            Height = height
        };
    }

    /// <summary>
    /// Converts a ThumbnailInfo contract to an ImageEntity for database storage.
    /// Quality is not stored - it's inferred from dimensions.
    /// </summary>
    public static ImageEntity ToEntity(ThumbnailInfo contract)
    {
        ArgumentNullException.ThrowIfNull(contract);

        return new ImageEntity
        {
            RemoteUrl = contract.Url.ToString(),
            Width = contract.Width,
            Height = contract.Height,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates an existing entity with data from a contract.
    /// </summary>
    public static void UpdateEntity(ImageEntity entity, ThumbnailInfo contract)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(contract);

        // RemoteUrl should not change - it's the unique identifier
        entity.Width = contract.Width;
        entity.Height = contract.Height;
    }

    /// <summary>
    /// Infers thumbnail quality from dimensions.
    /// Based on standard YouTube thumbnail sizes.
    /// </summary>
    private static ThumbnailQuality InferQualityFromDimensions(int width, int height)
    {
        // YouTube thumbnail standard sizes:
        // default: 120x90
        // medium: 320x180
        // high: 480x360
        // standard: 640x480
        // maxres: 1280x720 or higher

        var pixels = width * height;

        return pixels switch
        {
            >= 921600 => ThumbnailQuality.MaxRes,    // 1280x720 = 921,600
            >= 307200 => ThumbnailQuality.Standard,  // 640x480 = 307,200
            >= 172800 => ThumbnailQuality.High,      // 480x360 = 172,800
            >= 57600 => ThumbnailQuality.Medium,     // 320x180 = 57,600
            > 0 => ThumbnailQuality.Default,         // 120x90 = 10,800
            _ => ThumbnailQuality.Unknown
        };
    }
}
