using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;
using DomainContracts = TMS.Apps.FrontTube.Backend.Repository.Data.Contracts;

namespace TMS.Apps.FrontTube.Backend.Repository.Data.Mappers;

public static class EntityDomainMapper
{
    public static DomainContracts.ChannelDomain ToDomain(ChannelEntity entity, DomainContracts.ChannelDomain? target = null)
    {
        ArgumentNullException.ThrowIfNull(entity);

        target ??= new DomainContracts.ChannelDomain();

        target.Id = entity.Id;
        target.CreatedAt = entity.CreatedAt;
        target.LastSyncedAt = entity.LastSyncedAt;
        target.Hash = entity.Hash;
        target.AbsoluteRemoteUrl = entity.AbsoluteRemoteUrl;
        target.Title = entity.Title;
        target.Alias = entity.Alias;
        target.Description = entity.Description;
        target.DescriptionHtml = entity.DescriptionHtml;
        target.Handle = entity.Handle;
        target.SubscriberCount = entity.SubscriberCount;
        target.VideoCount = entity.VideoCount;
        target.TotalViewCount = entity.TotalViewCount;
        target.JoinedAt = entity.JoinedAt;
        target.IsVerified = entity.IsVerified;
        target.Keywords = entity.Keywords;

        if (entity.Avatars.Count > 0)
        {
            var avatars = entity.Avatars
                .Where(a => a.Image is not null)
                .Select(a => ToDomain(a.Image!))
                .ToList();

            if (avatars.Count > 0)
            {
                target.Avatars = avatars;
            }
        }

        if (entity.Banners.Count > 0)
        {
            var banners = entity.Banners
                .Where(b => b.Image is not null)
                .Select(b => ToDomain(b.Image!))
                .ToList();

            if (banners.Count > 0)
            {
                target.Banners = banners;
            }
        }

        return target;
    }

    public static ChannelEntity FromDomain(DomainContracts.ChannelDomain domain, ChannelEntity? target = null)
    {
        ArgumentNullException.ThrowIfNull(domain);

        target ??= new ChannelEntity
        {
            Hash = domain.Hash,
            AbsoluteRemoteUrl = domain.AbsoluteRemoteUrl,
            Title = domain.Title
        };

        target.Id = domain.Id;
        target.CreatedAt = domain.CreatedAt;
        target.LastSyncedAt = domain.LastSyncedAt;
        target.Hash = domain.Hash;
        target.AbsoluteRemoteUrl = domain.AbsoluteRemoteUrl;
        target.Title = domain.Title;
        target.Alias = domain.Alias;
        target.Description = domain.Description;
        target.DescriptionHtml = domain.DescriptionHtml;
        target.Handle = domain.Handle;
        target.SubscriberCount = domain.SubscriberCount;
        target.VideoCount = domain.VideoCount;
        target.TotalViewCount = domain.TotalViewCount;
        target.JoinedAt = domain.JoinedAt;
        target.IsVerified = domain.IsVerified;
        target.Keywords = domain.Keywords;

        return target;
    }

    public static DomainContracts.VideoDomain ToDomain(VideoEntity entity, DomainContracts.VideoDomain? target = null)
    {
        ArgumentNullException.ThrowIfNull(entity);

        target ??= new DomainContracts.VideoDomain();

        target.Id = entity.Id;
        target.CreatedAt = entity.CreatedAt;
        target.LastSyncedAt = entity.LastSyncedAt;
        target.Hash = entity.Hash;
        target.AbsoluteRemoteUrl = entity.AbsoluteRemoteUrl;
        target.Title = entity.Title;
        target.Description = entity.Description;
        target.DescriptionHtml = entity.DescriptionHtml;
        target.DurationSeconds = entity.DurationSeconds;
        target.ViewCount = entity.ViewCount;
        target.LikesCount = entity.LikesCount;
        target.DislikesCount = entity.DislikesCount;
        target.PublishedAt = entity.PublishedAt;
        target.Genre = entity.Genre;
        target.Keywords = entity.Keywords;
        target.IsLive = entity.IsLive;
        target.IsUpcoming = entity.IsUpcoming;
        target.IsShort = entity.IsShort;
        target.IsWatched = entity.IsWatched;
        target.ChannelId = entity.ChannelId;
        if (entity.Channel is not null)
        {
            target.Channel = ToDomain(entity.Channel, target.Channel);
        }

        if (entity.Thumbnails.Count > 0)
        {
            var thumbnails = entity.Thumbnails
                .Where(t => t.Image is not null)
                .Select(t => ToDomain(t.Image!))
                .ToList();

            if (thumbnails.Count > 0)
            {
                target.Thumbnails = thumbnails;
            }
        }

        if (entity.Streams.Count > 0)
        {
            target.Streams = entity.Streams
                .Select(stream => ToDomain(stream))
                .ToList();
        }

        if (entity.Captions.Count > 0)
        {
            target.Captions = entity.Captions
                .Select(caption => ToDomain(caption))
                .ToList();
        }

        return target;
    }

    public static VideoEntity FromDomain(DomainContracts.VideoDomain domain, VideoEntity? target = null)
    {
        ArgumentNullException.ThrowIfNull(domain);

        target ??= new VideoEntity
        {
            Hash = domain.Hash,
            AbsoluteRemoteUrl = domain.AbsoluteRemoteUrl,
            Title = domain.Title
        };

        target.Id = domain.Id;
        target.CreatedAt = domain.CreatedAt;
        target.LastSyncedAt = domain.LastSyncedAt;
        target.Hash = domain.Hash;
        target.AbsoluteRemoteUrl = domain.AbsoluteRemoteUrl;
        target.Title = domain.Title;
        target.Description = domain.Description;
        target.DescriptionHtml = domain.DescriptionHtml;
        target.DurationSeconds = domain.DurationSeconds;
        target.ViewCount = domain.ViewCount;
        target.LikesCount = domain.LikesCount;
        target.DislikesCount = domain.DislikesCount;
        target.PublishedAt = domain.PublishedAt;
        target.Genre = domain.Genre;
        target.Keywords = domain.Keywords;
        target.IsLive = domain.IsLive;
        target.IsUpcoming = domain.IsUpcoming;
        target.IsShort = domain.IsShort;
        target.IsWatched = domain.IsWatched;
        target.ChannelId = domain.ChannelId;

        if (domain.Channel is not null)
        {
            target.Channel = FromDomain(domain.Channel);
        }

        return target;
    }

    public static DomainContracts.ImageDomain ToDomain(ImageEntity entity, DomainContracts.ImageDomain? target = null)
    {
        ArgumentNullException.ThrowIfNull(entity);

        target ??= new DomainContracts.ImageDomain();

        target.Id = entity.Id;
        target.CreatedAt = entity.CreatedAt;
        target.LastSyncedAt = entity.LastSyncedAt;
        target.Hash = entity.Hash;
        target.AbsoluteRemoteUrl = entity.AbsoluteRemoteUrl;
        target.Data = entity.Data;
        target.Width = entity.Width;
        target.Height = entity.Height;
        target.MimeType = entity.MimeType;

        return target;
    }

    public static ImageEntity FromDomain(DomainContracts.ImageDomain domain, ImageEntity? target = null)
    {
        ArgumentNullException.ThrowIfNull(domain);

        target ??= new ImageEntity
        {
            Hash = domain.Hash,
            AbsoluteRemoteUrl = domain.AbsoluteRemoteUrl
        };

        target.Id = domain.Id;
        target.CreatedAt = domain.CreatedAt;
        target.LastSyncedAt = domain.LastSyncedAt;
        target.Hash = domain.Hash;
        target.AbsoluteRemoteUrl = domain.AbsoluteRemoteUrl;
        target.Data = domain.Data;
        target.Width = domain.Width;
        target.Height = domain.Height;
        target.MimeType = domain.MimeType;

        return target;
    }

    public static DomainContracts.StreamDomain ToDomain(StreamEntity entity, DomainContracts.StreamDomain? target = null)
    {
        ArgumentNullException.ThrowIfNull(entity);

        target ??= new DomainContracts.StreamDomain();

        target.Id = entity.Id;
        target.CreatedAt = entity.CreatedAt;
        target.LastSyncedAt = entity.LastSyncedAt;
        target.Hash = entity.Hash;
        target.VideoId = entity.VideoId;
        target.AbsoluteRemoteUrl = entity.AbsoluteRemoteUrl;
        target.StreamTypeId = entity.StreamTypeId;
        target.ContainerId = entity.ContainerId;
        target.VideoCodecId = entity.VideoCodecId;
        target.AudioCodecId = entity.AudioCodecId;
        target.AudioQualityId = entity.AudioQualityId;
        target.ProjectionTypeId = entity.ProjectionTypeId;
        target.QualityLabel = entity.QualityLabel;
        target.Width = entity.Width;
        target.Height = entity.Height;
        target.FrameRate = entity.FrameRate;
        target.Bitrate = entity.Bitrate;
        target.ContentLength = entity.ContentLength;
        target.AudioSampleRate = entity.AudioSampleRate;
        target.AudioChannels = entity.AudioChannels;
        target.MimeType = entity.MimeType;
        target.Itag = entity.Itag;

        return target;
    }

    public static StreamEntity FromDomain(DomainContracts.StreamDomain domain, StreamEntity? target = null)
    {
        ArgumentNullException.ThrowIfNull(domain);

        target ??= new StreamEntity
        {
            Hash = domain.Hash,
            AbsoluteRemoteUrl = domain.AbsoluteRemoteUrl
        };

        target.Id = domain.Id;
        target.CreatedAt = domain.CreatedAt;
        target.LastSyncedAt = domain.LastSyncedAt;
        target.Hash = domain.Hash;
        target.VideoId = domain.VideoId;
        target.AbsoluteRemoteUrl = domain.AbsoluteRemoteUrl;
        target.StreamTypeId = domain.StreamTypeId;
        target.ContainerId = domain.ContainerId;
        target.VideoCodecId = domain.VideoCodecId;
        target.AudioCodecId = domain.AudioCodecId;
        target.AudioQualityId = domain.AudioQualityId;
        target.ProjectionTypeId = domain.ProjectionTypeId;
        target.QualityLabel = domain.QualityLabel;
        target.Width = domain.Width;
        target.Height = domain.Height;
        target.FrameRate = domain.FrameRate;
        target.Bitrate = domain.Bitrate;
        target.ContentLength = domain.ContentLength;
        target.AudioSampleRate = domain.AudioSampleRate;
        target.AudioChannels = domain.AudioChannels;
        target.MimeType = domain.MimeType;
        target.Itag = domain.Itag;

        if (domain.Video is not null)
        {
            target.Video = FromDomain(domain.Video);
        }

        return target;
    }

    public static DomainContracts.CaptionDomain ToDomain(CaptionEntity entity, DomainContracts.CaptionDomain? target = null)
    {
        ArgumentNullException.ThrowIfNull(entity);

        target ??= new DomainContracts.CaptionDomain();

        target.Id = entity.Id;
        target.CreatedAt = entity.CreatedAt;
        target.LastSyncedAt = entity.LastSyncedAt;
        target.Hash = entity.Hash;
        target.VideoId = entity.VideoId;
        target.Label = entity.Label;
        target.LanguageCode = entity.LanguageCode;
        target.IsAutoGenerated = entity.IsAutoGenerated;
        target.AbsoluteRemoteUrl = entity.AbsoluteRemoteUrl;
        target.Text = entity.Text;

        return target;
    }

    public static CaptionEntity FromDomain(DomainContracts.CaptionDomain domain, CaptionEntity? target = null)
    {
        ArgumentNullException.ThrowIfNull(domain);

        target ??= new CaptionEntity
        {
            Hash = domain.Hash,
            AbsoluteRemoteUrl = domain.AbsoluteRemoteUrl,
            Label = domain.Label,
            LanguageCode = domain.LanguageCode
        };

        target.Id = domain.Id;
        target.CreatedAt = domain.CreatedAt;
        target.LastSyncedAt = domain.LastSyncedAt;
        target.Hash = domain.Hash;
        target.VideoId = domain.VideoId;
        target.Label = domain.Label;
        target.LanguageCode = domain.LanguageCode;
        target.IsAutoGenerated = domain.IsAutoGenerated;
        target.AbsoluteRemoteUrl = domain.AbsoluteRemoteUrl;
        target.Text = domain.Text;

        if (domain.Video is not null)
        {
            target.Video = FromDomain(domain.Video);
        }

        return target;
    }

}
