using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;
using TMS.Apps.FrontTube.Backend.Repository.Interfaces;
using TMS.Apps.FrontTube.Backend.Repository.Models;
using TMS.Apps.FrontTube.Backend.Repository.DataBase;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;

namespace TMS.Apps.FrontTube.Backend.Repository.Data.Tools;

internal class DatabaseSynchronizer
{
    private readonly DataBaseContextPool _pool;

    private readonly ILogger<DatabaseSynchronizer> _logger;

    private readonly CacheHelper _cacheManager;

    private readonly CacheHelper _cacheHelper;

    private ConcurrentDictionary<string, ICommonContract> _commons = [];

    public bool IsSynchronizing { get; private set; }

    public DatabaseSynchronizer(
        DataBaseContextPool pool,
        CacheHelper cacheHelper,
        ILoggerFactory loggerFactory)
    {
        _pool = pool;
        _cacheHelper = cacheHelper;
        _logger = loggerFactory.CreateLogger<DatabaseSynchronizer>();
        //_cacheHelper = new CacheHelper(_pool, _cacheManager, loggerFactory);

    }

    private string ToKey(ICommonContract common)
    {
        var cacheable = common as ICacheableCommon;

        if (cacheable != null)
        {
            if (cacheable.IsMetaData)
            {
                return $"{cacheable.RemoteIdentity.Hash}m";
            }
            else
            {
                return cacheable.RemoteIdentity.Hash.ToString();
            }
        }

        if (common is VideosPageCommon videosPage)
        {
            return $"{videosPage.ChannelRemoteIdentity.Hash}p"; // TODO: fix
        }

        throw new InvalidOperationException("Unsupported common contract type for queuing.");
    }

    public void Enqueue(ICommonContract common)
    {
        var key = ToKey(common);

        // we always prefer full common over metadata common

        if (key.EndsWith("m"))
        {
            // metadata common
            if (_commons.TryGetValue(key[..^1], out var existingCommon))
            {
                // full common already queued, skip
                return;
            }
        }
        else if (!key.EndsWith("p"))
        {
            // we received full common, remove metadata common if exists
            if (_commons.TryGetValue($"{key}m", out var existingMetaCommon))
            {
                // remove metadata common
                _commons.TryRemove($"{key}m", out _);
            }
        }

        if (!_commons.TryAdd(ToKey(common), common))
        {
            _commons[ToKey(common)] = common;
        }

        _logger.LogDebug("Totally {Count} commons queued for synchronization.", _commons.Count);
    }

    public bool TryGetQueued<T>(long hash, bool isMetaData, out ICommonContract? common)
    where T : ICommonContract
    {
        string key = string.Empty;

        var t = typeof(T);
        if (t == typeof(VideoCommon) || t == typeof(VideoMetadataCommon)
            || t == typeof(ChannelCommon)
            || t == typeof(ChannelMetadataCommon))
        {
            key = isMetaData ? $"{hash}m" : hash.ToString();
        }
        else if (t == typeof(VideosPageCommon))
        {
            key = $"{hash}p"; // TODO: fix
        }
        else
        {
            throw new InvalidOperationException("Unsupported common contract type for queuing.");
        }

        return _commons.TryGetValue(key, out common);

    }

    private async Task<List<CacheResult<ChannelEntity>>> SyncChannelsMetadataAsync(DataBaseContext db, List<ChannelMetadataCommon> channelsMetadata, CancellationToken cancellationToken)
    {
        var channelCacheResults = await _cacheHelper.InvalidateCachedAsync<ChannelEntity>(
            channelsMetadata,
            db,
            cancellationToken,
            null);

        // invalidate all avatars for these channels
        await _cacheHelper.InvalidateImagesCacheAsync<ChannelEntity, ChannelAvatarMapEntity, ChannelMetadataCommon>(
            channelCacheResults,
            c => c.Avatars,
            db,
            cancellationToken);

        return channelCacheResults;
    }

    private async Task SyncChannelsAsync(DataBaseContext db, List<ChannelCommon> channels, CancellationToken cancellationToken)
    {
        var channelCacheResults = await SyncChannelsMetadataAsync(
            db,
            channels.Cast<ChannelMetadataCommon>().ToList(),
            cancellationToken);

        // invalidate all banners for these channels
        await _cacheHelper.InvalidateImagesCacheAsync<ChannelEntity, ChannelBannerMapEntity, ChannelCommon>(
            channelCacheResults,
            c => c.Banners,
            db,
            cancellationToken);
    }

    private async Task<List<CacheResult<VideoEntity>>> SyncVideosMetadataAsync(DataBaseContext db, List<VideoBaseCommon> videosBase, CancellationToken cancellationToken)
    {
        var videoCacheResults = await _cacheHelper.InvalidateCachedAsync<VideoEntity>(
            videosBase,
            db,
            cancellationToken,
            null);

        // sync video channels metadata
        var channelCacheResults = await SyncChannelsMetadataAsync(
            db,
            videosBase
                .Select(v => v.Channel)
                .DistinctBy(c => c.RemoteIdentity.Hash)
                .Cast<ChannelMetadataCommon>()
                .ToList(),
            cancellationToken);

        // link videos to their channels by common
        _ = videoCacheResults
            .Join(
                channelCacheResults,
                v => ((VideoBaseCommon)v.Common!).Channel.RemoteIdentity.Hash,
                c => ((ChannelMetadataCommon)c.Common!).RemoteIdentity.Hash,
                (v, c) =>
                {
                    v.Entity!.Channel = c.Entity!;

                    return (v, c);
                })
            .ToList();

        // invalidate all video thumbnails
        await _cacheHelper.InvalidateImagesCacheAsync<VideoEntity, VideoThumbnailMapEntity, VideoBaseCommon>(
            videoCacheResults,
            v => v.Thumbnails,
            db,
            cancellationToken);

        return videoCacheResults;
    }

    // a function to log all detected changes in db context change tracker
    private void LogChanges(DataBaseContext db)
    {
        var entries = db.ChangeTracker.Entries()
            .Where(e => e.State != Microsoft.EntityFrameworkCore.EntityState.Unchanged)
            .ToList();

        if (entries.Count == 0)
        {
            _logger.LogDebug("No changes detected in the DbContext change tracker.");
            return;
        }

        foreach (var entry in entries)
        {
            _logger.LogDebug("Entity: {Entity}, State: {State}", entry.Entity.GetType().Name, entry.State);
        }
    }

    private async Task SyncVideosAsync(DataBaseContext db, List<VideoCommon> videos, CancellationToken cancellationToken)
    {
        var videoCacheResults = await SyncVideosMetadataAsync(
            db,
            videos.Cast<VideoBaseCommon>().ToList(),
            cancellationToken);

        // invalidate all video streams
        // collect video streams
        var videoStreams = videoCacheResults
            .Select(v => (VideoCacheResult: v, StreamsCommons:
                ((VideoCommon)v.Common!).MutexStreams
                    .Concat(((VideoCommon)v.Common!).AdaptiveStreams)
                    .ToList()))
            .ToList();

        // clean upp all streams in db, since they are always changing
        var videosHashes = videoStreams
            .Select(vs => vs.VideoCacheResult.Common!.RemoteIdentity.Hash)
            .ToList();
        var streamsToRemove = db.Streams.Where(s => videosHashes.Contains(s.Video.Hash));
        db.Streams.RemoveRange(streamsToRemove);        

        await _cacheHelper.InvalidateCachedAsync<StreamEntity>(
            videoStreams
                .SelectMany(vs => vs.StreamsCommons)
                .ToList(),
                db,
            cancellationToken,
            s => s.Entity!.Video = videoStreams
                .Single(vs => vs.StreamsCommons.Contains(((StreamMetadataCommon)s.Common!))).VideoCacheResult.Entity!);

        // invalidate all captions
        // collect video captions
        var videoCaptions = videoCacheResults
            .Select(v => (VideoCacheResult: v, CaptionsCommons:
                ((VideoCommon)v.Common!).Captions
                    .ToList()))
            .ToList();

        await _cacheHelper.InvalidateCachedAsync<CaptionEntity>(
            videoCaptions
                .SelectMany(vc => vc.CaptionsCommons)
                .ToList(),
            db,
            cancellationToken,
            c => c.Entity!.Video = videoCaptions
                .Single(vc => vc.CaptionsCommons.Contains(((CaptionMetadataCommon)c.Common!))).VideoCacheResult.Entity!);

    }

    private async Task SyncVideosPagesAsync(DataBaseContext db, List<VideosPageCommon> videosPages, CancellationToken cancellationToken)
    {
        await SyncVideosMetadataAsync(
            db,
            videosPages
                .SelectMany(vp => vp.Videos)
                .Cast<VideoBaseCommon>()
                .ToList(),
            cancellationToken);
    }

    private int GetSavingPriority(ICommonContract common)
    {
        switch (common)
        {
            case ChannelCommon:
                return 0;
            case ChannelMetadataCommon:
                return 1;
            case VideoCommon:
                return 2;
            case VideoMetadataCommon:
                return 3;
            case VideosPageCommon:
                return 4;
            default:
                throw new InvalidOperationException("Unsupported common contract type for saving priority.");
        }
        
    }

    public async Task SynchronizeAsync(CancellationToken cancellationToken)
    {
        if (IsSynchronizing)
        {
            _logger.LogInformation("Database synchronization is already in progress. Skipping this run.");
            return;
        }

        if (_commons.IsEmpty)
        {
            _logger.LogInformation("No commons queued for synchronization. Skipping.");
            return;
        }

        IsSynchronizing = true;

        _logger.LogInformation("Starting database synchronization...");

        Dictionary<string, ICommonContract> removedCommons = new();

        try
        {

            await using var db = await _pool.GetContextAsync(cancellationToken);

            while (!_commons.IsEmpty)
            {
                // create a snapshot and drain the dictionary
                foreach (var (key, _) in _commons) // safe
                {
                    if (!_commons.TryRemove(key, out var value))
                    {
                        continue;
                    }

                    removedCommons[key] = value;
                }

                // group by type to optimize processing
                var grouped = removedCommons
                    .Select(kv => kv.Value)
                    .GroupBy(common => common.GetType())
                    .Select(g => (type: g.Key, commons: g.ToList()))
                    .OrderBy(g => GetSavingPriority(g.commons.First()))
                    .ToList();

                foreach (var (type, commons) in grouped)
                {
                    switch (type)
                    {
                        case Type t when t == typeof(VideoMetadataCommon):
                            await SyncVideosMetadataAsync(db, commons.Cast<VideoBaseCommon>().ToList(), cancellationToken);
                            break;

                        case Type t when t == typeof(VideoCommon):
                            await SyncVideosAsync(db, commons.Cast<VideoCommon>().ToList(), cancellationToken);
                            break;

                        case Type t when t == typeof(ChannelMetadataCommon):
                            await SyncChannelsMetadataAsync(db, commons.Cast<ChannelMetadataCommon>().ToList(), cancellationToken);
                            break;

                        case Type t when t == typeof(ChannelCommon):
                            await SyncChannelsAsync(db, commons.Cast<ChannelCommon>().ToList(), cancellationToken);
                            break;

                        case Type t when t == typeof(VideosPageCommon):
                            await SyncVideosPagesAsync(db, commons.Cast<VideosPageCommon>().ToList(), cancellationToken);
                            break;
                    }

                }
            }

            LogChanges(db);

            var count = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Synchronized {Count} entities to database.", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database synchronization.");

            // re-enqueue removed items for next run
            foreach (var kvp in removedCommons)
            {
                _commons.TryAdd(kvp.Key, kvp.Value);
            }
        }
        finally
        {
            IsSynchronizing = false;
        }
    }
}
