using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Contracts;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Enums;
using TMS.Apps.Web.OutVidious.Providers.Invidious.ApiModels;

namespace TMS.Apps.Web.OutVidious.Providers.Invidious.Mappers;

/// <summary>
/// Maps Invidious channel DTOs to common contracts.
/// </summary>
public static class ChannelMapper
{
    private static readonly Dictionary<string, string> TabDisplayNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["videos"] = "Videos",
        ["shorts"] = "Shorts",
        ["streams"] = "Live",
        ["playlists"] = "Playlists",
        ["community"] = "Community",
        ["channels"] = "Channels",
        ["about"] = "About"
    };

    /// <summary>
    /// Maps an Invidious channel DTO to a ChannelDetails contract.
    /// </summary>
    public static ChannelDetails ToChannelDetails(InvidiousChannelDto dto, Uri baseUrl)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(baseUrl);

        var channelUrl = !string.IsNullOrWhiteSpace(dto.AuthorUrl)
            ? TryCreateUri($"{baseUrl.ToString().TrimEnd('/')}{dto.AuthorUrl}")
            : null;

        return new ChannelDetails
        {
            ChannelId = dto.AuthorId,
            Name = dto.Author,
            Description = dto.Description ?? string.Empty,
            DescriptionHtml = dto.DescriptionHtml,
            ChannelUrl = channelUrl,
            SubscriberCount = dto.SubCount,
            SubscriberCountText = FormatSubscriberCount(dto.SubCount),
            TotalViewCount = dto.TotalViews,
            JoinedAt = dto.Joined > 0 ? DateTimeOffset.FromUnixTimeSeconds(dto.Joined) : null,
            Avatars = dto.AuthorThumbnails.Select(ThumbnailMapper.ToChannelThumbnailInfo).ToList(),
            Banners = dto.AuthorBanners.Select(ToBannerThumbnailInfo).ToList(),
            AvailableTabs = dto.Tabs.Select(ToChannelTab).ToList(),
            IsVerified = false, // Invidious doesn't expose this directly
            Keywords = []
        };
    }

    /// <summary>
    /// Maps an Invidious channel video DTO to a VideoSummary contract.
    /// </summary>
    public static VideoSummary ToVideoSummary(InvidiousChannelVideoDto dto, Uri baseUrl)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(baseUrl);

        var channelUrl = !string.IsNullOrWhiteSpace(dto.AuthorUrl)
            ? TryCreateUri($"{baseUrl.ToString().TrimEnd('/')}{dto.AuthorUrl}")
            : null;

        return new VideoSummary
        {
            VideoId = dto.VideoId,
            Title = dto.Title,
            Duration = TimeSpan.FromSeconds(dto.LengthSeconds),
            ViewCount = dto.ViewCount,
            ViewCountText = dto.ViewCountText ?? FormatViewCount(dto.ViewCount),
            PublishedTimeText = dto.PublishedText,
            PublishedAt = dto.Published > 0 ? DateTimeOffset.FromUnixTimeSeconds(dto.Published) : null,
            Channel = new ChannelInfo
            {
                ChannelId = dto.AuthorId,
                Name = dto.Author,
                ChannelUrl = channelUrl
            },
            Thumbnails = dto.VideoThumbnails.Select(ThumbnailMapper.ToThumbnailInfo).ToList(),
            IsLive = dto.LiveNow,
            IsUpcoming = dto.IsUpcoming,
            IsShort = dto.Type == "shortVideo"
        };
    }

    private static ThumbnailInfo ToBannerThumbnailInfo(InvidiousChannelBannerDto dto)
    {
        // Determine quality based on width
        var quality = dto.Width switch
        {
            >= 2560 => ThumbnailQuality.MaxRes,
            >= 1920 => ThumbnailQuality.High,
            >= 1280 => ThumbnailQuality.Standard,
            >= 640 => ThumbnailQuality.Medium,
            _ => ThumbnailQuality.Default
        };

        var originalUrl = ThumbnailMapper.ExtractBannerUrl(dto.Url);

        return new ThumbnailInfo
        {
            Quality = quality,
            Url = originalUrl,
            Width = dto.Width,
            Height = dto.Height
        };
    }

    private static ChannelTab ToChannelTab(string tabName)
    {
        var displayName = TabDisplayNames.TryGetValue(tabName, out var name) ? name : tabName;
        
        return new ChannelTab
        {
            TabId = tabName.ToLowerInvariant(),
            DisplayName = displayName,
            IsAvailable = true
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

    private static Uri? TryCreateUri(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return uri;
        }

        return null;
    }
}
