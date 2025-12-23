using TMS.Apps.FTube.Backend.DataBase.Entities;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;

namespace TMS.Apps.FTube.Backend.DataRepository.Mappers;

/// <summary>
/// Maps between database entities and provider contracts for videos.
/// </summary>
public static class VideoEntityMapper
{
    /// <summary>
    /// Converts a VideoEntity to a VideoInfo contract.
    /// </summary>
    public static VideoInfo ToContract(VideoEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var channelInfo = entity.Channel is not null
            ? ChannelEntityMapper.ToChannelInfo(entity.Channel)
            : new ChannelInfo
            {
                ChannelId = entity.ChannelId?.ToString() ?? "unknown",
                Name = "Unknown Channel"
            };

        return new VideoInfo
        {
            VideoId = entity.RemoteId,
            Title = entity.Title,
            Description = entity.Description ?? string.Empty,
            DescriptionHtml = entity.DescriptionHtml,
            Channel = channelInfo,
            PublishedAt = entity.PublishedAt.HasValue
                ? new DateTimeOffset(entity.PublishedAt.Value, TimeSpan.Zero)
                : null,
            Duration = TimeSpan.FromSeconds(entity.DurationSeconds),
            ViewCount = entity.ViewCount,
            LikeCount = entity.LikesCount ?? 0,
            DislikeCount = entity.DislikesCount,
            Keywords = ParseKeywords(entity.Keywords),
            Genre = entity.Genre,
            Thumbnails = entity.Thumbnails
                .Select(t => ImageEntityMapper.ToThumbnailInfo(t.Image))
                .ToList(),
            Captions = entity.Captions
                .Select(CaptionEntityMapper.ToContract)
                .ToList(),
            AdaptiveStreams = entity.Streams
                .Where(s => s.StreamTypeId == (int)StreamType.Video || s.StreamTypeId == (int)StreamType.Audio)
                .Select(StreamEntityMapper.ToContract)
                .ToList(),
            CombinedStreams = entity.Streams
                .Where(s => s.StreamTypeId == (int)StreamType.VideoAndAudio)
                .Select(StreamEntityMapper.ToContract)
                .ToList(),
            IsLive = entity.IsLive,
            IsUpcoming = entity.IsUpcoming
        };
    }

    /// <summary>
    /// Converts a VideoInfo contract to a VideoEntity for database storage.
    /// </summary>
    public static VideoEntity ToEntity(VideoInfo contract, int? channelId = null)
    {
        ArgumentNullException.ThrowIfNull(contract);

        return new VideoEntity
        {
            RemoteId = contract.VideoId,
            Title = contract.Title,
            Description = contract.Description,
            DescriptionHtml = contract.DescriptionHtml,
            DurationSeconds = (long)contract.Duration.TotalSeconds,
            ViewCount = contract.ViewCount,
            LikesCount = contract.LikeCount,
            DislikesCount = contract.DislikeCount,
            PublishedAt = contract.PublishedAt?.UtcDateTime,
            Genre = contract.Genre,
            Keywords = JoinKeywords(contract.Keywords),
            IsLive = contract.IsLive,
            IsUpcoming = contract.IsUpcoming,
            IsShort = false, // Not available in VideoInfo
            ChannelId = channelId,
            CreatedAt = DateTime.UtcNow,
            LastSyncedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates an existing entity with data from a contract.
    /// </summary>
    public static void UpdateEntity(VideoEntity entity, VideoInfo contract, int? channelId = null)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(contract);

        entity.Title = contract.Title;
        entity.Description = contract.Description;
        entity.DescriptionHtml = contract.DescriptionHtml;
        entity.DurationSeconds = (long)contract.Duration.TotalSeconds;
        entity.ViewCount = contract.ViewCount;
        entity.LikesCount = contract.LikeCount;
        entity.DislikesCount = contract.DislikeCount;
        entity.PublishedAt = contract.PublishedAt?.UtcDateTime;
        entity.Genre = contract.Genre;
        entity.Keywords = JoinKeywords(contract.Keywords);
        entity.IsLive = contract.IsLive;
        entity.IsUpcoming = contract.IsUpcoming;
        entity.LastSyncedAt = DateTime.UtcNow;

        if (channelId.HasValue)
        {
            entity.ChannelId = channelId.Value;
        }
    }

    /// <summary>
    /// Converts a VideoSummary to a VideoEntity (for channel video listings).
    /// </summary>
    public static VideoEntity ToEntity(VideoSummary contract, int? channelId = null)
    {
        ArgumentNullException.ThrowIfNull(contract);

        return new VideoEntity
        {
            RemoteId = contract.VideoId,
            Title = contract.Title,
            DurationSeconds = (long)contract.Duration.TotalSeconds,
            ViewCount = contract.ViewCount,
            PublishedAt = contract.PublishedAt?.UtcDateTime,
            IsLive = contract.IsLive,
            IsUpcoming = contract.IsUpcoming,
            IsShort = contract.IsShort,
            ChannelId = channelId,
            CreatedAt = DateTime.UtcNow,
            LastSyncedAt = DateTime.UtcNow
        };
    }

    private static IReadOnlyList<string> ParseKeywords(string? keywords)
    {
        if (string.IsNullOrWhiteSpace(keywords))
        {
            return [];
        }

        return keywords.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static string? JoinKeywords(IReadOnlyList<string>? keywords)
    {
        if (keywords is null || keywords.Count == 0)
        {
            return null;
        }

        return string.Join(",", keywords);
    }
}
