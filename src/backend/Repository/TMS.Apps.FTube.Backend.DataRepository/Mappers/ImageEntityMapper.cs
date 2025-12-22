using TMS.Apps.FTube.Backend.DataBase.Entities;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Contracts;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Enums;

namespace TMS.Apps.FTube.Backend.DataRepository.Mappers;

/// <summary>
/// Maps between database entities and provider contracts for images/thumbnails.
/// </summary>
public static class ImageEntityMapper
{
    /// <summary>
    /// Converts an ImageEntity to a ThumbnailInfo contract.
    /// </summary>
    public static ThumbnailInfo ToThumbnailInfo(ImageEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new ThumbnailInfo
        {
            Quality = ParseQuality(entity.Quality),
            Url = new Uri(entity.RemoteUrl ?? $"data:image/{entity.MimeType ?? "jpeg"};base64,placeholder", UriKind.RelativeOrAbsolute),
            Width = entity.Width ?? 0,
            Height = entity.Height ?? 0
        };
    }

    /// <summary>
    /// Converts a ThumbnailInfo contract to an ImageEntity for database storage.
    /// </summary>
    public static ImageEntity ToEntity(ThumbnailInfo contract)
    {
        ArgumentNullException.ThrowIfNull(contract);

        return new ImageEntity
        {
            RemoteUrl = contract.Url.ToString(),
            Width = contract.Width,
            Height = contract.Height,
            Quality = contract.Quality.ToString().ToLowerInvariant(),
            CreatedAt = DateTime.UtcNow,
            LastSyncedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates an existing entity with data from a contract.
    /// </summary>
    public static void UpdateEntity(ImageEntity entity, ThumbnailInfo contract)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(contract);

        entity.RemoteUrl = contract.Url.ToString();
        entity.Width = contract.Width;
        entity.Height = contract.Height;
        entity.Quality = contract.Quality.ToString().ToLowerInvariant();
        entity.LastSyncedAt = DateTime.UtcNow;
    }

    private static ThumbnailQuality ParseQuality(string? quality)
    {
        if (string.IsNullOrWhiteSpace(quality))
        {
            return ThumbnailQuality.Unknown;
        }

        return quality.ToLowerInvariant() switch
        {
            "default" => ThumbnailQuality.Default,
            "medium" => ThumbnailQuality.Medium,
            "high" => ThumbnailQuality.High,
            "standard" => ThumbnailQuality.Standard,
            "maxres" => ThumbnailQuality.MaxRes,
            _ => ThumbnailQuality.Unknown
        };
    }
}
