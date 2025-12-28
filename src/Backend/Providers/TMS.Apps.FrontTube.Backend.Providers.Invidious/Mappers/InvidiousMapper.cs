using TMS.Apps.FrontTube.Backend.Common.ProviderCore;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.ApiModels;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.Mappers;

/// <summary>
/// Maps Invidious API DTOs to common provider contracts.
/// </summary>
public static class InvidiousMapper
{
    /// <summary>
    /// Maps an Invidious video details DTO to a common VideoInfo contract.
    /// </summary>
    /// <param name="dto">The Invidious video details DTO.</param>
    /// <param name="baseUrl">The base URL of the Invidious instance.</param>
    /// <returns>The mapped VideoInfo contract.</returns>
    public static Video ToVideoInfo(InvidiousVideoDetailsDto dto, Uri baseUrl)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(baseUrl);

        return new Video
        {
            AbsoluteRemoteUrl = YouTubeUrlBuilder.BuildVideoUrl(dto.VideoId),
            Title = dto.Title,
            DescriptionText = dto.Description,
            DescriptionHtml = dto.DescriptionHtml,
            Channel = MapChannel(dto),
            PublishedAtUtc = dto.Published > 0 
                ? DateTimeOffset.FromUnixTimeSeconds(dto.Published) 
                : null,
            PublishedAgo = dto.PublishedText,
            Duration = TimeSpan.FromSeconds(dto.LengthSeconds),
            ViewCount = dto.ViewCount,
            LikeCount = dto.LikeCount,
            DislikeCount = dto.DislikeCount > 0 ? dto.DislikeCount : null,
            Tags = dto.Keywords,
            Genre = !string.IsNullOrWhiteSpace(dto.Genre) ? dto.Genre : null,
            Thumbnails = dto.VideoThumbnails.Select(ThumbnailMapper.ToThumbnailInfo).ToList(),
            AdaptiveStreams = dto.AdaptiveFormats.Select(StreamMapper.ToStreamInfo).ToList(),
            MutexStreams = dto.FormatStreams.Select(StreamMapper.ToStreamInfo).ToList(),
            Captions = dto.Captions.Select(c => CaptionMapper.ToCaptionInfo(c, baseUrl)).ToList(),
#pragma warning disable CS0618 // Type or member is obsolete
            DashManifestUrl = !string.IsNullOrWhiteSpace(dto.DashUrl) 
                ? TryCreateUri(dto.DashUrl) 
                : null,
            HlsManifestUrl = !string.IsNullOrWhiteSpace(dto.HlsUrl) 
                ? TryCreateUri(dto.HlsUrl) 
                : null,
#pragma warning restore CS0618 // Type or member is obsolete
            IsLive = dto.LiveNow,
            IsUpcoming = dto.IsUpcoming,
            PremiereTimestamp = dto.PremiereTimestamp.HasValue && dto.PremiereTimestamp > 0
                ? DateTimeOffset.FromUnixTimeSeconds(dto.PremiereTimestamp.Value)
                : null,
            IsFamilyFriendly = dto.IsFamilyFriendly,
            IsListed = dto.IsListed,
            AllowRatings = dto.AllowRatings,
            IsPremium = dto.Premium || dto.Paid,
            AllowedRegions = dto.AllowedRegions
        };
    }

    private static ChannelMetadata MapChannel(InvidiousVideoDetailsDto dto)
    {
        return new ChannelMetadata
        {
            AbsoluteRemoteUrl = YouTubeUrlBuilder.BuildChannelUrl(dto.AuthorId),
            Name = dto.Author,
#pragma warning disable CS0618 // Type or member is obsolete
            SubscriberCountText = dto.SubCountText,
#pragma warning restore CS0618 // Type or member is obsolete
            SubscriberCount = ParseSubscriberCount(dto.SubCountText),
            Avatars = dto.AuthorThumbnails
                .Select(ThumbnailMapper.ToChannelThumbnailInfo)
                .ToList()
        };
    }

    private static long? ParseSubscriberCount(string? subCountText)
    {
        if (string.IsNullOrWhiteSpace(subCountText))
        {
            return null;
        }

        // Parse strings like "1.5M subscribers", "100K", etc.
        var cleaned = subCountText
            .Replace("subscribers", "", StringComparison.OrdinalIgnoreCase)
            .Replace("subscriber", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return null;
        }

        var multiplier = 1L;
        if (cleaned.EndsWith("K", StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 1_000;
            cleaned = cleaned[..^1];
        }
        else if (cleaned.EndsWith("M", StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 1_000_000;
            cleaned = cleaned[..^1];
        }
        else if (cleaned.EndsWith("B", StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 1_000_000_000;
            cleaned = cleaned[..^1];
        }

        if (double.TryParse(cleaned.Trim(), out var value))
        {
            return (long)(value * multiplier);
        }

        return null;
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
