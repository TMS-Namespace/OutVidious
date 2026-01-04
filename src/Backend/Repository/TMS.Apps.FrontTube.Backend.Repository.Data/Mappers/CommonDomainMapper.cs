using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using CommonConfig = TMS.Apps.FrontTube.Backend.Common.ProviderCore.Configuration;
using CommonContracts = TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using DomainConfig = TMS.Apps.FrontTube.Backend.Repository.Data.Contracts.Configuration;
using DomainContracts = TMS.Apps.FrontTube.Backend.Repository.Data.Contracts;

namespace TMS.Apps.FrontTube.Backend.Repository.Data.Mappers;

public static class CommonDomainMapper
{
    private static string? JoinKeywords(IEnumerable<string>? keywords)
    {
        if (keywords is null)
        {
            return null;
        }

        var list = keywords as IReadOnlyCollection<string> ?? keywords.ToList();
        return list.Count == 0 ? null : string.Join(',', list);
    }

    private static IReadOnlyList<string> SplitKeywords(string? keywords)
    {
        if (string.IsNullOrWhiteSpace(keywords))
        {
            return [];
        }

        return keywords
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    private static TEnum ToEnum<TEnum>(int value, TEnum fallback)
        where TEnum : struct, Enum
    {
        return Enum.IsDefined(typeof(TEnum), value) ? (TEnum)(object)value : fallback;
    }

    private static DomainContracts.VideoDomain MapVideoBase(VideoBaseCommon video, DomainContracts.VideoDomain? target = null)
    {
        ArgumentNullException.ThrowIfNull(video);

        target ??= new DomainContracts.VideoDomain();

        target.Hash = video.Hash;
        target.AbsoluteRemoteUrl = video.AbsoluteRemoteUrl.ToString();
        target.Title = video.Title;
        target.DurationSeconds = (long)video.Duration.TotalSeconds;
        target.ViewCount = video.ViewCount;
        target.PublishedAt = video.PublishedAtUtc?.UtcDateTime;
        target.IsLive = video.IsLive;
        target.IsUpcoming = video.IsUpcoming;
        target.Channel = ToDomain(video.Channel);
        target.Thumbnails = video.Thumbnails
            .Select((CommonContracts.ImageMetadataCommon thumbnail) => ToDomain(thumbnail))
            .ToList();
        target.LastSyncedAt = DateTime.UtcNow;

        return target;
    }

    public static DomainContracts.IdentityDomain ToDomain(CommonContracts.CacheableIdentity identity, DomainContracts.IdentityDomain? target = null)
    {
        ArgumentNullException.ThrowIfNull(identity);

        target ??= new DomainContracts.IdentityDomain
        {
            AbsoluteRemoteUrlString = identity.AbsoluteRemoteUrlString
        };

        target.AbsoluteRemoteUrlString = identity.AbsoluteRemoteUrlString;
        target.RemoteId = identity.RemoteId;
        target.DataBaseId = identity.DataBaseId;

        return target;
    }

    public static CommonContracts.CacheableIdentity FromDomain(DomainContracts.IdentityDomain domain, CommonContracts.CacheableIdentity? target = null)
    {
        ArgumentNullException.ThrowIfNull(domain);

        if (target is null)
        {
            return new CommonContracts.CacheableIdentity
            {
                AbsoluteRemoteUrlString = domain.AbsoluteRemoteUrlString,
                RemoteId = domain.RemoteId,
                DataBaseId = domain.DataBaseId
            };
        }

        return target with
        {
            AbsoluteRemoteUrlString = domain.AbsoluteRemoteUrlString,
            RemoteId = domain.RemoteId,
            DataBaseId = domain.DataBaseId
        };
    }

    public static DomainContracts.ChannelDomain ToDomain(CommonContracts.ChannelMetadataCommon channel, DomainContracts.ChannelDomain? target = null)
    {
        ArgumentNullException.ThrowIfNull(channel);

        target ??= new DomainContracts.ChannelDomain();

        target.Hash = channel.Hash;
        target.AbsoluteRemoteUrl = channel.AbsoluteRemoteUrl.ToString();
        target.Title = channel.Name;
        target.SubscriberCount = channel.SubscriberCount;
        target.Avatars = channel.Avatars
            .Select((CommonContracts.ImageMetadataCommon avatar) => ToDomain(avatar))
            .ToList();

        target.LastSyncedAt = DateTime.UtcNow;

        return target;
    }

    public static DomainContracts.ChannelDomain ToDomain(ChannelCommon channel, DomainContracts.ChannelDomain? target = null)
    {
        ArgumentNullException.ThrowIfNull(channel);

        target = ToDomain((CommonContracts.ChannelMetadataCommon)channel, target);

        target.Description = channel.Description;
        target.DescriptionHtml = channel.DescriptionHtml;
        target.VideoCount = channel.VideoCount;
        target.TotalViewCount = channel.TotalViewCount;
        target.JoinedAt = channel.JoinedAt?.UtcDateTime;
        target.IsVerified = channel.IsVerified;
        target.Keywords = JoinKeywords(channel.Tags);
        target.Banners = channel.Banners
            .Select((CommonContracts.ImageMetadataCommon banner) => ToDomain(banner))
            .ToList();
        target.AvailableTabs = channel.AvailableTabs
#pragma warning disable CS0618
            .Select(t => t.RemoteTabId)
#pragma warning restore CS0618
            .ToList();

        return target;
    }

    public static CommonContracts.ChannelMetadataCommon FromDomain(DomainContracts.ChannelDomain domain, CommonContracts.ChannelMetadataCommon? target = null)
    {
        ArgumentNullException.ThrowIfNull(domain);

        var avatars = domain.Avatars
            .Select((DomainContracts.ImageDomain avatar) => FromDomain(avatar))
            .ToList();

        if (target is null)
        {
            return new CommonContracts.ChannelMetadataCommon
            {
                AbsoluteRemoteUrl = new Uri(domain.AbsoluteRemoteUrl),
                Name = domain.Title,
                SubscriberCount = domain.SubscriberCount,
                Avatars = avatars
            };
        }

        return target with
        {
            AbsoluteRemoteUrl = new Uri(domain.AbsoluteRemoteUrl),
            Name = domain.Title,
            SubscriberCount = domain.SubscriberCount,
            Avatars = avatars
        };
    }

    public static ChannelCommon FromDomain(DomainContracts.ChannelDomain domain, ChannelCommon? target = null)
    {
        ArgumentNullException.ThrowIfNull(domain);

        var avatars = domain.Avatars
            .Select((DomainContracts.ImageDomain avatar) => FromDomain(avatar))
            .ToList();

        var banners = domain.Banners
            .Select((DomainContracts.ImageDomain banner) => FromDomain(banner))
            .ToList();

        var tabs = domain.AvailableTabs
            .Select(t => new CommonContracts.ChannelTab
            {
#pragma warning disable CS0618
                RemoteTabId = t,
#pragma warning restore CS0618
                Name = t,
                IsAvailable = true
            })
            .ToList();

        if (target is null)
        {
            return new ChannelCommon
            {
                AbsoluteRemoteUrl = new Uri(domain.AbsoluteRemoteUrl),
                Name = domain.Title,
                SubscriberCount = domain.SubscriberCount,
                Avatars = avatars,
                Description = domain.Description ?? string.Empty,
                DescriptionHtml = domain.DescriptionHtml,
                VideoCount = domain.VideoCount,
                TotalViewCount = domain.TotalViewCount,
                JoinedAt = domain.JoinedAt?.ToUniversalTime(),
                IsVerified = domain.IsVerified,
                Tags = SplitKeywords(domain.Keywords),
                Banners = banners,
                AvailableTabs = tabs
            };
        }

        return target with
        {
            AbsoluteRemoteUrl = new Uri(domain.AbsoluteRemoteUrl),
            Name = domain.Title,
            SubscriberCount = domain.SubscriberCount,
            Avatars = avatars,
            Description = domain.Description ?? string.Empty,
            DescriptionHtml = domain.DescriptionHtml,
            VideoCount = domain.VideoCount,
            TotalViewCount = domain.TotalViewCount,
            JoinedAt = domain.JoinedAt?.ToUniversalTime(),
            IsVerified = domain.IsVerified,
            Tags = SplitKeywords(domain.Keywords),
            Banners = banners,
            AvailableTabs = tabs
        };
    }

    public static DomainContracts.ImageDomain ToDomain(CommonContracts.ImageMetadataCommon image, DomainContracts.ImageDomain? target = null)
    {
        ArgumentNullException.ThrowIfNull(image);

        target ??= new DomainContracts.ImageDomain();

        target.Hash = image.Hash;
        target.AbsoluteRemoteUrl = image.AbsoluteRemoteUrl.ToString();
        target.Width = image.Width;
        target.Height = image.Height;
        target.LastSyncedAt = DateTime.UtcNow;

        return target;
    }

    public static CommonContracts.ImageMetadataCommon FromDomain(DomainContracts.ImageDomain domain, CommonContracts.ImageMetadataCommon? target = null)
    {
        ArgumentNullException.ThrowIfNull(domain);

        if (target is null)
        {
            return new CommonContracts.ImageMetadataCommon
            {
                Quality = ImageQuality.Unknown,
                AbsoluteRemoteUrl = new Uri(domain.AbsoluteRemoteUrl),
                Width = domain.Width ?? 0,
                Height = domain.Height ?? 0
            };
        }

        return target with
        {
            Quality = ImageQuality.Unknown,
            AbsoluteRemoteUrl = new Uri(domain.AbsoluteRemoteUrl),
            Width = domain.Width ?? 0,
            Height = domain.Height ?? 0
        };
    }

    public static DomainContracts.CaptionDomain ToDomain(CommonContracts.CaptionMetadataCommon caption, DomainContracts.CaptionDomain? target = null)
    {
        ArgumentNullException.ThrowIfNull(caption);

        target ??= new DomainContracts.CaptionDomain();

        target.Hash = caption.Hash;
        target.AbsoluteRemoteUrl = caption.AbsoluteRemoteUrl.ToString();
        target.Label = caption.Name;
        target.LanguageCode = caption.LanguageCode;
        target.IsAutoGenerated = caption.IsAutoGenerated;
        target.LastSyncedAt = DateTime.UtcNow;
        if (target.CreatedAt == default)
        {
            target.CreatedAt = DateTime.UtcNow;
        }

        return target;
    }

    public static CommonContracts.CaptionMetadataCommon FromDomain(DomainContracts.CaptionDomain domain, CommonContracts.CaptionMetadataCommon? target = null)
    {
        ArgumentNullException.ThrowIfNull(domain);

        if (target is null)
        {
            return new CommonContracts.CaptionMetadataCommon
            {
                Name = domain.Label,
                LanguageCode = domain.LanguageCode,
                AbsoluteRemoteUrl = new Uri(domain.AbsoluteRemoteUrl),
                IsAutoGenerated = domain.IsAutoGenerated
            };
        }

        return target with
        {
            Name = domain.Label,
            LanguageCode = domain.LanguageCode,
            AbsoluteRemoteUrl = new Uri(domain.AbsoluteRemoteUrl),
            IsAutoGenerated = domain.IsAutoGenerated
        };
    }

    public static DomainContracts.StreamDomain ToDomain(CommonContracts.StreamMetadataCommon stream, DomainContracts.StreamDomain? target = null)
    {
        ArgumentNullException.ThrowIfNull(stream);

        target ??= new DomainContracts.StreamDomain();

        target.Hash = stream.Hash;
        target.AbsoluteRemoteUrl = stream.AbsoluteRemoteUrl.ToString();
        target.StreamTypeId = (int)stream.Type;
        target.ContainerId = (int)stream.Container;
        target.VideoCodecId = stream.VideoCodec is null ? null : (int?)stream.VideoCodec;
        target.AudioCodecId = stream.AudioCodec is null ? null : (int?)stream.AudioCodec;
        target.AudioQualityId = stream.AudioQualityLevel is null ? null : (int?)stream.AudioQualityLevel;
        target.ProjectionTypeId = (int)stream.Projection;
        target.QualityLabel = stream.QualityLabel;
        target.Width = stream.Width;
        target.Height = stream.Height;
        target.FrameRate = stream.FrameRate;
        target.Bitrate = stream.Bitrate;
        target.ContentLength = stream.ContentLength;
        target.AudioSampleRate = stream.AudioSampleRate;
        target.AudioChannels = stream.AudioChannels;
        target.MimeType = stream.MimeType;
        target.Itag = stream.Itag;
        target.LastSyncedAt = DateTime.UtcNow;

        if (target.CreatedAt == default)
        {
            target.CreatedAt = DateTime.UtcNow;
        }

        return target;
    }

    public static CommonContracts.StreamMetadataCommon FromDomain(DomainContracts.StreamDomain domain, CommonContracts.StreamMetadataCommon? target = null)
    {
        ArgumentNullException.ThrowIfNull(domain);

        var type = ToEnum(domain.StreamTypeId, StreamType.Unknown);
        var container = ToEnum(domain.ContainerId, VideoContainer.Unknown);
        var projection = ToEnum(domain.ProjectionTypeId, ProjectionType.Unknown);

        var videoCodec = domain.VideoCodecId is null
            ? (VideoCodec?)null
            : ToEnum(domain.VideoCodecId.Value, VideoCodec.Unknown);

        var audioCodec = domain.AudioCodecId is null
            ? (AudioCodec?)null
            : ToEnum(domain.AudioCodecId.Value, AudioCodec.Unknown);

        var audioQuality = domain.AudioQualityId is null
            ? (AudioQuality?)null
            : ToEnum(domain.AudioQualityId.Value, AudioQuality.Unknown);

        if (target is null)
        {
            return new CommonContracts.StreamMetadataCommon
            {
                Type = type,
                AbsoluteRemoteUrl = new Uri(domain.AbsoluteRemoteUrl),
                Container = container,
                VideoCodec = videoCodec,
                AudioCodec = audioCodec,
                QualityLabel = domain.QualityLabel,
                Width = domain.Width,
                Height = domain.Height,
                FrameRate = domain.FrameRate,
                Bitrate = domain.Bitrate,
                ContentLength = domain.ContentLength,
                AudioSampleRate = domain.AudioSampleRate,
                AudioChannels = domain.AudioChannels,
                AudioQualityLevel = audioQuality,
                Projection = projection,
                MimeType = domain.MimeType,
                Itag = domain.Itag
            };
        }

        return target with
        {
            Type = type,
            AbsoluteRemoteUrl = new Uri(domain.AbsoluteRemoteUrl),
            Container = container,
            VideoCodec = videoCodec,
            AudioCodec = audioCodec,
            QualityLabel = domain.QualityLabel,
            Width = domain.Width,
            Height = domain.Height,
            FrameRate = domain.FrameRate,
            Bitrate = domain.Bitrate,
            ContentLength = domain.ContentLength,
            AudioSampleRate = domain.AudioSampleRate,
            AudioChannels = domain.AudioChannels,
            AudioQualityLevel = audioQuality,
            Projection = projection,
            MimeType = domain.MimeType,
            Itag = domain.Itag
        };
    }

    public static DomainContracts.VideoDomain ToDomain(CommonContracts.VideoMetadataCommon video, DomainContracts.VideoDomain? target = null)
    {
        ArgumentNullException.ThrowIfNull(video);

        target = MapVideoBase((VideoBaseCommon)video, target);
        target.IsShort = video.IsShort;

        return target;
    }

    public static DomainContracts.VideoDomain ToDomain(VideoCommon video, DomainContracts.VideoDomain? target = null)
    {
        ArgumentNullException.ThrowIfNull(video);

        target = MapVideoBase(video, target);

        target.Description = video.DescriptionText;
        target.DescriptionHtml = video.DescriptionHtml;
        target.LikesCount = video.LikeCount;
        target.DislikesCount = video.DislikeCount;
        target.Genre = video.Genre;
        target.Keywords = JoinKeywords(video.Tags);

        var streams = video.AdaptiveStreams
            .Concat(video.MutexStreams)
            .Select((CommonContracts.StreamMetadataCommon stream) => ToDomain(stream))
            .ToList();

        target.Streams = streams;
        target.Captions = video.Captions
            .Select((CommonContracts.CaptionMetadataCommon caption) => ToDomain(caption))
            .ToList();

        return target;
    }

    public static CommonContracts.VideoMetadataCommon FromDomain(DomainContracts.VideoDomain domain, CommonContracts.VideoMetadataCommon? target = null)
    {
        ArgumentNullException.ThrowIfNull(domain);

        if (domain.Channel is null)
        {
            throw new ArgumentException("VideoDomain.Channel is required to map to VideoMetadataCommon.", nameof(domain));
        }

        var thumbnails = domain.Thumbnails
            .Select((DomainContracts.ImageDomain thumbnail) => FromDomain(thumbnail))
            .ToList();

        if (target is null)
        {
            return new CommonContracts.VideoMetadataCommon
            {
                AbsoluteRemoteUrl = new Uri(domain.AbsoluteRemoteUrl),
                Title = domain.Title,
                Duration = TimeSpan.FromSeconds(domain.DurationSeconds),
                ViewCount = domain.ViewCount,
                PublishedAtUtc = domain.PublishedAt?.ToUniversalTime(),
                Channel = FromDomain(domain.Channel, (CommonContracts.ChannelMetadataCommon?)null),
                Thumbnails = thumbnails,
                IsLive = domain.IsLive,
                IsUpcoming = domain.IsUpcoming,
                IsShort = domain.IsShort
            };
        }

        return target with
        {
            AbsoluteRemoteUrl = new Uri(domain.AbsoluteRemoteUrl),
            Title = domain.Title,
            Duration = TimeSpan.FromSeconds(domain.DurationSeconds),
            ViewCount = domain.ViewCount,
            PublishedAtUtc = domain.PublishedAt?.ToUniversalTime(),
            Channel = FromDomain(domain.Channel, (CommonContracts.ChannelMetadataCommon?)null),
            Thumbnails = thumbnails,
            IsLive = domain.IsLive,
            IsUpcoming = domain.IsUpcoming,
            IsShort = domain.IsShort
        };
    }

    public static VideoCommon FromDomain(DomainContracts.VideoDomain domain, VideoCommon? target = null)
    {
        ArgumentNullException.ThrowIfNull(domain);

        if (domain.Channel is null)
        {
            throw new ArgumentException("VideoDomain.Channel is required to map to Video.", nameof(domain));
        }

        var thumbnails = domain.Thumbnails
            .Select((DomainContracts.ImageDomain thumbnail) => FromDomain(thumbnail))
            .ToList();

        var streams = domain.Streams
            .Select((DomainContracts.StreamDomain stream) => FromDomain(stream))
            .ToList();

        var adaptiveStreams = streams
            .Where(s => s.Type is StreamType.Video or StreamType.Audio)
            .ToList();

        var mutexStreams = streams
            .Where(s => s.Type == StreamType.Mutex)
            .ToList();

        var captions = domain.Captions
            .Select((DomainContracts.CaptionDomain caption) => FromDomain(caption))
            .ToList();

        if (target is null)
        {
            return new VideoCommon
            {
                AbsoluteRemoteUrl = new Uri(domain.AbsoluteRemoteUrl),
                Title = domain.Title,
                Duration = TimeSpan.FromSeconds(domain.DurationSeconds),
                ViewCount = domain.ViewCount,
                PublishedAtUtc = domain.PublishedAt?.ToUniversalTime(),
                Channel = FromDomain(domain.Channel, (CommonContracts.ChannelMetadataCommon?)null),
                Thumbnails = thumbnails,
                IsLive = domain.IsLive,
                IsUpcoming = domain.IsUpcoming,
                DescriptionText = domain.Description ?? string.Empty,
                DescriptionHtml = domain.DescriptionHtml,
                LikeCount = domain.LikesCount ?? 0,
                DislikeCount = domain.DislikesCount,
                Tags = SplitKeywords(domain.Keywords),
                Genre = domain.Genre,
                AdaptiveStreams = adaptiveStreams,
                MutexStreams = mutexStreams,
                Captions = captions,
                AllowedRegions = [],
                IsFamilyFriendly = false,
                IsListed = false,
                AllowRatings = false,
                IsPremium = false
            };
        }

        return target with
        {
            AbsoluteRemoteUrl = new Uri(domain.AbsoluteRemoteUrl),
            Title = domain.Title,
            Duration = TimeSpan.FromSeconds(domain.DurationSeconds),
            ViewCount = domain.ViewCount,
            PublishedAtUtc = domain.PublishedAt?.ToUniversalTime(),
            Channel = FromDomain(domain.Channel, (CommonContracts.ChannelMetadataCommon?)null),
            Thumbnails = thumbnails,
            IsLive = domain.IsLive,
            IsUpcoming = domain.IsUpcoming,
            DescriptionText = domain.Description ?? string.Empty,
            DescriptionHtml = domain.DescriptionHtml,
            LikeCount = domain.LikesCount ?? 0,
            DislikeCount = domain.DislikesCount,
            Tags = SplitKeywords(domain.Keywords),
            Genre = domain.Genre,
            AdaptiveStreams = adaptiveStreams,
            MutexStreams = mutexStreams,
            Captions = captions,
            AllowedRegions = [],
            IsFamilyFriendly = false,
            IsListed = false,
            AllowRatings = false,
            IsPremium = false
        };
    }

    public static DomainContracts.VideosPageDomain ToDomain(VideosPageCommon page, DomainContracts.VideosPageDomain? target = null)
    {
        ArgumentNullException.ThrowIfNull(page);

        target ??= new DomainContracts.VideosPageDomain
        {
            ChannelAbsoluteRemoteUrl = page.ChannelRemoteAbsoluteUrl
        };

        target.ChannelAbsoluteRemoteUrl = page.ChannelRemoteAbsoluteUrl;
        target.Tab = page.Tab;
        target.ContinuationToken = page.ContinuationToken;
        target.TotalVideoCount = page.TotalVideoCount;
        target.PageNumber = page.PageNumber;
        target.Videos = page.Videos
            .Select((CommonContracts.VideoMetadataCommon video) => ToDomain(video))
            .ToList();

        return target;
    }

    public static VideosPageCommon FromDomain(DomainContracts.VideosPageDomain domain, VideosPageCommon? target = null)
    {
        ArgumentNullException.ThrowIfNull(domain);

        var videos = domain.Videos
            .Select(video => FromDomain(video, (CommonContracts.VideoMetadataCommon?)null))
            .ToList();

        if (target is null)
        {
            return new VideosPageCommon
            {
                ChannelRemoteAbsoluteUrl = domain.ChannelAbsoluteRemoteUrl,
                Tab = domain.Tab,
                Videos = videos,
                ContinuationToken = domain.ContinuationToken,
                TotalVideoCount = domain.TotalVideoCount,
                PageNumber = domain.PageNumber
            };
        }

        return target with
        {
            ChannelRemoteAbsoluteUrl = domain.ChannelAbsoluteRemoteUrl,
            Tab = domain.Tab,
            Videos = videos,
            ContinuationToken = domain.ContinuationToken,
            TotalVideoCount = domain.TotalVideoCount,
            PageNumber = domain.PageNumber
        };
    }

    public static DomainConfig.CacheConfig ToDomain(CommonConfig.CacheConfig config, DomainConfig.CacheConfig? target = null)
    {
        ArgumentNullException.ThrowIfNull(config);

        target ??= new DomainConfig.CacheConfig();

        return target with
        {
            StalenessConfigs = ToDomain(config.StalenessConfigs),
            SecondLevelCache = ToDomain(config.SecondLevelCache)
        };
    }

    public static CommonConfig.CacheConfig FromDomain(DomainConfig.CacheConfig config, CommonConfig.CacheConfig? target = null)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (target is null)
        {
            return new CommonConfig.CacheConfig
            {
                StalenessConfigs = FromDomain(config.StalenessConfigs),
                SecondLevelCache = FromDomain(config.SecondLevelCache)
            };
        }

        return target with
        {
            StalenessConfigs = FromDomain(config.StalenessConfigs),
            SecondLevelCache = FromDomain(config.SecondLevelCache)
        };
    }

    public static DomainConfig.DatabaseConnectionPoolConfig ToDomain(CommonConfig.DatabaseConnectionPoolConfig config, DomainConfig.DatabaseConnectionPoolConfig? target = null)
    {
        ArgumentNullException.ThrowIfNull(config);

        target ??= new DomainConfig.DatabaseConnectionPoolConfig();

        return target with
        {
            Enabled = config.Enabled,
            MinPoolSize = config.MinPoolSize,
            MaxPoolSize = config.MaxPoolSize,
            TimeoutSeconds = config.TimeoutSeconds,
            ConnectionIdleLifetimeSeconds = config.ConnectionIdleLifetimeSeconds,
            ConnectionPruningIntervalSeconds = config.ConnectionPruningIntervalSeconds
        };
    }

    public static CommonConfig.DatabaseConnectionPoolConfig FromDomain(DomainConfig.DatabaseConnectionPoolConfig config, CommonConfig.DatabaseConnectionPoolConfig? target = null)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (target is null)
        {
            return new CommonConfig.DatabaseConnectionPoolConfig
            {
                Enabled = config.Enabled,
                MinPoolSize = config.MinPoolSize,
                MaxPoolSize = config.MaxPoolSize,
                TimeoutSeconds = config.TimeoutSeconds,
                ConnectionIdleLifetimeSeconds = config.ConnectionIdleLifetimeSeconds,
                ConnectionPruningIntervalSeconds = config.ConnectionPruningIntervalSeconds
            };
        }

        return target with
        {
            Enabled = config.Enabled,
            MinPoolSize = config.MinPoolSize,
            MaxPoolSize = config.MaxPoolSize,
            TimeoutSeconds = config.TimeoutSeconds,
            ConnectionIdleLifetimeSeconds = config.ConnectionIdleLifetimeSeconds,
            ConnectionPruningIntervalSeconds = config.ConnectionPruningIntervalSeconds
        };
    }

    public static DomainConfig.DatabaseConfig ToDomain(CommonConfig.DatabaseConfig config, DomainConfig.DatabaseConfig? target = null)
    {
        ArgumentNullException.ThrowIfNull(config);

        target ??= new DomainConfig.DatabaseConfig();

        return target with
        {
            Host = config.Host,
            Port = config.Port,
            DatabaseName = config.DatabaseName,
            Username = config.Username,
            Password = config.Password,
            IsDevMode = config.IsDevMode,
            LogAllQueries = config.LogAllQueries,
            ConnectionPoolConfig = ToDomain(config.ConnectionPoolConfig),
            SensitiveDataLogging = config.SensitiveDataLogging,
            DbContextPoolSize = config.DbContextPoolSize,
            CommandTimeoutSeconds = config.CommandTimeoutSeconds,
            EnableRetryOnFailure = config.EnableRetryOnFailure,
            MaxRetryCount = config.MaxRetryCount,
            MaxRetryDelay = config.MaxRetryDelay
        };
    }

    public static CommonConfig.DatabaseConfig FromDomain(DomainConfig.DatabaseConfig config, CommonConfig.DatabaseConfig? target = null)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (target is null)
        {
            return new CommonConfig.DatabaseConfig
            {
                Host = config.Host,
                Port = config.Port,
                DatabaseName = config.DatabaseName,
                Username = config.Username,
                Password = config.Password,
                IsDevMode = config.IsDevMode,
                LogAllQueries = config.LogAllQueries,
                ConnectionPoolConfig = FromDomain(config.ConnectionPoolConfig),
                SensitiveDataLogging = config.SensitiveDataLogging,
                DbContextPoolSize = config.DbContextPoolSize,
                CommandTimeoutSeconds = config.CommandTimeoutSeconds,
                EnableRetryOnFailure = config.EnableRetryOnFailure,
                MaxRetryCount = config.MaxRetryCount,
                MaxRetryDelay = config.MaxRetryDelay
            };
        }

        return target with
        {
            Host = config.Host,
            Port = config.Port,
            DatabaseName = config.DatabaseName,
            Username = config.Username,
            Password = config.Password,
            IsDevMode = config.IsDevMode,
            LogAllQueries = config.LogAllQueries,
            ConnectionPoolConfig = FromDomain(config.ConnectionPoolConfig),
            SensitiveDataLogging = config.SensitiveDataLogging,
            DbContextPoolSize = config.DbContextPoolSize,
            CommandTimeoutSeconds = config.CommandTimeoutSeconds,
            EnableRetryOnFailure = config.EnableRetryOnFailure,
            MaxRetryCount = config.MaxRetryCount,
            MaxRetryDelay = config.MaxRetryDelay
        };
    }

    public static DomainConfig.ProviderConfig ToDomain(CommonConfig.ProviderConfig config, DomainConfig.ProviderConfig? target = null)
    {
        ArgumentNullException.ThrowIfNull(config);

        target ??= new DomainConfig.ProviderConfig();

        return target with
        {
            BaseUri = config.BaseUri,
            BypassSslValidation = config.BypassSslValidation,
            TimeoutSeconds = config.TimeoutSeconds
        };
    }

    public static CommonConfig.ProviderConfig FromDomain(DomainConfig.ProviderConfig config, CommonConfig.ProviderConfig? target = null)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (target is null)
        {
            return new CommonConfig.ProviderConfig
            {
                BaseUri = config.BaseUri,
                BypassSslValidation = config.BypassSslValidation,
                TimeoutSeconds = config.TimeoutSeconds
            };
        }

        return target with
        {
            BaseUri = config.BaseUri,
            BypassSslValidation = config.BypassSslValidation,
            TimeoutSeconds = config.TimeoutSeconds
        };
    }

    public static DomainConfig.SecondLevelCacheConfig ToDomain(CommonConfig.SecondLevelCacheConfig config, DomainConfig.SecondLevelCacheConfig? target = null)
    {
        ArgumentNullException.ThrowIfNull(config);

        target ??= new DomainConfig.SecondLevelCacheConfig();

        return target with
        {
            Enable = config.Enable,
            CacheKeyPrefix = config.CacheKeyPrefix,
            EnableLogging = config.EnableLogging,
            CacheAllQueries = config.CacheAllQueries,
            CacheAllQueriesExpirationMode = config.CacheAllQueriesExpirationMode,
            CacheAllQueriesTimeout = config.CacheAllQueriesTimeout,
            UseDbCallsIfCachingProviderIsDown = config.UseDbCallsIfCachingProviderIsDown,
            EnableSensitiveDataLogging = config.EnableSensitiveDataLogging,
            EnableDetailedErrors = config.EnableDetailedErrors,
            DbCallsIfCachingProviderIsDownTimeout = config.DbCallsIfCachingProviderIsDownTimeout
        };
    }

    public static CommonConfig.SecondLevelCacheConfig FromDomain(DomainConfig.SecondLevelCacheConfig config, CommonConfig.SecondLevelCacheConfig? target = null)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (target is null)
        {
            return new CommonConfig.SecondLevelCacheConfig
            {
                Enable = config.Enable,
                CacheKeyPrefix = config.CacheKeyPrefix,
                EnableLogging = config.EnableLogging,
                CacheAllQueries = config.CacheAllQueries,
                CacheAllQueriesExpirationMode = config.CacheAllQueriesExpirationMode,
                CacheAllQueriesTimeout = config.CacheAllQueriesTimeout,
                UseDbCallsIfCachingProviderIsDown = config.UseDbCallsIfCachingProviderIsDown,
                EnableSensitiveDataLogging = config.EnableSensitiveDataLogging,
                EnableDetailedErrors = config.EnableDetailedErrors,
                DbCallsIfCachingProviderIsDownTimeout = config.DbCallsIfCachingProviderIsDownTimeout
            };
        }

        return target with
        {
            Enable = config.Enable,
            CacheKeyPrefix = config.CacheKeyPrefix,
            EnableLogging = config.EnableLogging,
            CacheAllQueries = config.CacheAllQueries,
            CacheAllQueriesExpirationMode = config.CacheAllQueriesExpirationMode,
            CacheAllQueriesTimeout = config.CacheAllQueriesTimeout,
            UseDbCallsIfCachingProviderIsDown = config.UseDbCallsIfCachingProviderIsDown,
            EnableSensitiveDataLogging = config.EnableSensitiveDataLogging,
            EnableDetailedErrors = config.EnableDetailedErrors,
            DbCallsIfCachingProviderIsDownTimeout = config.DbCallsIfCachingProviderIsDownTimeout
        };
    }

    public static DomainConfig.StalenessConfig ToDomain(CommonConfig.StalenessConfig config, DomainConfig.StalenessConfig? target = null)
    {
        ArgumentNullException.ThrowIfNull(config);

        target ??= new DomainConfig.StalenessConfig();

        return target with
        {
            VideoStalenessThreshold = config.VideoStalenessThreshold,
            ChannelStalenessThreshold = config.ChannelStalenessThreshold,
            ImageStalenessThreshold = config.ImageStalenessThreshold,
            CaptionStalenessThreshold = config.CaptionStalenessThreshold,
            VideoMemoryCacheCapacity = config.VideoMemoryCacheCapacity,
            ChannelMemoryCacheCapacity = config.ChannelMemoryCacheCapacity,
            ImageMemoryCacheCapacity = config.ImageMemoryCacheCapacity
        };
    }

    public static CommonConfig.StalenessConfig FromDomain(DomainConfig.StalenessConfig config, CommonConfig.StalenessConfig? target = null)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (target is null)
        {
            return new CommonConfig.StalenessConfig
            {
                VideoStalenessThreshold = config.VideoStalenessThreshold,
                ChannelStalenessThreshold = config.ChannelStalenessThreshold,
                ImageStalenessThreshold = config.ImageStalenessThreshold,
                CaptionStalenessThreshold = config.CaptionStalenessThreshold,
                VideoMemoryCacheCapacity = config.VideoMemoryCacheCapacity,
                ChannelMemoryCacheCapacity = config.ChannelMemoryCacheCapacity,
                ImageMemoryCacheCapacity = config.ImageMemoryCacheCapacity
            };
        }

        return target with
        {
            VideoStalenessThreshold = config.VideoStalenessThreshold,
            ChannelStalenessThreshold = config.ChannelStalenessThreshold,
            ImageStalenessThreshold = config.ImageStalenessThreshold,
            CaptionStalenessThreshold = config.CaptionStalenessThreshold,
            VideoMemoryCacheCapacity = config.VideoMemoryCacheCapacity,
            ChannelMemoryCacheCapacity = config.ChannelMemoryCacheCapacity,
            ImageMemoryCacheCapacity = config.ImageMemoryCacheCapacity
        };
    }
}
