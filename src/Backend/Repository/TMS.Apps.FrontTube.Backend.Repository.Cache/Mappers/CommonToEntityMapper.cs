using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Cache;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;

namespace TMS.Apps.FrontTube.Backend.Repository.Cache.Mappers;

public static class CommonToEntityMapper
{
    private static string? JoinKeywords(IEnumerable<string>? keywords)
    {
        if (keywords is null) return null;
        var list = keywords as IReadOnlyCollection<string> ?? keywords.ToList();
        return list.Count == 0 ? null : string.Join(',', list);
    }

    // Video (full)
    public static VideoEntity ToEntity(VideoCommon common, VideoEntity? targetEntity = null)
    {
        ArgumentNullException.ThrowIfNull(common);

        var absoluteRemoteUrl = common.AbsoluteRemoteUrl.ToString();
        var hash = HashHelper.ComputeHash(absoluteRemoteUrl);

        if (targetEntity is not null)
        {
            targetEntity.Hash = hash;
            targetEntity.AbsoluteRemoteUrl = absoluteRemoteUrl;
            targetEntity.Title = common.Title;
            targetEntity.Description = common.DescriptionText;
            targetEntity.DescriptionHtml = common.DescriptionHtml;
            targetEntity.DurationSeconds = (long)common.Duration.TotalSeconds;
            targetEntity.ViewCount = common.ViewCount;
            targetEntity.LikesCount = common.LikeCount;
            targetEntity.DislikesCount = common.DislikeCount;
            targetEntity.PublishedAt = common.PublishedAtUtc?.UtcDateTime;
            targetEntity.Genre = common.Genre;
            targetEntity.Keywords = JoinKeywords(common.Tags);
            targetEntity.IsLive = common.IsLive;
            targetEntity.IsUpcoming = common.IsUpcoming;
            targetEntity.LastSyncedAt = DateTime.UtcNow;

            return targetEntity;
        }

        return new VideoEntity
        {
            Hash = hash,
            AbsoluteRemoteUrl = absoluteRemoteUrl,
            Title = common.Title,
            Description = common.DescriptionText,
            DescriptionHtml = common.DescriptionHtml,
            DurationSeconds = (long)common.Duration.TotalSeconds,
            ViewCount = common.ViewCount,
            LikesCount = common.LikeCount,
            DislikesCount = common.DislikeCount,
            PublishedAt = common.PublishedAtUtc?.UtcDateTime,
            Genre = common.Genre,
            Keywords = JoinKeywords(common.Tags),
            IsLive = common.IsLive,
            IsUpcoming = common.IsUpcoming,
            CreatedAt = DateTime.UtcNow,
            LastSyncedAt = DateTime.UtcNow
        };
    }

    // Video metadata (summary)
    public static VideoEntity ToEntity(VideoMetadataCommon common, VideoEntity? targetEntity = null)
    {
        ArgumentNullException.ThrowIfNull(common);

        var absoluteRemoteUrl = common.AbsoluteRemoteUrl.ToString();
        var hash = HashHelper.ComputeHash(absoluteRemoteUrl);

        if (targetEntity is not null)
        {
            targetEntity.Hash = hash;
            targetEntity.AbsoluteRemoteUrl = absoluteRemoteUrl;
            targetEntity.Title = common.Title;
            targetEntity.DurationSeconds = (long)common.Duration.TotalSeconds;
            targetEntity.ViewCount = common.ViewCount;
            targetEntity.PublishedAt = common.PublishedAtUtc?.UtcDateTime;
            targetEntity.IsLive = common.IsLive;
            targetEntity.IsUpcoming = common.IsUpcoming;
            targetEntity.IsShort = common.IsShort;
            targetEntity.LastSyncedAt = DateTime.UtcNow;

            return targetEntity;
        }

        return new VideoEntity
        {
            Hash = hash,
            AbsoluteRemoteUrl = absoluteRemoteUrl,
            Title = common.Title,
            DurationSeconds = (long)common.Duration.TotalSeconds,
            ViewCount = common.ViewCount,
            PublishedAt = common.PublishedAtUtc?.UtcDateTime,
            IsLive = common.IsLive,
            IsUpcoming = common.IsUpcoming,
            IsShort = common.IsShort,
            CreatedAt = DateTime.UtcNow,
            LastSyncedAt = DateTime.UtcNow
        };
    }

    // Channel (full)
    public static ChannelEntity ToEntity(ChannelCommon common, ChannelEntity? targetEntity = null)
    {
        ArgumentNullException.ThrowIfNull(common);

        var absoluteRemoteUrl = common.AbsoluteRemoteUrl.ToString();
        var hash = HashHelper.ComputeHash(absoluteRemoteUrl);

        if (targetEntity is not null)
        {
            targetEntity.Hash = hash;
            targetEntity.AbsoluteRemoteUrl = absoluteRemoteUrl;
            targetEntity.Title = common.Name;
            targetEntity.Description = common.Description;
            targetEntity.DescriptionHtml = common.DescriptionHtml;
            targetEntity.SubscriberCount = common.SubscriberCount;
            targetEntity.VideoCount = common.VideoCount;
            targetEntity.TotalViewCount = common.TotalViewCount;
            targetEntity.JoinedAt = common.JoinedAt?.UtcDateTime;
            targetEntity.IsVerified = common.IsVerified;
            targetEntity.Keywords = JoinKeywords(common.Tags);
            targetEntity.LastSyncedAt = DateTime.UtcNow;

            return targetEntity;
        }

        return new ChannelEntity
        {
            Hash = hash,
            AbsoluteRemoteUrl = absoluteRemoteUrl,
            Title = common.Name,
            Description = common.Description,
            DescriptionHtml = common.DescriptionHtml,
            SubscriberCount = common.SubscriberCount,
            VideoCount = common.VideoCount,
            TotalViewCount = common.TotalViewCount,
            JoinedAt = common.JoinedAt?.UtcDateTime,
            IsVerified = common.IsVerified,
            Keywords = JoinKeywords(common.Tags),
            CreatedAt = DateTime.UtcNow,
            LastSyncedAt = DateTime.UtcNow
        };
    }

    // Channel metadata
    public static ChannelEntity ToEntity(ChannelMetadataCommon common, ChannelEntity? targetEntity = null)
    {
        ArgumentNullException.ThrowIfNull(common);

        var absoluteRemoteUrl = common.AbsoluteRemoteUrl.ToString();
        var hash = HashHelper.ComputeHash(absoluteRemoteUrl);

        if (targetEntity is not null)
        {
            targetEntity.Hash = hash;
            targetEntity.AbsoluteRemoteUrl = absoluteRemoteUrl;
            targetEntity.Title = common.Name;
            targetEntity.LastSyncedAt = DateTime.UtcNow;

            return targetEntity;
        }

        return new ChannelEntity
        {
            Hash = hash,
            AbsoluteRemoteUrl = absoluteRemoteUrl,
            Title = common.Name,
            CreatedAt = DateTime.UtcNow,
            LastSyncedAt = DateTime.UtcNow
        };
    }

    // Image metadata
    public static ImageEntity ToEntity(ImageMetadataCommon common, ImageEntity? targetEntity = null)
    {
        ArgumentNullException.ThrowIfNull(common);

        var absoluteRemoteUrl = common.AbsoluteRemoteUrl.ToString();
        var hash = HashHelper.ComputeHash(absoluteRemoteUrl);

        if (targetEntity is not null)
        {
            targetEntity.Hash = hash;
            targetEntity.AbsoluteRemoteUrl = absoluteRemoteUrl;
            targetEntity.Width = common.Width;
            targetEntity.Height = common.Height;
            targetEntity.LastSyncedAt = DateTime.UtcNow;

            return targetEntity;
        }

        return new ImageEntity
        {
            Hash = hash,
            AbsoluteRemoteUrl = absoluteRemoteUrl,
            Width = common.Width,
            Height = common.Height,
            CreatedAt = DateTime.UtcNow,
            LastSyncedAt = null // data not downloaded yet
        };
    }

    // Caption metadata
    public static CaptionEntity ToEntity(CaptionMetadataCommon common, CaptionEntity? targetEntity = null)
    {
        ArgumentNullException.ThrowIfNull(common);

        var absoluteRemoteUrl = common.AbsoluteRemoteUrl.ToString();
        var hash = HashHelper.ComputeHash(absoluteRemoteUrl);

        if (targetEntity is not null)
        {
            targetEntity.Hash = hash;
            targetEntity.AbsoluteRemoteUrl = absoluteRemoteUrl;
            targetEntity.Label = common.Name;
            targetEntity.LanguageCode = common.LanguageCode;
            targetEntity.IsAutoGenerated = common.IsAutoGenerated;
            targetEntity.LastSyncedAt = DateTime.UtcNow;

            return targetEntity;
        }

        return new CaptionEntity
        {
            Hash = hash,
            AbsoluteRemoteUrl = absoluteRemoteUrl,
            Label = common.Name,
            LanguageCode = common.LanguageCode,
            IsAutoGenerated = common.IsAutoGenerated,
            CreatedAt = DateTime.UtcNow,
            LastSyncedAt = null // data not dowloaded yet
        };
    }

    // Stream metadata (basic mapping)
    public static StreamEntity ToEntity(StreamMetadataCommon common, StreamEntity? targetEntity = null)
    {
        ArgumentNullException.ThrowIfNull(common);

        var absoluteRemoteUrl = common.AbsoluteRemoteUrl.ToString();
        var hash = HashHelper.ComputeHash(absoluteRemoteUrl);

        if (targetEntity is not null)
        {
            targetEntity.Hash = hash;
            targetEntity.AbsoluteRemoteUrl = absoluteRemoteUrl;
            targetEntity.Bitrate = common.Bitrate;
            targetEntity.MimeType = common.MimeType;
            targetEntity.Itag = common.Itag;
            targetEntity.ContentLength = common.ContentLength;
            targetEntity.Width = common.Width;
            targetEntity.Height = common.Height;
            targetEntity.FrameRate = common.FrameRate;
            targetEntity.StreamTypeId = (int)common.Type;
            targetEntity.ContainerId = (int)common.Container;
            targetEntity.VideoCodecId = common.VideoCodec is null ? null : (int?) (int) common.VideoCodec;
            targetEntity.AudioCodecId = common.AudioCodec is null ? null : (int?) (int) common.AudioCodec;
            targetEntity.AudioSampleRate = common.AudioSampleRate;
            targetEntity.AudioChannels = common.AudioChannels;
            targetEntity.ProjectionTypeId = (int)common.Projection;
            targetEntity.QualityLabel = common.QualityLabel;
            targetEntity.LastSyncedAt = DateTime.UtcNow;

            return targetEntity;
        }

        return new StreamEntity
        {
            Hash = hash,
            AbsoluteRemoteUrl = absoluteRemoteUrl,
            Bitrate = common.Bitrate,
            MimeType = common.MimeType,
            Itag = common.Itag,
            ContentLength = common.ContentLength,
            Width = common.Width,
            Height = common.Height,
            FrameRate = common.FrameRate,
            StreamTypeId = (int)common.Type,
            ContainerId = (int)common.Container,
            VideoCodecId = common.VideoCodec is null ? null : (int?) (int) common.VideoCodec,
            AudioCodecId = common.AudioCodec is null ? null : (int?) (int) common.AudioCodec,
            AudioSampleRate = common.AudioSampleRate,
            AudioChannels = common.AudioChannels,
            ProjectionTypeId = (int)common.Projection,
            QualityLabel = common.QualityLabel,
            CreatedAt = DateTime.UtcNow,
            LastSyncedAt = DateTime.UtcNow
        };
    }
}
