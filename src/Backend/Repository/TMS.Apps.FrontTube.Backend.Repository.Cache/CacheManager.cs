using BitFaster.Caching;
using BitFaster.Caching.Lru;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Repository.DataBase;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Interfaces;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Mappers;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Configuration;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Repository.Cache;

/// <summary>
/// Data repository implementing multi-tier caching: Memory → Database → Provider.
/// </summary>
public sealed class CacheManager : ICacheManager
{
    private readonly CacheConfig _config;
    private readonly ILogger<CacheManager> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly string _connectionString;
    private readonly DevModeSeeder? _devModeSeeder;

    private readonly ICache<string, CachedItem<Video>> _videoCache;
    private readonly ICache<string, CachedItem<Channel>> _channelCache;
    private readonly ICache<string, CachedItem<CachedImage>> _imageCache;
    private readonly HttpClient _httpClient;

    private bool _disposed;
    private bool _devUserSeeded;

    public CacheManager(
        CacheConfig config,
        ILoggerFactory loggerFactory)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _logger = loggerFactory.CreateLogger<CacheManager>();
        _connectionString = config.DataBase.BuildConnectionString();

        // Initialize dev mode seeder if enabled
        if (config.DataBase.IsDevMode)
        {
            _devModeSeeder = new DevModeSeeder(loggerFactory);
        }

        // Initialize caches with expiration
        _videoCache = new ConcurrentLruBuilder<string, CachedItem<Video>>()
            .WithCapacity(_config.VideoMemoryCacheCapacity)
            .WithExpireAfterWrite(_config.MemoryCacheTtl)
            .Build();

        _channelCache = new ConcurrentLruBuilder<string, CachedItem<Channel>>()
            .WithCapacity(_config.ChannelMemoryCacheCapacity)
            .WithExpireAfterWrite(_config.MemoryCacheTtl)
            .Build();

        _imageCache = new ConcurrentLruBuilder<string, CachedItem<CachedImage>>()
            .WithCapacity(_config.ImageMemoryCacheCapacity)
            .WithExpireAfterWrite(_config.MemoryCacheTtl)
            .Build();

        // Initialize HTTP client for fetching images with browser-like headers
        _httpClient = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
            // Accept any SSL certificate (for development with self-signed certificates)
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        })
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        _httpClient.DefaultRequestHeaders.Accept.ParseAdd("image/avif,image/webp,image/apng,image/svg+xml,image/*,*/*;q=0.8");
        _httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
        _httpClient.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");

        _logger.LogDebug(
            "DataRepository initialized with video staleness: {VideoThreshold}, channel staleness: {ChannelThreshold}, image staleness: {ImageThreshold}",
            _config.VideoStalenessThreshold,
            _config.ChannelStalenessThreshold,
            _config.ImageStalenessThreshold);
    }

    /// <summary>
    /// Creates a new front_tubeDbContext instance using the configured connection string.
    /// Ensures the database schema is created on first use and seeds dev user if enabled.
    /// </summary>
    private async Task<DataBaseContext> CreateDbContextAsync(CancellationToken cancellationToken)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataBaseContext>();
        optionsBuilder.UseNpgsql(_connectionString);
        var context = new DataBaseContext(optionsBuilder.Options);

        // Ensure database schema is created (code-first)
        await context.Database.EnsureCreatedAsync(cancellationToken);

        // Seed dev user if enabled and not already seeded
        if (_devModeSeeder is not null && !_devUserSeeded)
        {
            await _devModeSeeder.SeedDevUserAsync(context, cancellationToken);
            _devUserSeeded = true;
        }

        return context;
    }

    /// <summary>
    /// Creates a new front_tubeDbContext instance synchronously.
    /// Ensures the database schema is created on first use.
    /// </summary>
    private DataBaseContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataBaseContext>();
        optionsBuilder.UseNpgsql(_connectionString);
        var context = new DataBaseContext(optionsBuilder.Options);

        // Ensure database schema is created (code-first)
        context.Database.EnsureCreated();

        return context;
    }

    /// <inheritdoc />
    public async Task<Video?> GetVideoAsync(string remoteId, IProvider provider, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(remoteId);
        ArgumentNullException.ThrowIfNull(provider);

        _logger.LogDebug("GetVideoAsync called for: {RemoteId}", remoteId);

        // Note: Videos always need fresh data from provider because stream URLs expire quickly.
        // We still check cache/DB for metadata freshness to decide if we need to update DB.

        // 1. Check memory cache for fresh data with streams
        if (_videoCache.TryGet(remoteId, out var cached))
        {
            _logger.LogDebug("Video cache hit for: {RemoteId}", remoteId);

            // Only use cache if it has stream data (provider data)
            if (!IsStale(cached.LastSyncedAt, _config.VideoStalenessThreshold) && 
                cached.Data.CombinedStreams.Count > 0)
            {
                _logger.LogDebug("Video is fresh with streams, returning cached: {RemoteId}", remoteId);
                return cached.Data;
            }

            _logger.LogDebug("Video cache is stale or missing streams, will refresh: {RemoteId}", remoteId);
        }

        // 2. Always fetch from provider to get fresh stream URLs
        _logger.LogDebug("Fetching video from provider: {RemoteId}", remoteId);
        var providerVideo = await provider.GetVideoInfoAsync(remoteId, cancellationToken);

        if (providerVideo is null)
        {
            _logger.LogWarning("Provider returned null for video: {RemoteId}", remoteId);
            
            // Fall back to DB if provider fails (may have cached streams)
            await using var dbContextFallback = await CreateDbContextAsync(cancellationToken);
            var entityFallback = await dbContextFallback.Videos
                .Include(v => v.Channel)
                .Include(v => v.Thumbnails).ThenInclude(t => t.Image)
                .Include(v => v.Captions)
                .Include(v => v.Streams)
                .FirstOrDefaultAsync(v => v.RemoteId == remoteId, cancellationToken);
                
            return entityFallback is not null ? VideoEntityMapper.ToContract(entityFallback) : null;
        }

        // 3. Check database for existing entity to update
        await using var dbContext = await CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.Videos
            .Include(v => v.Channel)
            .FirstOrDefaultAsync(v => v.RemoteId == remoteId, cancellationToken);

        // 4. Upsert to database (store metadata for future reference)
        await UpsertVideoAsync(dbContext, entity, providerVideo, cancellationToken);

        // 5. Put in cache with stream data
        PutVideoInCache(remoteId, providerVideo, DateTime.UtcNow);

        _logger.LogDebug("Video fetched and cached: {RemoteId}", remoteId);
        return providerVideo;
    }

    /// <inheritdoc />
    public async Task<Channel?> GetChannelAsync(string remoteId, IProvider provider, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(remoteId);
        ArgumentNullException.ThrowIfNull(provider);

        _logger.LogDebug("GetChannelAsync called for: {RemoteId}", remoteId);

        // 1. Check memory cache
        if (_channelCache.TryGet(remoteId, out var cached))
        {
            _logger.LogDebug("Channel cache hit for: {RemoteId}", remoteId);

            if (!IsStale(cached.LastSyncedAt, _config.ChannelStalenessThreshold))
            {
                _logger.LogDebug("Channel is fresh, returning cached: {RemoteId}", remoteId);
                return cached.Data;
            }

            _logger.LogDebug("Channel is stale, will refresh: {RemoteId}", remoteId);
        }

        // 2. Check database
        await using var dbContext = await CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.Channels
            .Include(c => c.Avatars).ThenInclude(a => a.Image)
            .Include(c => c.Banners).ThenInclude(b => b.Image)
            .FirstOrDefaultAsync(c => c.RemoteId == remoteId, cancellationToken);

        if (entity is not null)
        {
            _logger.LogDebug("Channel found in database: {RemoteId}", remoteId);

            if (!IsStale(entity.LastSyncedAt, _config.ChannelStalenessThreshold))
            {
                var channelDetails = ChannelEntityMapper.ToContract(entity);
                PutChannelInCache(remoteId, channelDetails, entity.LastSyncedAt);
                _logger.LogDebug("Channel is fresh in DB, returning: {RemoteId}", remoteId);
                return channelDetails;
            }

            _logger.LogDebug("Channel is stale in DB, will refresh: {RemoteId}", remoteId);
        }

        // 3. Fetch from provider
        _logger.LogDebug("Fetching channel from provider: {RemoteId}", remoteId);
        var providerChannel = await provider.GetChannelDetailsAsync(remoteId, cancellationToken);

        if (providerChannel is null)
        {
            _logger.LogWarning("Provider returned null for channel: {RemoteId}", remoteId);
            return entity is not null ? ChannelEntityMapper.ToContract(entity) : null;
        }

        // 4. Upsert to database
        await UpsertChannelAsync(dbContext, entity, providerChannel, cancellationToken);

        // 5. Put in cache
        PutChannelInCache(remoteId, providerChannel, DateTime.UtcNow);

        _logger.LogDebug("Channel fetched and cached: {RemoteId}", remoteId);
        return providerChannel;
    }

    /// <inheritdoc />
    public async Task<VideosPage?> GetChannelVideosAsync(
        string channelId,
        string tab,
        string? continuationToken,
        IProvider provider,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channelId);
        ArgumentNullException.ThrowIfNull(provider);

        _logger.LogDebug(
            "GetChannelVideosAsync called for channel: {ChannelId}, tab: {Tab}, continuation: {HasContinuation}",
            channelId,
            tab,
            continuationToken is not null);

        // Channel video pages are not cached as they change frequently
        // But we do save video summaries to DB for persistence
        var page = await provider.GetChannelVideosAsync(channelId, tab, continuationToken, cancellationToken);

        if (page?.Videos is not null && page.Videos.Count > 0)
        {
            _logger.LogDebug("Fetched {Count} videos from channel", page.Videos.Count);
            
            // Save video summaries to database in background (don't block the response)
            // Use CancellationToken.None so the background save completes even if the request ends
            _ = Task.Run(async () =>
            {
                try
                {
                    await UpsertVideoSummariesAsync(channelId, page.Videos, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save video summaries for channel {ChannelId}", channelId);
                }
            });
        }

        return page;
    }

    private async Task UpsertVideoSummariesAsync(string channelId, IReadOnlyList<VideoMetadata> videos, CancellationToken cancellationToken)
    {
        await using var dbContext = await CreateDbContextAsync(cancellationToken);

        // Get channel entity to link videos
        var channelEntity = await dbContext.Channels
            .FirstOrDefaultAsync(c => c.RemoteId == channelId, cancellationToken);

        int? dbChannelId = channelEntity?.Id;

        foreach (var video in videos)
        {
            // Check if video already exists
            var existingVideo = await dbContext.Videos
                .FirstOrDefaultAsync(v => v.RemoteId == video.RemoteId, cancellationToken);

            VideoEntity videoEntity;

            if (existingVideo is null)
            {
                // Create new video from summary
                videoEntity = VideoEntityMapper.ToEntity(video, dbChannelId);
                dbContext.Videos.Add(videoEntity);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            else
            {
                videoEntity = existingVideo;

                // Update existing video's basic info if needed
                // Don't overwrite full VideoInfo data with summary data
                if (existingVideo.ChannelId is null && dbChannelId.HasValue)
                {
                    existingVideo.ChannelId = dbChannelId.Value;
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
            }

            // Upsert video thumbnails
            if (video.Thumbnails.Count > 0)
            {
                await UpsertVideoThumbnailsAsync(dbContext, videoEntity, video.Thumbnails, cancellationToken);
            }
        }

        _logger.LogDebug("Saved {Count} video summaries for channel {ChannelId}", videos.Count, channelId);
    }

    /// <inheritdoc />
    public void InvalidateVideo(string remoteId)
    {
        if (string.IsNullOrWhiteSpace(remoteId))
        {
            return;
        }

        _videoCache.TryRemove(remoteId);
        _logger.LogDebug("Invalidated video cache for: {RemoteId}", remoteId);
    }

    /// <inheritdoc />
    public void InvalidateChannel(string remoteId)
    {
        if (string.IsNullOrWhiteSpace(remoteId))
        {
            return;
        }

        _channelCache.TryRemove(remoteId);
        _logger.LogDebug("Invalidated channel cache for: {RemoteId}", remoteId);
    }

    /// <inheritdoc />
    public void ClearMemoryCache()
    {
        _videoCache.Clear();
        _channelCache.Clear();
        _logger.LogInformation("Cleared all memory caches");
    }

    private static bool IsStale(DateTime lastSyncedAt, TimeSpan threshold)
    {
        return DateTime.UtcNow - lastSyncedAt > threshold;
    }

    private void PutVideoInCache(string remoteId, Video video, DateTime lastSyncedAt)
    {
        _videoCache.AddOrUpdate(remoteId, new CachedItem<Video>(video, lastSyncedAt));
    }

    private void PutChannelInCache(string remoteId, Channel channel, DateTime lastSyncedAt)
    {
        _channelCache.AddOrUpdate(remoteId, new CachedItem<Channel>(channel, lastSyncedAt));
    }

    private async Task UpsertVideoAsync(DataBaseContext dbContext, VideoEntity? existingEntity, Video video, CancellationToken cancellationToken)
    {
        try
        {
            // First, ensure channel exists
            int? channelId = null;
            if (video.Channel is not null)
            {
                var channelEntity = await dbContext.Channels
                    .FirstOrDefaultAsync(c => c.RemoteId == video.Channel.RemoteId, cancellationToken);

                if (channelEntity is null)
                {
                    channelEntity = ChannelEntityMapper.ToEntity(video.Channel);
                    dbContext.Channels.Add(channelEntity);
                    await dbContext.SaveChangesAsync(cancellationToken);
                }

                channelId = channelEntity.Id;
            }

            int videoId;
            if (existingEntity is not null)
            {
                VideoEntityMapper.UpdateEntity(existingEntity, video, channelId);
                videoId = existingEntity.Id;
            }
            else
            {
                var newEntity = VideoEntityMapper.ToEntity(video, channelId);
                dbContext.Videos.Add(newEntity);
                await dbContext.SaveChangesAsync(cancellationToken);
                videoId = newEntity.Id;
            }

            // Upsert streams - delete old ones and add new ones
            await UpsertStreamsAsync(dbContext, videoId, video, cancellationToken);

            _logger.LogDebug("Video upserted to database: {RemoteId}", video.RemoteId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert video to database: {RemoteId}", video.RemoteId);
            // Don't rethrow - we still have the data from provider
        }
    }

    private async Task UpsertStreamsAsync(DataBaseContext dbContext, int videoId, Video video, CancellationToken cancellationToken)
    {
        // Get existing streams for this video
        var existingStreams = await dbContext.Streams
            .Where(s => s.VideoId == videoId)
            .ToListAsync(cancellationToken);

        // Delete all existing streams first to avoid duplicate key issues
        // Stream URLs expire anyway, so we always want fresh data
        if (existingStreams.Count > 0)
        {
            dbContext.Streams.RemoveRange(existingStreams);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        // Combine all streams from the video (only those with itag)
        // Use GroupBy to ensure uniqueness by itag (same itag might appear in both adaptive and combined)
        var allStreams = video.AdaptiveStreams
            .Concat(video.CombinedStreams)
            .Where(s => s.Itag.HasValue) // Only save streams with itag for unique constraint
            .GroupBy(s => s.Itag!.Value)
            .Select(g => g.First()) // Take first stream per itag
            .ToList();

        // Add all new streams
        foreach (var stream in allStreams)
        {
            var newStream = StreamEntityMapper.ToEntity(stream, videoId);
            dbContext.Streams.Add(newStream);
        }

        // Save the new streams
        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Upserted {StreamCount} streams for video {VideoId}", 
            allStreams.Count, 
            video.RemoteId);
    }

    private async Task UpsertChannelAsync(DataBaseContext dbContext, ChannelEntity? existingEntity, Channel channel, CancellationToken cancellationToken)
    {
        try
        {
            ChannelEntity channelEntity;

            if (existingEntity is not null)
            {
                ChannelEntityMapper.UpdateEntity(existingEntity, channel);
                channelEntity = existingEntity;
            }
            else
            {
                channelEntity = ChannelEntityMapper.ToEntity(channel);
                dbContext.Channels.Add(channelEntity);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            // Upsert avatars
            if (channel.Avatars.Count > 0)
            {
                await UpsertChannelImagesAsync(
                    dbContext, 
                    channelEntity, 
                    channel.Avatars, 
                    isAvatar: true, 
                    cancellationToken);
            }

            // Upsert banners
            if (channel.Banners.Count > 0)
            {
                await UpsertChannelImagesAsync(
                    dbContext, 
                    channelEntity, 
                    channel.Banners, 
                    isAvatar: false, 
                    cancellationToken);
            }

            _logger.LogDebug("Channel upserted to database: {RemoteId}", channel.RemoteId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert channel to database: {RemoteId}", channel.RemoteId);
            // Don't rethrow - we still have the data from provider
        }
    }

    private async Task UpsertChannelImagesAsync(
        DataBaseContext dbContext,
        ChannelEntity channelEntity,
        IReadOnlyList<Image> images,
        bool isAvatar,
        CancellationToken cancellationToken)
    {
        // Deduplicate images by URL to avoid duplicate key errors
        var uniqueImages = images
            .GroupBy(i => i.RemoteUrl.ToString())
            .Select(g => g.First())
            .ToList();

        var processedImageIds = new HashSet<int>();

        foreach (var thumbnailInfo in uniqueImages)
        {
            var remoteUrl = thumbnailInfo.RemoteUrl.ToString();

            // Check memory cache first
            if (_imageCache.TryGet(remoteUrl, out var cached) && 
                !IsStale(cached.LastSyncedAt, _config.ImageStalenessThreshold))
            {
                _logger.LogDebug("Image found in memory cache during upsert: {RemoteUrl}", remoteUrl);
            }

            // Find or create the image entity by RemoteUrl
            var imageEntity = await dbContext.Images
                .FirstOrDefaultAsync(i => i.RemoteUrl == remoteUrl, cancellationToken);

            if (imageEntity is null)
            {
                imageEntity = new ImageEntity
                {
                    RemoteUrl = remoteUrl,
                    Width = thumbnailInfo.Width,
                    Height = thumbnailInfo.Height,
                    CreatedAt = DateTime.UtcNow
                };
                dbContext.Images.Add(imageEntity);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            // Skip if we already processed this image in this batch
            if (!processedImageIds.Add(imageEntity.Id))
            {
                continue;
            }

            // Check if mapping already exists
            if (isAvatar)
            {
                var existingMap = await dbContext.Set<ChannelAvatarMapEntity>()
                    .AnyAsync(m => m.ChannelId == channelEntity.Id && m.ImageId == imageEntity.Id, cancellationToken);

                if (!existingMap)
                {
                    dbContext.Set<ChannelAvatarMapEntity>().Add(new ChannelAvatarMapEntity
                    {
                        ChannelId = channelEntity.Id,
                        ImageId = imageEntity.Id
                    });
                }
            }
            else
            {
                var existingMap = await dbContext.Set<ChannelBannerMapEntity>()
                    .AnyAsync(m => m.ChannelId == channelEntity.Id && m.ImageId == imageEntity.Id, cancellationToken);

                if (!existingMap)
                {
                    dbContext.Set<ChannelBannerMapEntity>().Add(new ChannelBannerMapEntity
                    {
                        ChannelId = channelEntity.Id,
                        ImageId = imageEntity.Id
                    });
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertVideoThumbnailsAsync(
        DataBaseContext dbContext,
        VideoEntity videoEntity,
        IReadOnlyList<Image> thumbnails,
        CancellationToken cancellationToken)
    {
        // Deduplicate thumbnails by URL to avoid duplicate key errors
        var uniqueThumbnails = thumbnails
            .GroupBy(t => t.RemoteUrl.ToString())
            .Select(g => g.First())
            .ToList();

        var processedImageIds = new HashSet<int>();

        foreach (var thumbnailInfo in uniqueThumbnails)
        {
            var remoteUrl = thumbnailInfo.RemoteUrl.ToString();

            // Check memory cache first
            if (_imageCache.TryGet(remoteUrl, out var cached) && 
                !IsStale(cached.LastSyncedAt, _config.ImageStalenessThreshold))
            {
                _logger.LogDebug("Image found in memory cache during video thumbnail upsert: {RemoteUrl}", remoteUrl);
            }

            // Find or create the image entity by RemoteUrl
            var imageEntity = await dbContext.Images
                .FirstOrDefaultAsync(i => i.RemoteUrl == remoteUrl, cancellationToken);

            if (imageEntity is null)
            {
                imageEntity = new ImageEntity
                {
                    RemoteUrl = remoteUrl,
                    Width = thumbnailInfo.Width,
                    Height = thumbnailInfo.Height,
                    CreatedAt = DateTime.UtcNow
                };
                dbContext.Images.Add(imageEntity);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            // Skip if we already processed this image in this batch
            if (!processedImageIds.Add(imageEntity.Id))
            {
                continue;
            }

            // Check if mapping already exists
            var existingMap = await dbContext.Set<VideoThumbnailMapEntity>()
                .AnyAsync(m => m.VideoId == videoEntity.Id && m.ImageId == imageEntity.Id, cancellationToken);

            if (!existingMap)
            {
                dbContext.Set<VideoThumbnailMapEntity>().Add(new VideoThumbnailMapEntity
                {
                    VideoId = videoEntity.Id,
                    ImageId = imageEntity.Id
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _httpClient.Dispose();
        _disposed = true;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Image Caching
    // ─────────────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<CachedImage?> GetImageAsync(Uri originalUrl, Uri fetchUrl, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(originalUrl);
        ArgumentNullException.ThrowIfNull(fetchUrl);

        var remoteUrl = originalUrl.ToString();
        _logger.LogDebug("GetImageAsync called for: {RemoteUrl}", remoteUrl);

        // 1. Check memory cache (keyed by original URL)
        if (_imageCache.TryGet(remoteUrl, out var cached))
        {
            _logger.LogDebug("Image cache hit for: {RemoteUrl}", remoteUrl);

            if (!IsStale(cached.LastSyncedAt, _config.ImageStalenessThreshold))
            {
                _logger.LogDebug("Image is fresh, returning cached: {RemoteUrl}", remoteUrl);
                return cached.Data;
            }

            _logger.LogDebug("Image is stale, will refresh: {RemoteUrl}", remoteUrl);
        }

        // 2. Check database by RemoteUrl
        await using var dbContext = await CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.Images
            .FirstOrDefaultAsync(i => i.RemoteUrl == remoteUrl, cancellationToken);

        if (entity?.Data is not null && entity.Data.Length > 0)
        {
            _logger.LogDebug("Image found in database with data: {RemoteUrl}", remoteUrl);

            if (!IsStale(entity.LastSyncedAt ?? DateTime.MinValue, _config.ImageStalenessThreshold))
            {
                // Fresh in DB - put in cache and return
                var cachedFromDb = new CachedImage
                {
                    Data = entity.Data,
                    MimeType = entity.MimeType ?? "image/jpeg",
                    Width = entity.Width,
                    Height = entity.Height,
                    LastSyncedAt = entity.LastSyncedAt ?? DateTime.UtcNow
                };
                PutImageInCache(remoteUrl, cachedFromDb, entity.LastSyncedAt ?? DateTime.UtcNow);
                return cachedFromDb;
            }

            _logger.LogDebug("Image in DB is stale, will refresh: {RemoteUrl}", remoteUrl);
        }
        else if (entity is not null)
        {
            _logger.LogDebug("Image metadata found in database, but no binary data yet: {RemoteUrl}", remoteUrl);
        }

        // 3. Fetch from web using the fetch URL (may be provider proxy)
        var fetchedImage = await FetchImageFromWebAsync(fetchUrl, cancellationToken);

        if (fetchedImage is null)
        {
            _logger.LogWarning("Failed to fetch image from web: {RemoteUrl} via {FetchUrl}", remoteUrl, fetchUrl);
            
            // Return stale data if available
            if (entity?.Data is not null && entity.Data.Length > 0)
            {
                return new CachedImage
                {
                    Data = entity.Data,
                    MimeType = entity.MimeType ?? "image/jpeg",
                    Width = entity.Width,
                    Height = entity.Height,
                    LastSyncedAt = entity.LastSyncedAt ?? DateTime.UtcNow
                };
            }

            return null;
        }

        // 4. Put in cache immediately so UI gets the image fast
        PutImageInCache(remoteUrl, fetchedImage, DateTime.UtcNow);

        // 5. Upsert to database in the background - don't wait for it to complete
        _ = Task.Run(async () =>
        {
            try
            {
                _logger.LogDebug("Background: About to upsert image to database: {RemoteUrl}", remoteUrl);
                
                // Create new context for the background save operation
                await using var saveContext = await CreateDbContextAsync(CancellationToken.None);
                var saveEntity = await saveContext.Images
                    .FirstOrDefaultAsync(i => i.RemoteUrl == remoteUrl, CancellationToken.None);
                
                await UpsertImageAsync(saveContext, saveEntity, fetchedImage, remoteUrl, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background: Failed to save image to database: {RemoteUrl}", remoteUrl);
            }
        }, CancellationToken.None);

        _logger.LogDebug("Image fetched and cached: {RemoteUrl}", remoteUrl);
        return fetchedImage;
    }

    /// <inheritdoc />
    public void InvalidateImage(Uri originalUrl)
    {
        ArgumentNullException.ThrowIfNull(originalUrl);

        var remoteUrl = originalUrl.ToString();
        _imageCache.TryRemove(remoteUrl);
        _logger.LogDebug("Image cache invalidated: {RemoteUrl}", remoteUrl);
    }

    private async Task<CachedImage?> FetchImageFromWebAsync(Uri imageUrl, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Fetching image from web: {ImageUrl}", imageUrl);

            using var response = await _httpClient.GetAsync(imageUrl, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch image, status: {StatusCode}, url: {ImageUrl}", 
                    response.StatusCode, imageUrl);
                return null;
            }

            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";

            // Extract actual dimensions from image bytes
            var (width, height) = GetImageDimensions(data);

            return new CachedImage
            {
                Data = data,
                MimeType = contentType,
                Width = width,
                Height = height,
                LastSyncedAt = DateTime.UtcNow
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Image fetch cancelled: {ImageUrl}", imageUrl);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching image from web: {ImageUrl}", imageUrl);
            return null;
        }
    }

    /// <summary>
    /// Extracts image dimensions from raw image bytes by parsing headers.
    /// Supports JPEG, PNG, GIF, WebP, and BMP formats.
    /// </summary>
    private (int? Width, int? Height) GetImageDimensions(byte[] data)
    {
        if (data.Length < 24)
        {
            return (null, null);
        }

        try
        {
            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
            {
                // Width at bytes 16-19, Height at bytes 20-23 (big-endian)
                var width = (data[16] << 24) | (data[17] << 16) | (data[18] << 8) | data[19];
                var height = (data[20] << 24) | (data[21] << 16) | (data[22] << 8) | data[23];
                return (width, height);
            }

            // JPEG: FF D8 FF
            if (data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
            {
                return GetJpegDimensions(data);
            }

            // GIF: 47 49 46 38 (GIF8)
            if (data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x38)
            {
                // Width at bytes 6-7, Height at bytes 8-9 (little-endian)
                var width = data[6] | (data[7] << 8);
                var height = data[8] | (data[9] << 8);
                return (width, height);
            }

            // WebP: 52 49 46 46 ... 57 45 42 50 (RIFF....WEBP)
            if (data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46 &&
                data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50)
            {
                return GetWebPDimensions(data);
            }

            // BMP: 42 4D (BM)
            if (data[0] == 0x42 && data[1] == 0x4D && data.Length >= 26)
            {
                // Width at bytes 18-21, Height at bytes 22-25 (little-endian)
                var width = data[18] | (data[19] << 8) | (data[20] << 16) | (data[21] << 24);
                var height = Math.Abs(data[22] | (data[23] << 8) | (data[24] << 16) | (data[25] << 24));
                return (width, height);
            }

            return (null, null);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to extract image dimensions from bytes");
            return (null, null);
        }
    }

    private static (int? Width, int? Height) GetJpegDimensions(byte[] data)
    {
        var i = 2;
        while (i < data.Length - 9)
        {
            if (data[i] != 0xFF)
            {
                return (null, null);
            }

            var marker = data[i + 1];

            // SOF markers (Start of Frame) contain dimensions
            // SOF0 (0xC0) through SOF3 (0xC3), SOF5-SOF7, SOF9-SOF11, SOF13-SOF15
            if ((marker >= 0xC0 && marker <= 0xC3) ||
                (marker >= 0xC5 && marker <= 0xC7) ||
                (marker >= 0xC9 && marker <= 0xCB) ||
                (marker >= 0xCD && marker <= 0xCF))
            {
                if (i + 9 < data.Length)
                {
                    // Height at offset +5 (2 bytes, big-endian)
                    // Width at offset +7 (2 bytes, big-endian)
                    var height = (data[i + 5] << 8) | data[i + 6];
                    var width = (data[i + 7] << 8) | data[i + 8];
                    return (width, height);
                }
            }

            // Skip to next marker
            if (marker == 0xD8 || marker == 0xD9 || (marker >= 0xD0 && marker <= 0xD7))
            {
                // Standalone markers
                i += 2;
            }
            else
            {
                // Markers with length
                if (i + 3 >= data.Length)
                {
                    return (null, null);
                }

                var length = (data[i + 2] << 8) | data[i + 3];
                i += 2 + length;
            }
        }

        return (null, null);
    }

    private static (int? Width, int? Height) GetWebPDimensions(byte[] data)
    {
        if (data.Length < 30)
        {
            return (null, null);
        }

        // VP8 (lossy): starts at offset 12
        if (data[12] == 0x56 && data[13] == 0x50 && data[14] == 0x38 && data[15] == 0x20)
        {
            // VP8 bitstream, dimensions at offset 26-29
            if (data.Length >= 30)
            {
                var width = (data[26] | (data[27] << 8)) & 0x3FFF;
                var height = (data[28] | (data[29] << 8)) & 0x3FFF;
                return (width, height);
            }
        }

        // VP8L (lossless): starts at offset 12
        if (data[12] == 0x56 && data[13] == 0x50 && data[14] == 0x38 && data[15] == 0x4C)
        {
            if (data.Length >= 25)
            {
                // Signature byte at 20, then packed width/height
                var bits = data[21] | (data[22] << 8) | (data[23] << 16) | (data[24] << 24);
                var width = (bits & 0x3FFF) + 1;
                var height = ((bits >> 14) & 0x3FFF) + 1;
                return (width, height);
            }
        }

        // VP8X (extended): starts at offset 12
        if (data[12] == 0x56 && data[13] == 0x50 && data[14] == 0x38 && data[15] == 0x58)
        {
            if (data.Length >= 30)
            {
                // Canvas width at bytes 24-26 (little-endian, +1)
                // Canvas height at bytes 27-29 (little-endian, +1)
                var width = (data[24] | (data[25] << 8) | (data[26] << 16)) + 1;
                var height = (data[27] | (data[28] << 8) | (data[29] << 16)) + 1;
                return (width, height);
            }
        }

        return (null, null);
    }

    private async Task UpsertImageAsync(
        DataBaseContext dbContext,
        ImageEntity? existingEntity,
        CachedImage image,
        string remoteUrl,
        CancellationToken cancellationToken)
    {
        try
        {
            if (existingEntity is not null)
            {
                _logger.LogDebug("Updating existing image entity with binary data: {RemoteUrl}, EntityId: {EntityId}", 
                    remoteUrl, existingEntity.Id);
                
                // Update existing entity with binary data (never change RemoteUrl)
                existingEntity.Data = image.Data;
                existingEntity.LastSyncedAt = DateTime.UtcNow;

                // Only update MimeType if we have a valid value from the fetched image
                if (!string.IsNullOrEmpty(image.MimeType))
                {
                    existingEntity.MimeType = image.MimeType;
                }

                // Always overwrite dimensions with actual values extracted from image bytes
                if (image.Width.HasValue)
                {
                    existingEntity.Width = image.Width;
                }

                if (image.Height.HasValue)
                {
                    existingEntity.Height = image.Height;
                }

                // Ensure the entity is tracked as modified
                dbContext.Entry(existingEntity).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }
            else
            {
                _logger.LogDebug("Creating new image entity with binary data: {RemoteUrl}", remoteUrl);
                
                // Create new entity
                var newEntity = new ImageEntity
                {
                    RemoteUrl = remoteUrl,
                    Data = image.Data,
                    MimeType = image.MimeType,
                    Width = image.Width,
                    Height = image.Height,
                    CreatedAt = DateTime.UtcNow,
                    LastSyncedAt = DateTime.UtcNow
                };
                dbContext.Images.Add(newEntity);
            }

            var savedCount = await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Image upserted to database: {RemoteUrl}, SavedCount: {SavedCount}", remoteUrl, savedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert image to database: {RemoteUrl}", remoteUrl);
            // Don't rethrow - we still have the data in memory
        }
    }

    private void PutImageInCache(string remoteUrl, CachedImage image, DateTime lastSyncedAt)
    {
        var cachedItem = new CachedItem<CachedImage>(image, lastSyncedAt);
        _imageCache.AddOrUpdate(remoteUrl, cachedItem);
    }
}
