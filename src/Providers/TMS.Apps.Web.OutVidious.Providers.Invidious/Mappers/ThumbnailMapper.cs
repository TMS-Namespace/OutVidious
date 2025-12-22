using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Contracts;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Enums;
using TMS.Apps.Web.OutVidious.Providers.Invidious.ApiModels;

namespace TMS.Apps.Web.OutVidious.Providers.Invidious.Mappers;

/// <summary>
/// Maps Invidious thumbnail DTOs to common ThumbnailInfo contracts.
/// </summary>
public static class ThumbnailMapper
{
    /// <summary>
    /// Maps an Invidious video thumbnail DTO to a ThumbnailInfo contract.
    /// </summary>
    public static ThumbnailInfo ToThumbnailInfo(InvidiousVideoThumbnailDto dto)
    {
        return new ThumbnailInfo
        {
            Quality = ParseThumbnailQuality(dto.Quality),
            Url = new Uri(dto.Url, UriKind.RelativeOrAbsolute),
            Width = dto.Width,
            Height = dto.Height
        };
    }

    /// <summary>
    /// Maps an Invidious author thumbnail DTO to a ThumbnailInfo contract.
    /// </summary>
    public static ThumbnailInfo ToChannelThumbnailInfo(InvidiousAuthorThumbnailDto dto)
    {
        // Determine quality based on dimensions
        var quality = dto.Width switch
        {
            >= 512 => ThumbnailQuality.MaxRes,
            >= 176 => ThumbnailQuality.High,
            >= 88 => ThumbnailQuality.Medium,
            _ => ThumbnailQuality.Default
        };

        return new ThumbnailInfo
        {
            Quality = quality,
            Url = new Uri(dto.Url, UriKind.RelativeOrAbsolute),
            Width = dto.Width,
            Height = dto.Height
        };
    }

    private static ThumbnailQuality ParseThumbnailQuality(string quality)
    {
        return quality.ToLowerInvariant() switch
        {
            "default" => ThumbnailQuality.Default,
            "medium" => ThumbnailQuality.Medium,
            "high" => ThumbnailQuality.High,
            "standard" or "sd" or "sddefault" => ThumbnailQuality.Standard,
            "maxres" or "maxresdefault" => ThumbnailQuality.MaxRes,
            _ => ThumbnailQuality.Unknown
        };
    }
}
