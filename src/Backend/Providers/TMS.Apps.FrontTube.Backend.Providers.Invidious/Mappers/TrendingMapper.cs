using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.Mappers;

/// <summary>
/// Maps Invidious trending/popular DTOs to common contracts.
/// </summary>
internal static class TrendingMapper
{
    /// <summary>
    /// Maps a list of trending video DTOs to TrendingVideosCommon.
    /// </summary>
    public static TrendingVideosCommon ToTrendingVideos(
        IReadOnlyList<TrendingVideo> dtos,
        TrendingCategory category,
        string? region,
        Uri baseUrl)
    {
        ArgumentNullException.ThrowIfNull(dtos);
        ArgumentNullException.ThrowIfNull(baseUrl);

        return new TrendingVideosCommon
        {
            Category = category,
            Region = region,
            Videos = dtos.Select(dto => ToVideoMetadata(dto, baseUrl)).ToList()
        };
    }

    /// <summary>
    /// Maps a list of popular video DTOs to TrendingVideosCommon.
    /// </summary>
    public static TrendingVideosCommon ToPopularVideos(
        IReadOnlyList<PopularVideo> dtos,
        Uri baseUrl)
    {
        ArgumentNullException.ThrowIfNull(dtos);
        ArgumentNullException.ThrowIfNull(baseUrl);

        return new TrendingVideosCommon
        {
            Category = TrendingCategory.Default,
            Videos = dtos.Select(dto => ToVideoMetadataFromPopular(dto, baseUrl)).ToList()
        };
    }

    private static VideoMetadataCommon ToVideoMetadata(TrendingVideo dto, Uri baseUrl)
    {
        return new VideoMetadataCommon
        {
            RemoteIdentity = new RemoteIdentityCommon(
                RemoteIdentityTypeCommon.Video,
                dto.VideoId),
            Title = dto.Title,
            Duration = TimeSpan.FromSeconds(dto.LengthSeconds),
            ViewCount = dto.ViewCount,
            PublishedAtUtc = dto.Published > 0 ? DateTimeOffset.FromUnixTimeSeconds(dto.Published) : null,
#pragma warning disable CS0618 // Type or member is obsolete
            PublishedAgo = dto.PublishedText,
#pragma warning restore CS0618 // Type or member is obsolete
            Channel = new ChannelMetadataCommon
            {
                RemoteIdentity = new RemoteIdentityCommon(
                    RemoteIdentityTypeCommon.Channel,
                    dto.AuthorId),
                Name = dto.Author
            },
            Thumbnails = dto.VideoThumbnails.Select(ThumbnailMapper.ToThumbnailInfo).ToList(),
            IsLive = dto.LiveNow
        };
    }

    private static VideoMetadataCommon ToVideoMetadataFromPopular(PopularVideo dto, Uri baseUrl)
    {
        return new VideoMetadataCommon
        {
            RemoteIdentity = new RemoteIdentityCommon(
                RemoteIdentityTypeCommon.Video,
                dto.VideoId),
            Title = dto.Title,
            Duration = TimeSpan.FromSeconds(dto.LengthSeconds),
            ViewCount = dto.ViewCount,
            PublishedAtUtc = dto.Published > 0 ? DateTimeOffset.FromUnixTimeSeconds(dto.Published) : null,
#pragma warning disable CS0618 // Type or member is obsolete
            PublishedAgo = dto.PublishedText,
#pragma warning restore CS0618 // Type or member is obsolete
            Channel = new ChannelMetadataCommon
            {
                RemoteIdentity = new RemoteIdentityCommon(
                    RemoteIdentityTypeCommon.Channel,
                    dto.AuthorId),
                Name = dto.Author
            },
            Thumbnails = dto.VideoThumbnails.Select(ThumbnailMapper.ToThumbnailInfo).ToList()
        };
    }
}
