using Microsoft.EntityFrameworkCore;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities.Enums;

namespace TMS.Apps.FrontTube.Backend.Repository.DataBase;

/// <summary>
/// Database context for FTube application.
/// </summary>
public class DataBaseContext : DbContext
{
    public DataBaseContext(DbContextOptions<DataBaseContext> options)
        : base(options)
    {
    }

    // Shared tables (not user-scoped)
    public DbSet<ChannelEntity> Channels => Set<ChannelEntity>();

    public DbSet<VideoEntity> Videos => Set<VideoEntity>();

    public DbSet<ImageEntity> Images => Set<ImageEntity>();

    public DbSet<VideoCaptionEntity> VideoCaptions => Set<VideoCaptionEntity>();

    public DbSet<ChannelAvatarMapEntity> ChannelAvatarMaps => Set<ChannelAvatarMapEntity>();

    public DbSet<ChannelBannerMapEntity> ChannelBannerMaps => Set<ChannelBannerMapEntity>();

    public DbSet<VideoThumbnailMapEntity> VideoThumbnailMaps => Set<VideoThumbnailMapEntity>();

    public DbSet<StreamEntity> Streams => Set<StreamEntity>();

    // User table
    public DbSet<UserEntity> Users => Set<UserEntity>();

    // Scoped tables (user-specific)
    public DbSet<ScopedSubscriptionEntity> ScopedSubscriptions => Set<ScopedSubscriptionEntity>();

    public DbSet<ScopedLocalPlaylistEntity> ScopedLocalPlaylists => Set<ScopedLocalPlaylistEntity>();

    public DbSet<ScopedLocalPlaylistVideoMapEntity> ScopedLocalPlaylistVideoMaps => Set<ScopedLocalPlaylistVideoMapEntity>();

    public DbSet<ScopedWatchingHistoryEntity> ScopedWatchingHistory => Set<ScopedWatchingHistoryEntity>();

    public DbSet<ScopedGroupEntity> ScopedGroups => Set<ScopedGroupEntity>();

    public DbSet<ScopedSubscriptionGroupMapEntity> ScopedSubscriptionGroupMaps => Set<ScopedSubscriptionGroupMapEntity>();

    // Enum tables
    public DbSet<EnumStreamTypeEntity> EnumStreamTypes => Set<EnumStreamTypeEntity>();

    public DbSet<EnumVideoContainerEntity> EnumVideoContainers => Set<EnumVideoContainerEntity>();

    public DbSet<EnumVideoCodecEntity> EnumVideoCodecs => Set<EnumVideoCodecEntity>();

    public DbSet<EnumAudioCodecEntity> EnumAudioCodecs => Set<EnumAudioCodecEntity>();

    public DbSet<EnumAudioQualityEntity> EnumAudioQualities => Set<EnumAudioQualityEntity>();

    public DbSet<EnumProjectionTypeEntity> EnumProjectionTypes => Set<EnumProjectionTypeEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure enum tables first (they have seed data)
        ConfigureEnumStreamType(modelBuilder);
        ConfigureEnumVideoContainer(modelBuilder);
        ConfigureEnumVideoCodec(modelBuilder);
        ConfigureEnumAudioCodec(modelBuilder);
        ConfigureEnumAudioQuality(modelBuilder);
        ConfigureEnumProjectionType(modelBuilder);

        // Shared tables
        ConfigureChannel(modelBuilder);
        ConfigureVideo(modelBuilder);
        ConfigureImage(modelBuilder);
        ConfigureVideoCaption(modelBuilder);
        ConfigureStream(modelBuilder);
        ConfigureChannelAvatarMap(modelBuilder);
        ConfigureChannelBannerMap(modelBuilder);
        ConfigureVideoThumbnailMap(modelBuilder);

        // User table
        ConfigureUser(modelBuilder);

        // Scoped tables
        ConfigureScopedSubscription(modelBuilder);
        ConfigureScopedLocalPlaylist(modelBuilder);
        ConfigureScopedLocalPlaylistVideoMap(modelBuilder);
        ConfigureScopedWatchingHistory(modelBuilder);
        ConfigureScopedGroup(modelBuilder);
        ConfigureScopedSubscriptionGroupMap(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("user");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");

            entity.HasIndex(e => e.Name);
        });
    }

    private static void ConfigureChannel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChannelEntity>(entity =>
        {
            entity.ToTable("channel");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");

            entity.Property(e => e.LastSyncedAt).HasColumnName("last_synced_at");
            entity.Property(e => e.RemoteId).HasColumnName("remote_id").HasMaxLength(24).IsRequired();
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Alias).HasColumnName("alias").HasMaxLength(255);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(5000);
            entity.Property(e => e.DescriptionHtml).HasColumnName("description_html");
            entity.Property(e => e.Handle).HasColumnName("handle").HasMaxLength(50);
            entity.Property(e => e.SubscriberCount).HasColumnName("subscriber_count");
            entity.Property(e => e.VideoCount).HasColumnName("video_count");
            entity.Property(e => e.TotalViewCount).HasColumnName("total_view_count");
            entity.Property(e => e.JoinedAt).HasColumnName("joined_at");
            entity.Property(e => e.IsVerified).HasColumnName("is_verified");
            entity.Property(e => e.Keywords).HasColumnName("keywords").HasMaxLength(1000);

            entity.HasIndex(e => e.RemoteId).IsUnique();
            entity.HasIndex(e => e.Handle);
        });
    }

    private static void ConfigureVideo(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VideoEntity>(entity =>
        {
            entity.ToTable("video");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");

            entity.Property(e => e.LastSyncedAt).HasColumnName("last_synced_at");
            entity.Property(e => e.RemoteId).HasColumnName("remote_id").HasMaxLength(20).IsRequired();
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(5000);
            entity.Property(e => e.DescriptionHtml).HasColumnName("description_html");
            entity.Property(e => e.DurationSeconds).HasColumnName("duration_seconds");
            entity.Property(e => e.ViewCount).HasColumnName("view_count");
            entity.Property(e => e.LikesCount).HasColumnName("likes_count");
            entity.Property(e => e.DislikesCount).HasColumnName("dislikes_count");
            entity.Property(e => e.PublishedAt).HasColumnName("published_at");
            entity.Property(e => e.Genre).HasColumnName("genre").HasMaxLength(100);
            entity.Property(e => e.Keywords).HasColumnName("keywords").HasMaxLength(1000);
            entity.Property(e => e.IsLive).HasColumnName("is_live");
            entity.Property(e => e.IsUpcoming).HasColumnName("is_upcoming");
            entity.Property(e => e.IsShort).HasColumnName("is_short");
            entity.Property(e => e.IsWatched).HasColumnName("is_watched");
            entity.Property(e => e.ChannelId).HasColumnName("channel_id");

            entity.HasIndex(e => e.RemoteId).IsUnique();
            entity.HasIndex(e => e.ChannelId);
            entity.HasIndex(e => e.PublishedAt);

            entity.HasOne(e => e.Channel)
                .WithMany(c => c.Videos)
                .HasForeignKey(e => e.ChannelId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureImage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ImageEntity>(entity =>
        {
            entity.ToTable("image");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.LastSyncedAt).HasColumnName("last_synced_at");
            entity.Property(e => e.RemoteUrl).HasColumnName("remote_url").HasMaxLength(2000).IsRequired();
            entity.Property(e => e.Data).HasColumnName("data");
            entity.Property(e => e.Width).HasColumnName("width");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.MimeType).HasColumnName("mime_type").HasMaxLength(50);

            entity.HasIndex(e => e.RemoteUrl).IsUnique();
        });
    }

    private static void ConfigureVideoCaption(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VideoCaptionEntity>(entity =>
        {
            entity.ToTable("video_caption");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.VideoId).HasColumnName("video_id");
            entity.Property(e => e.Label).HasColumnName("label").HasMaxLength(100).IsRequired();
            entity.Property(e => e.LanguageCode).HasColumnName("language_code").HasMaxLength(10).IsRequired();
            entity.Property(e => e.IsAutoGenerated).HasColumnName("is_auto_generated");
            entity.Property(e => e.RemoteUrl).HasColumnName("remote_url").HasMaxLength(2000);
            entity.Property(e => e.CachedContent).HasColumnName("cached_content");

            entity.HasIndex(e => e.VideoId);
            entity.HasIndex(e => new { e.VideoId, e.LanguageCode });

            entity.HasOne(e => e.Video)
                .WithMany(v => v.Captions)
                .HasForeignKey(e => e.VideoId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureChannelAvatarMap(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChannelAvatarMapEntity>(entity =>
        {
            entity.ToTable("channel_avatar_map");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChannelId).HasColumnName("channel_id");
            entity.Property(e => e.ImageId).HasColumnName("image_id");

            entity.HasIndex(e => new { e.ChannelId, e.ImageId }).IsUnique();

            entity.HasOne(e => e.Channel)
                .WithMany(c => c.Avatars)
                .HasForeignKey(e => e.ChannelId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Image)
                .WithMany(i => i.ChannelAvatars)
                .HasForeignKey(e => e.ImageId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureChannelBannerMap(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChannelBannerMapEntity>(entity =>
        {
            entity.ToTable("channel_banner_map");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChannelId).HasColumnName("channel_id");
            entity.Property(e => e.ImageId).HasColumnName("image_id");

            entity.HasIndex(e => new { e.ChannelId, e.ImageId }).IsUnique();

            entity.HasOne(e => e.Channel)
                .WithMany(c => c.Banners)
                .HasForeignKey(e => e.ChannelId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Image)
                .WithMany(i => i.ChannelBanners)
                .HasForeignKey(e => e.ImageId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureVideoThumbnailMap(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VideoThumbnailMapEntity>(entity =>
        {
            entity.ToTable("video_thumbnail_map");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.VideoId).HasColumnName("video_id");
            entity.Property(e => e.ImageId).HasColumnName("image_id");

            entity.HasIndex(e => new { e.VideoId, e.ImageId }).IsUnique();

            entity.HasOne(e => e.Video)
                .WithMany(v => v.Thumbnails)
                .HasForeignKey(e => e.VideoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Image)
                .WithMany(i => i.VideoThumbnails)
                .HasForeignKey(e => e.ImageId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureScopedSubscription(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScopedSubscriptionEntity>(entity =>
        {
            entity.ToTable("scoped_subscription");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");

            entity.Property(e => e.ModifiedAt)
                .HasColumnName("modified_at")
                .HasDefaultValueSql("now()");

            entity.Property(e => e.ChannelId).HasColumnName("channel_id");
            entity.Property(e => e.Alias).HasColumnName("alias").HasMaxLength(255);
            entity.Property(e => e.NotificationsEnabled).HasColumnName("notifications_enabled");
            entity.Property(e => e.NotificationSettings).HasColumnName("notification_settings");
            entity.Property(e => e.DefaultPlaybackSpeed).HasColumnName("default_playback_speed");
            entity.Property(e => e.DefaultSyncInterval).HasColumnName("default_sync_interval");

            entity.HasIndex(e => new { e.UserId, e.ChannelId }).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ChannelId);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Channel)
                .WithMany(c => c.Subscriptions)
                .HasForeignKey(e => e.ChannelId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureScopedLocalPlaylist(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScopedLocalPlaylistEntity>(entity =>
        {
            entity.ToTable("scoped_local_playlist");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");

            entity.Property(e => e.Alias).HasColumnName("alias").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
            entity.Property(e => e.IsBuiltIn).HasColumnName("is_built_in");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");

            entity.HasIndex(e => new { e.UserId, e.Alias });
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                .WithMany(u => u.LocalPlaylists)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureScopedLocalPlaylistVideoMap(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScopedLocalPlaylistVideoMapEntity>(entity =>
        {
            entity.ToTable("scoped_local_playlist_video_map");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");

            entity.Property(e => e.LocalPlaylistId).HasColumnName("local_playlist_id");
            entity.Property(e => e.VideoId).HasColumnName("video_id");
            entity.Property(e => e.Position).HasColumnName("position");

            entity.HasIndex(e => new { e.LocalPlaylistId, e.VideoId }).IsUnique();
            entity.HasIndex(e => new { e.LocalPlaylistId, e.Position });

            entity.HasOne(e => e.LocalPlaylist)
                .WithMany(p => p.VideoMappings)
                .HasForeignKey(e => e.LocalPlaylistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Video)
                .WithMany(v => v.PlaylistMappings)
                .HasForeignKey(e => e.VideoId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureScopedWatchingHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScopedWatchingHistoryEntity>(entity =>
        {
            entity.ToTable("scoped_watching_history");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.Property(e => e.StartedAt)
                .HasColumnName("started_at")
                .HasDefaultValueSql("now()");

            entity.Property(e => e.LastWatchedAt).HasColumnName("last_watched_at");
            entity.Property(e => e.VideoId).HasColumnName("video_id");
            entity.Property(e => e.LastPosition).HasColumnName("last_position");
            entity.Property(e => e.VideoDuration).HasColumnName("video_duration");
            entity.Property(e => e.MarkedAsWatched).HasColumnName("marked_as_watched");
            entity.Property(e => e.PlaybackSpeed).HasColumnName("playback_speed");
            entity.Property(e => e.VideoQualityLabel).HasColumnName("video_quality_label").HasMaxLength(20);

            entity.HasIndex(e => new { e.UserId, e.VideoId });
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.VideoId);
            entity.HasIndex(e => e.StartedAt);

            entity.HasOne(e => e.User)
                .WithMany(u => u.WatchingHistory)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Video)
                .WithMany(v => v.WatchingHistory)
                .HasForeignKey(e => e.VideoId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureScopedGroup(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScopedGroupEntity>(entity =>
        {
            entity.ToTable("scoped_group");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Alias).HasColumnName("alias").HasMaxLength(255).IsRequired();
            entity.Property(e => e.ParentGroupId).HasColumnName("parent_group_id");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");

            entity.Property(e => e.ModifiedAt)
                .HasColumnName("modified_at")
                .HasDefaultValueSql("now()");

            entity.HasIndex(e => new { e.UserId, e.Alias });
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ParentGroupId);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Groups)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentGroup)
                .WithMany(g => g.ChildGroups)
                .HasForeignKey(e => e.ParentGroupId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureScopedSubscriptionGroupMap(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScopedSubscriptionGroupMapEntity>(entity =>
        {
            entity.ToTable("scoped_subscription_group_map");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.SubscriptionId).HasColumnName("subscription_id");

            entity.HasIndex(e => new { e.GroupId, e.SubscriptionId }).IsUnique();
            entity.HasIndex(e => e.GroupId);
            entity.HasIndex(e => e.SubscriptionId);

            entity.HasOne(e => e.Group)
                .WithMany(g => g.SubscriptionMappings)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Subscription)
                .WithMany(s => s.GroupMappings)
                .HasForeignKey(e => e.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureEnumStreamType(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EnumStreamTypeEntity>(entity =>
        {
            entity.ToTable("enum_stream_type");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(50).IsRequired();

            entity.HasIndex(e => e.Name).IsUnique();

            // Seed data from StreamType enum
            entity.HasData(
                new EnumStreamTypeEntity { Id = 0, Name = "Unknown" },
                new EnumStreamTypeEntity { Id = 1, Name = "Video" },
                new EnumStreamTypeEntity { Id = 2, Name = "Audio" },
                new EnumStreamTypeEntity { Id = 3, Name = "VideoAndAudio" }
            );
        });
    }

    private static void ConfigureEnumVideoContainer(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EnumVideoContainerEntity>(entity =>
        {
            entity.ToTable("enum_video_container");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(50).IsRequired();

            entity.HasIndex(e => e.Name).IsUnique();

            // Seed data from VideoContainer enum
            entity.HasData(
                new EnumVideoContainerEntity { Id = 0, Name = "Unknown" },
                new EnumVideoContainerEntity { Id = 1, Name = "Mp4" },
                new EnumVideoContainerEntity { Id = 2, Name = "WebM" },
                new EnumVideoContainerEntity { Id = 3, Name = "Mkv" },
                new EnumVideoContainerEntity { Id = 4, Name = "M4a" },
                new EnumVideoContainerEntity { Id = 5, Name = "Opus" }
            );
        });
    }

    private static void ConfigureEnumVideoCodec(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EnumVideoCodecEntity>(entity =>
        {
            entity.ToTable("enum_video_codec");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(50).IsRequired();

            entity.HasIndex(e => e.Name).IsUnique();

            // Seed data from VideoCodec enum
            entity.HasData(
                new EnumVideoCodecEntity { Id = 0, Name = "Unknown" },
                new EnumVideoCodecEntity { Id = 1, Name = "H264" },
                new EnumVideoCodecEntity { Id = 2, Name = "H265" },
                new EnumVideoCodecEntity { Id = 3, Name = "Vp8" },
                new EnumVideoCodecEntity { Id = 4, Name = "Vp9" },
                new EnumVideoCodecEntity { Id = 5, Name = "Av1" }
            );
        });
    }

    private static void ConfigureEnumAudioCodec(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EnumAudioCodecEntity>(entity =>
        {
            entity.ToTable("enum_audio_codec");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(50).IsRequired();

            entity.HasIndex(e => e.Name).IsUnique();

            // Seed data from AudioCodec enum
            entity.HasData(
                new EnumAudioCodecEntity { Id = 0, Name = "Unknown" },
                new EnumAudioCodecEntity { Id = 1, Name = "Aac" },
                new EnumAudioCodecEntity { Id = 2, Name = "Mp3" },
                new EnumAudioCodecEntity { Id = 3, Name = "Vorbis" },
                new EnumAudioCodecEntity { Id = 4, Name = "Opus" }
            );
        });
    }

    private static void ConfigureEnumAudioQuality(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EnumAudioQualityEntity>(entity =>
        {
            entity.ToTable("enum_audio_quality");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(50).IsRequired();

            entity.HasIndex(e => e.Name).IsUnique();

            // Seed data from AudioQuality enum
            entity.HasData(
                new EnumAudioQualityEntity { Id = 0, Name = "Unknown" },
                new EnumAudioQualityEntity { Id = 1, Name = "Low" },
                new EnumAudioQualityEntity { Id = 2, Name = "Medium" },
                new EnumAudioQualityEntity { Id = 3, Name = "High" }
            );
        });
    }

    private static void ConfigureEnumProjectionType(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EnumProjectionTypeEntity>(entity =>
        {
            entity.ToTable("enum_projection_type");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(50).IsRequired();

            entity.HasIndex(e => e.Name).IsUnique();

            // Seed data from ProjectionType enum
            entity.HasData(
                new EnumProjectionTypeEntity { Id = 0, Name = "Unknown" },
                new EnumProjectionTypeEntity { Id = 1, Name = "Rectangular" },
                new EnumProjectionTypeEntity { Id = 2, Name = "Spherical" },
                new EnumProjectionTypeEntity { Id = 3, Name = "Spherical360" },
                new EnumProjectionTypeEntity { Id = 4, Name = "Mesh" }
            );
        });
    }

    private static void ConfigureStream(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StreamEntity>(entity =>
        {
            entity.ToTable("stream");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");

            entity.Property(e => e.LastSyncedAt).HasColumnName("last_synced_at");
            entity.Property(e => e.VideoId).HasColumnName("video_id");
            entity.Property(e => e.Url).HasColumnName("url").HasMaxLength(2000).IsRequired();
            entity.Property(e => e.StreamTypeId).HasColumnName("stream_type_id");
            entity.Property(e => e.ContainerId).HasColumnName("container_id");
            entity.Property(e => e.VideoCodecId).HasColumnName("video_codec_id");
            entity.Property(e => e.AudioCodecId).HasColumnName("audio_codec_id");
            entity.Property(e => e.AudioQualityId).HasColumnName("audio_quality_id");
            entity.Property(e => e.ProjectionTypeId).HasColumnName("projection_type_id");
            entity.Property(e => e.QualityLabel).HasColumnName("quality_label").HasMaxLength(20);
            entity.Property(e => e.Width).HasColumnName("width");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.FrameRate).HasColumnName("frame_rate");
            entity.Property(e => e.Bitrate).HasColumnName("bitrate");
            entity.Property(e => e.ContentLength).HasColumnName("content_length");
            entity.Property(e => e.AudioSampleRate).HasColumnName("audio_sample_rate");
            entity.Property(e => e.AudioChannels).HasColumnName("audio_channels");
            entity.Property(e => e.MimeType).HasColumnName("mime_type").HasMaxLength(100);
            entity.Property(e => e.Itag).HasColumnName("itag");

            entity.HasIndex(e => e.VideoId);
            entity.HasIndex(e => new { e.VideoId, e.Itag }).IsUnique();

            entity.HasOne(e => e.Video)
                .WithMany(v => v.Streams)
                .HasForeignKey(e => e.VideoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.StreamType)
                .WithMany()
                .HasForeignKey(e => e.StreamTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Container)
                .WithMany()
                .HasForeignKey(e => e.ContainerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.VideoCodec)
                .WithMany()
                .HasForeignKey(e => e.VideoCodecId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AudioCodec)
                .WithMany()
                .HasForeignKey(e => e.AudioCodecId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AudioQuality)
                .WithMany()
                .HasForeignKey(e => e.AudioQualityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ProjectionType)
                .WithMany()
                .HasForeignKey(e => e.ProjectionTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
