using BitFaster.Caching;
using BitFaster.Caching.Lru;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMS.Apps.FTube.Backend.DataBase;
using TMS.Apps.FTube.Backend.DataBase.Entities;
using TMS.Apps.FTube.Backend.DataRepository.Interfaces;
using TMS.Apps.FTube.Backend.DataRepository.Mappers;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Configuration;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Contracts;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Interfaces;

namespace TMS.Apps.FTube.Backend.DataRepository;

/// <summary>
/// Data repository implementing multi-tier caching: Memory → Database → Provider.
/// </summary>
public sealed class DataRepository : IDataRepository
{
    private readonly DataRepositoryConfig _config;
    private readonly ILogger<DataRepository> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly string _connectionString;
    private readonly DevModeSeeder? _devModeSeeder;

    private readonly ICache<string, CachedItem<VideoInfo>> _videoCache;
    private readonly ICache<string, CachedItem<ChannelDetails>> _channelCache;

    private bool _disposed;
    private bool _devUserSeeded;

    public DataRepository(
        DataRepositoryConfig config,
        ILoggerFactory loggerFactory)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _logger = loggerFactory.CreateLogger<DataRepository>();
        _connectionString = config.DataBase.BuildConnectionString();

        // Initialize dev mode seeder if enabled
        if (config.DataBase.IsDevMode)
        {
            _devModeSeeder = new DevModeSeeder(loggerFactory);
        }

        // Initialize caches with expiration
        _videoCache = new ConcurrentLruBuilder<string, CachedItem<VideoInfo>>()
            .WithCapacity(_config.VideoMemoryCacheCapacity)
            .WithExpireAfterWrite(_config.MemoryCacheTtl)
            .Build();

        _channelCache = new ConcurrentLruBuilder<string, CachedItem<ChannelDetails>>()
            .WithCapacity(_config.ChannelMemoryCacheCapacity)
            .WithExpireAfterWrite(_config.MemoryCacheTtl)
            .Build();

        _logger.LogDebug(
            "DataRepository initialized with video staleness: {VideoThreshold}, channel staleness: {ChannelThreshold}",
            _config.VideoStalenessThreshold,
            _config.ChannelStalenessThreshold);
    }

    /// <summary>
    /// Creates a new FTubeDbContext instance using the configured connection string.
    /// Ensures the database schema is created on first use and seeds dev user if enabled.
    /// </summary>
    private async Task<FTubeDbContext> CreateDbContextAsync(CancellationToken cancellationToken)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FTubeDbContext>();
        optionsBuilder.UseNpgsql(_connectionString);
        var context = new FTubeDbContext(optionsBuilder.Options);

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
    /// Creates a new FTubeDbContext instance synchronously.
    /// Ensures the database schema is created on first use.
    /// </summary>
    private FTubeDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<FTubeDbContext>();
        optionsBuilder.UseNpgsql(_connectionString);
        var context = new FTubeDbContext(optionsBuilder.Options);

        // Ensure database schema is created (code-first)
        context.Database.EnsureCreated();

        return context;
    }

    /// <inheritdoc />
    public async Task<VideoInfo?> GetVideoAsync(string remoteId, IVideoProvider provider, CancellationToken cancellationToken)
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
    public async Task<ChannelDetails?> GetChannelAsync(string remoteId, IVideoProvider provider, CancellationToken cancellationToken)
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
    public async Task<ChannelVideoPage?> GetChannelVideosAsync(
        string channelId,
        string tab,
        string? continuationToken,
        IVideoProvider provider,
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

    private async Task UpsertVideoSummariesAsync(string channelId, IReadOnlyList<VideoSummary> videos, CancellationToken cancellationToken)
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
                .FirstOrDefaultAsync(v => v.RemoteId == video.VideoId, cancellationToken);

            if (existingVideo is null)
            {
                // Create new video from summary
                var newEntity = VideoEntityMapper.ToEntity(video, dbChannelId);
                dbContext.Videos.Add(newEntity);
            }
            else
            {
                // Update existing video's basic info if needed
                // Don't overwrite full VideoInfo data with summary data
                if (existingVideo.ChannelId is null && dbChannelId.HasValue)
                {
                    existingVideo.ChannelId = dbChannelId.Value;
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
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

    private void PutVideoInCache(string remoteId, VideoInfo video, DateTime lastSyncedAt)
    {
        _videoCache.AddOrUpdate(remoteId, new CachedItem<VideoInfo>(video, lastSyncedAt));
    }

    private void PutChannelInCache(string remoteId, ChannelDetails channel, DateTime lastSyncedAt)
    {
        _channelCache.AddOrUpdate(remoteId, new CachedItem<ChannelDetails>(channel, lastSyncedAt));
    }

    private async Task UpsertVideoAsync(FTubeDbContext dbContext, VideoEntity? existingEntity, VideoInfo video, CancellationToken cancellationToken)
    {
        try
        {
            // First, ensure channel exists
            int? channelId = null;
            if (video.Channel is not null)
            {
                var channelEntity = await dbContext.Channels
                    .FirstOrDefaultAsync(c => c.RemoteId == video.Channel.ChannelId, cancellationToken);

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

            _logger.LogDebug("Video upserted to database: {RemoteId}", video.VideoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert video to database: {RemoteId}", video.VideoId);
            // Don't rethrow - we still have the data from provider
        }
    }

    private async Task UpsertStreamsAsync(FTubeDbContext dbContext, int videoId, VideoInfo video, CancellationToken cancellationToken)
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
            video.VideoId);
    }

    private async Task UpsertChannelAsync(FTubeDbContext dbContext, ChannelEntity? existingEntity, ChannelDetails channel, CancellationToken cancellationToken)
    {
        try
        {
            if (existingEntity is not null)
            {
                ChannelEntityMapper.UpdateEntity(existingEntity, channel);
            }
            else
            {
                var newEntity = ChannelEntityMapper.ToEntity(channel);
                dbContext.Channels.Add(newEntity);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Channel upserted to database: {RemoteId}", channel.ChannelId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert channel to database: {RemoteId}", channel.ChannelId);
            // Don't rethrow - we still have the data from provider
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        // Caches don't need explicit disposal
        _disposed = true;
    }
}
