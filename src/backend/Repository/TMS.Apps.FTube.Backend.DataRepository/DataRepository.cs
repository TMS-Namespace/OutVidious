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
    private readonly string _connectionString;

    private readonly ICache<string, CachedItem<VideoInfo>> _videoCache;
    private readonly ICache<string, CachedItem<ChannelDetails>> _channelCache;

    private bool _disposed;

    public DataRepository(
        DataRepositoryConfig config,
        ILoggerFactory loggerFactory)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = loggerFactory?.CreateLogger<DataRepository>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        _connectionString = config.DataBase.BuildConnectionString();

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
    /// </summary>
    private FTubeDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<FTubeDbContext>();
        optionsBuilder.UseNpgsql(_connectionString);
        return new FTubeDbContext(optionsBuilder.Options);
    }

    /// <inheritdoc />
    public async Task<VideoInfo?> GetVideoAsync(string remoteId, IVideoProvider provider, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(remoteId);
        ArgumentNullException.ThrowIfNull(provider);

        _logger.LogDebug("GetVideoAsync called for: {RemoteId}", remoteId);

        // 1. Check memory cache
        if (_videoCache.TryGet(remoteId, out var cached))
        {
            _logger.LogDebug("Video cache hit for: {RemoteId}", remoteId);

            if (!IsStale(cached.LastSyncedAt, _config.VideoStalenessThreshold))
            {
                _logger.LogDebug("Video is fresh, returning cached: {RemoteId}", remoteId);
                return cached.Data;
            }

            _logger.LogDebug("Video is stale, will refresh: {RemoteId}", remoteId);
        }

        // 2. Check database
        await using var dbContext = CreateDbContext();
        var entity = await dbContext.Videos
            .Include(v => v.Channel)
            .Include(v => v.Thumbnails).ThenInclude(t => t.Image)
            .Include(v => v.Captions)
            .FirstOrDefaultAsync(v => v.RemoteId == remoteId, cancellationToken);

        if (entity is not null)
        {
            _logger.LogDebug("Video found in database: {RemoteId}", remoteId);

            if (!IsStale(entity.LastSyncedAt, _config.VideoStalenessThreshold))
            {
                var videoInfo = VideoEntityMapper.ToContract(entity);
                PutVideoInCache(remoteId, videoInfo, entity.LastSyncedAt);
                _logger.LogDebug("Video is fresh in DB, returning: {RemoteId}", remoteId);
                return videoInfo;
            }

            _logger.LogDebug("Video is stale in DB, will refresh: {RemoteId}", remoteId);
        }

        // 3. Fetch from provider
        _logger.LogDebug("Fetching video from provider: {RemoteId}", remoteId);
        var providerVideo = await provider.GetVideoInfoAsync(remoteId, cancellationToken);

        if (providerVideo is null)
        {
            _logger.LogWarning("Provider returned null for video: {RemoteId}", remoteId);
            return entity is not null ? VideoEntityMapper.ToContract(entity) : null;
        }

        // 4. Upsert to database
        await UpsertVideoAsync(dbContext, entity, providerVideo, cancellationToken);

        // 5. Put in cache
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
        await using var dbContext = CreateDbContext();
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
        // But we do cache individual videos when they are fetched
        var page = await provider.GetChannelVideosAsync(channelId, tab, continuationToken, cancellationToken);

        if (page?.Videos is not null)
        {
            // Optionally cache video summaries for quick lookup
            // But full VideoInfo requires separate fetch
            _logger.LogDebug("Fetched {Count} videos from channel", page.Videos.Count);
        }

        return page;
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

            if (existingEntity is not null)
            {
                VideoEntityMapper.UpdateEntity(existingEntity, video, channelId);
            }
            else
            {
                var newEntity = VideoEntityMapper.ToEntity(video, channelId);
                dbContext.Videos.Add(newEntity);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Video upserted to database: {RemoteId}", video.VideoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert video to database: {RemoteId}", video.VideoId);
            // Don't rethrow - we still have the data from provider
        }
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
