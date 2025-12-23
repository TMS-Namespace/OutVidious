using System.Text.RegularExpressions;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Contracts;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Enums;
using TMS.Apps.Web.OutVidious.Providers.Invidious.ApiModels;

namespace TMS.Apps.Web.OutVidious.Providers.Invidious.Mappers;

/// <summary>
/// Maps Invidious thumbnail DTOs to common ThumbnailInfo contracts.
/// </summary>
public static partial class ThumbnailMapper
{
    // Matches video thumbnails: /vi/VIDEO_ID/quality.jpg or /vi/VIDEO_ID/quality.webp
    [GeneratedRegex(@"/vi/([^/]+)/([^/]+)$", RegexOptions.Compiled)]
    private static partial Regex VideoThumbnailPattern();

    // Matches channel avatars that go through ggpht proxy: /ggpht/path
    [GeneratedRegex(@"/ggpht/(.+)$", RegexOptions.Compiled)]
    private static partial Regex ChannelAvatarGgphtPattern();

    // Matches googleusercontent URLs to extract the image identifier
    [GeneratedRegex(@"googleusercontent\.com/([^?]+)", RegexOptions.Compiled)]
    private static partial Regex GoogleUserContentPattern();

    /// <summary>
    /// Maps an Invidious video thumbnail DTO to a ThumbnailInfo contract.
    /// </summary>
    public static ThumbnailInfo ToThumbnailInfo(InvidiousVideoThumbnailDto dto)
    {
        var originalUrl = ExtractVideoThumbnailUrl(dto.Url);

        return new ThumbnailInfo
        {
            Quality = ParseThumbnailQuality(dto.Quality),
            Url = originalUrl,
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

        var originalUrl = ExtractAvatarUrl(dto.Url);

        return new ThumbnailInfo
        {
            Quality = quality,
            Url = originalUrl,
            Width = dto.Width,
            Height = dto.Height
        };
    }

    /// <summary>
    /// Extracts the original YouTube URL from a video thumbnail provider URL.
    /// </summary>
    public static Uri ExtractVideoThumbnailUrl(string providerUrl)
    {
        var match = VideoThumbnailPattern().Match(providerUrl);
        if (match.Success)
        {
            var videoId = match.Groups[1].Value;
            var qualityFile = match.Groups[2].Value;
            // Original YouTube URL
            return new Uri($"https://i.ytimg.com/vi/{videoId}/{qualityFile}", UriKind.Absolute);
        }

        // Fallback: use provider URL as-is
        return new Uri(providerUrl, UriKind.RelativeOrAbsolute);
    }

    /// <summary>
    /// Extracts the original YouTube URL from a channel avatar provider URL.
    /// </summary>
    public static Uri ExtractAvatarUrl(string providerUrl)
    {
        // Channel avatars through ggpht proxy: /ggpht/path
        var ggphtMatch = ChannelAvatarGgphtPattern().Match(providerUrl);
        if (ggphtMatch.Success)
        {
            var path = ggphtMatch.Groups[1].Value;
            // Original YouTube URL
            return new Uri($"https://yt3.ggpht.com/{path}", UriKind.Absolute);
        }

        // URLs that already contain googleusercontent.com - keep as-is
        var googleMatch = GoogleUserContentPattern().Match(providerUrl);
        if (googleMatch.Success)
        {
            return new Uri(providerUrl, UriKind.Absolute);
        }

        // Fallback: use provider URL as-is
        return new Uri(providerUrl, UriKind.RelativeOrAbsolute);
    }

    /// <summary>
    /// Extracts the original YouTube URL from a channel banner provider URL.
    /// </summary>
    public static Uri ExtractBannerUrl(string providerUrl)
    {
        // Banners follow similar patterns to avatars
        var ggphtMatch = ChannelAvatarGgphtPattern().Match(providerUrl);
        if (ggphtMatch.Success)
        {
            var path = ggphtMatch.Groups[1].Value;
            // Original YouTube URL
            return new Uri($"https://yt3.ggpht.com/{path}", UriKind.Absolute);
        }

        // URLs that already contain googleusercontent.com - keep as-is
        var googleMatch = GoogleUserContentPattern().Match(providerUrl);
        if (googleMatch.Success)
        {
            return new Uri(providerUrl, UriKind.Absolute);
        }

        // Fallback: use provider URL as-is
        return new Uri(providerUrl, UriKind.RelativeOrAbsolute);
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
