using System.Text.RegularExpressions;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.Mappers;

/// <summary>
/// Maps Invidious thumbnail DTOs to common ThumbnailInfo contracts.
/// </summary>
internal static partial class ThumbnailMapper
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
    public static ImageMetadataCommon ToThumbnailInfo(VideoThumbnail dto)
    {
        var originalUrl = ExtractVideoThumbnailUrl(dto.Url);

        return new ImageMetadataCommon
        {
            Quality = ParseThumbnailQuality(dto.Quality),
            RemoteIdentity = new RemoteIdentityCommon(
                RemoteIdentityTypeCommon.Image,
                originalUrl.ToString()),
            Width = dto.Width,
            Height = dto.Height
        };
    }

    /// <summary>
    /// Maps an Invidious author thumbnail DTO to a ThumbnailInfo contract.
    /// </summary>
    public static ImageMetadataCommon ToChannelThumbnailInfo(AuthorThumbnail dto)
    {
        // Determine quality based on dimensions
        var quality = dto.Width switch
        {
            >= 512 => ImageQuality.MaxRes,
            >= 176 => ImageQuality.High,
            >= 88 => ImageQuality.Medium,
            _ => ImageQuality.Default
        };

        var originalUrl = ExtractAvatarUrl(dto.Url);

        return new ImageMetadataCommon
        {
            Quality = quality,
            RemoteIdentity = new RemoteIdentityCommon(
                RemoteIdentityTypeCommon.Image,
                originalUrl.ToString()),
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

    private static ImageQuality ParseThumbnailQuality(string quality)
    {
        return quality.ToLowerInvariant() switch
        {
            "default" => ImageQuality.Default,
            "medium" => ImageQuality.Medium,
            "high" => ImageQuality.High,
            "standard" or "sd" or "sddefault" => ImageQuality.Standard,
            "maxres" or "maxresdefault" => ImageQuality.MaxRes,
            _ => ImageQuality.Unknown
        };
    }
}
