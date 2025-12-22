using Microsoft.EntityFrameworkCore;
using TMS.Apps.FTube.Backend.DataBase.Entities;

namespace TMS.Apps.FTube.Backend.DataBase;

/// <summary>
/// Database context for FTube application.
/// </summary>
public class FTubeDbContext : DbContext
{
    public FTubeDbContext(DbContextOptions<FTubeDbContext> options)
        : base(options)
    {
    }

    public DbSet<ChannelEntity> Channels => Set<ChannelEntity>();

    public DbSet<VideoEntity> Videos => Set<VideoEntity>();

    public DbSet<ImageEntity> Images => Set<ImageEntity>();

    public DbSet<VideoCaptionEntity> VideoCaptions => Set<VideoCaptionEntity>();

    public DbSet<ChannelAvatarMapEntity> ChannelAvatarMaps => Set<ChannelAvatarMapEntity>();

    public DbSet<ChannelBannerMapEntity> ChannelBannerMaps => Set<ChannelBannerMapEntity>();

    public DbSet<VideoThumbnailMapEntity> VideoThumbnailMaps => Set<VideoThumbnailMapEntity>();

    public DbSet<SubscriptionEntity> Subscriptions => Set<SubscriptionEntity>();

    public DbSet<LocalPlaylistEntity> LocalPlaylists => Set<LocalPlaylistEntity>();

    public DbSet<LocalPlaylistVideoMapEntity> LocalPlaylistVideoMaps => Set<LocalPlaylistVideoMapEntity>();

    public DbSet<WatchingHistoryEntity> WatchingHistory => Set<WatchingHistoryEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureChannel(modelBuilder);
        ConfigureVideo(modelBuilder);
        ConfigureImage(modelBuilder);
        ConfigureVideoCaption(modelBuilder);
        ConfigureChannelAvatarMap(modelBuilder);
        ConfigureChannelBannerMap(modelBuilder);
        ConfigureVideoThumbnailMap(modelBuilder);
        ConfigureSubscription(modelBuilder);
        ConfigureLocalPlaylist(modelBuilder);
        ConfigureLocalPlaylistVideoMap(modelBuilder);
        ConfigureWatchingHistory(modelBuilder);
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
            entity.Property(e => e.DefaultPlaybackSpeed).HasColumnName("default_playback_speed");

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
            entity.Property(e => e.RemoteId).HasColumnName("remote_id").HasMaxLength(100);
            entity.Property(e => e.RemoteUrl).HasColumnName("remote_url").HasMaxLength(2000);
            entity.Property(e => e.Data).HasColumnName("data");
            entity.Property(e => e.Width).HasColumnName("width");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.Quality).HasColumnName("quality").HasMaxLength(20);
            entity.Property(e => e.MimeType).HasColumnName("mime_type").HasMaxLength(50);

            entity.HasIndex(e => e.RemoteUrl);
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

    private static void ConfigureSubscription(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SubscriptionEntity>(entity =>
        {
            entity.ToTable("subscription");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");

            entity.Property(e => e.ChannelId).HasColumnName("channel_id");
            entity.Property(e => e.NotificationsEnabled).HasColumnName("notifications_enabled");
            entity.Property(e => e.NotificationSettings).HasColumnName("notification_settings");

            entity.HasIndex(e => e.ChannelId).IsUnique();

            entity.HasOne(e => e.Channel)
                .WithOne(c => c.Subscription)
                .HasForeignKey<SubscriptionEntity>(e => e.ChannelId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureLocalPlaylist(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LocalPlaylistEntity>(entity =>
        {
            entity.ToTable("local_playlist");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");

            entity.Property(e => e.Alias).HasColumnName("alias").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
            entity.Property(e => e.IsBuiltIn).HasColumnName("is_built_in");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");

            entity.HasIndex(e => e.Alias);
        });
    }

    private static void ConfigureLocalPlaylistVideoMap(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LocalPlaylistVideoMapEntity>(entity =>
        {
            entity.ToTable("local_playlist_video_map");

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

    private static void ConfigureWatchingHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WatchingHistoryEntity>(entity =>
        {
            entity.ToTable("watching_history");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.StartedAt)
                .HasColumnName("started_at")
                .HasDefaultValueSql("now()");

            entity.Property(e => e.LastWatchedAt).HasColumnName("last_watched_at");
            entity.Property(e => e.VideoId).HasColumnName("video_id");
            entity.Property(e => e.LastPosition).HasColumnName("last_position");
            entity.Property(e => e.VideoDuration).HasColumnName("video_duration");
            entity.Property(e => e.MarkedAsWatched).HasColumnName("mark_as_watched");
            entity.Property(e => e.PlaybackSpeed).HasColumnName("playback_speed");

            entity.HasIndex(e => e.VideoId);
            entity.HasIndex(e => e.StartedAt);

            entity.HasOne(e => e.Video)
                .WithMany(v => v.WatchingHistory)
                .HasForeignKey(e => e.VideoId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
