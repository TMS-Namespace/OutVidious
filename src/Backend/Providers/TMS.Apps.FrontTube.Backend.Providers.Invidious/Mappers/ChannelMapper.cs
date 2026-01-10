using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.Tools;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.Mappers;

/// <summary>
/// Maps Invidious channel DTOs to common contracts.
/// </summary>
internal static class ChannelMapper
{
    /// <summary>
    /// Maps an Invidious channel DTO to a ChannelDetails contract.
    /// </summary>
    public static ChannelCommon ToChannelDetails(Channel dto, Uri baseUrl)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(baseUrl);

        return new ChannelCommon
        {
            RemoteIdentity = new RemoteIdentityCommon(
                RemoteIdentityTypeCommon.Channel,
                dto.AuthorId),
            Name = dto.Author,
            Description = dto.Description ?? string.Empty,
            DescriptionHtml = dto.DescriptionHtml,
            SubscriberCount = dto.SubCount,
#pragma warning disable CS0618 // Type or member is obsolete
            SubscriberCountText = FormatSubscriberCount(dto.SubCount),
#pragma warning restore CS0618 // Type or member is obsolete
            TotalViewCount = dto.TotalViews,
            JoinedAt = dto.Joined > 0 ? DateTimeOffset.FromUnixTimeSeconds(dto.Joined) : null,
            Avatars = dto.AuthorThumbnails.Select(ThumbnailMapper.ToChannelThumbnailInfo).ToList(),
            Banners = dto.AuthorBanners.Select(ToBannerThumbnailInfo).ToList(),
            AvailableTabs = dto.Tabs.Select(t => t.ToChannelTabEnum()).ToList(),
            IsVerified = false, // Invidious doesn't expose this directly
            Tags = []
        };
    }

    /// <summary>
    /// Maps an Invidious channel video DTO to a VideoSummary contract.
    /// </summary>
    public static VideoMetadataCommon ToVideoSummary(ChannelVideo dto, Uri baseUrl)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(baseUrl);

        return new VideoMetadataCommon
        {
            RemoteIdentity = new RemoteIdentityCommon(
                RemoteIdentityTypeCommon.Video,
                dto.VideoId),
            Title = dto.Title,
            Duration = TimeSpan.FromSeconds(dto.LengthSeconds),
            ViewCount = dto.ViewCount,
#pragma warning disable CS0618 // Type or member is obsolete
            ViewCountText = dto.ViewCountText ?? FormatViewCount(dto.ViewCount),
            PublishedAgo = dto.PublishedText,
#pragma warning restore CS0618 // Type or member is obsolete
            PublishedAtUtc = dto.Published > 0 ? DateTimeOffset.FromUnixTimeSeconds(dto.Published) : null,
            Channel = new ChannelMetadataCommon
            {
                RemoteIdentity = new RemoteIdentityCommon(
                    RemoteIdentityTypeCommon.Channel,
                    dto.AuthorId),
                Name = dto.Author,
            },
            Thumbnails = dto.VideoThumbnails.Select(ThumbnailMapper.ToThumbnailInfo).ToList(),
            IsLive = dto.LiveNow,
            IsUpcoming = dto.IsUpcoming,
            IsShort = dto.Type == "shortVideo"
        };
    }

    private static ImageMetadataCommon ToBannerThumbnailInfo(ChannelBanner dto)
    {
        // Determine quality based on width
        // var quality = dto.Width switch
        // {
        //     >= 2560 => ImageQuality.MaxRes,
        //     >= 1920 => ImageQuality.High,
        //     >= 1280 => ImageQuality.Standard,
        //     >= 640 => ImageQuality.Medium,
        //     _ => ImageQuality.Default
        // };

        var originalUrl = ThumbnailMapper.ExtractBannerUrl(dto.Url);

        return new ImageMetadataCommon
        {
            //Quality = quality,
            RemoteIdentity = new RemoteIdentityCommon(
                RemoteIdentityTypeCommon.Image,
                originalUrl.ToString()),
            Width = dto.Width,
            Height = dto.Height
        };
    }

    private static string FormatSubscriberCount(long count)
    {
        return count switch
        {
            >= 1_000_000_000 => $"{count / 1_000_000_000.0:F1}B subscribers",
            >= 1_000_000 => $"{count / 1_000_000.0:F1}M subscribers",
            >= 1_000 => $"{count / 1_000.0:F1}K subscribers",
            _ => $"{count:N0} subscribers"
        };
    }

    private static string FormatViewCount(long count)
    {
        return count switch
        {
            >= 1_000_000_000 => $"{count / 1_000_000_000.0:F1}B views",
            >= 1_000_000 => $"{count / 1_000_000.0:F1}M views",
            >= 1_000 => $"{count / 1_000.0:F1}K views",
            _ => $"{count:N0} views"
        };
    }
}
