using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Configuration;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Interfaces;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Mappers;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Enums;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Models;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Tools;
using TMS.Apps.FrontTube.Backend.Repository.DataBase;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Repository.Cache;

/// <summary>
/// Data and cache manager implementing multi-tier caching: Memory → Database → Provider.
/// It will try to fetch, if not in DB, only <T> entity types, however it will not fetch related entities (e.g., Channel for Video).
/// </summary>
public sealed class CacheManager : ICacheManager
{
    private readonly CacheConfig _config;
    private readonly ILogger<CacheManager> _logger;
    private readonly DataBaseContextPool _pool;
    //private readonly ICache<long, ICacheableEntity> _entityCache;
    private readonly HttpClient _httpClient;
    private bool _disposed;

    /// <inheritdoc />
    public IProvider Provider { get; set; }

    public CacheManager(
        CacheConfig cacheConfig,
        DataBaseContextPool pool,
        IProvider provider,
        HttpClient httpClient,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(cacheConfig);
        ArgumentNullException.ThrowIfNull(pool);
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _config = cacheConfig;
        _pool = pool;
        Provider = provider;
        _logger = loggerFactory.CreateLogger<CacheManager>();

        _httpClient = httpClient;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _httpClient.Dispose();
        // Note: _pool is not disposed here - it's owned by Super
        _disposed = true;
    }

    private bool IsStale(ICacheableEntity domain)
    {
        if (domain.LastSyncedAt is null)
        {
            return true;
        }

        var threshold = domain switch
        {
            VideoEntity => _config.VideoStalenessThreshold,
            ChannelEntity => _config.ChannelStalenessThreshold,
            ImageEntity => _config.ImageStalenessThreshold,
            _ => _config.VideoStalenessThreshold
        };

        return DateTime.UtcNow - domain.LastSyncedAt.Value > threshold;
    }

    private (
        List<(CacheableIdentity Identity, ICacheableEntity Entity, ICacheableCommon Common)> UpdatedEntitiesWithCommonAndIdentity,
        List<(CacheableIdentity Identity, ICacheableEntity Entity, ICacheableCommon Common)> CreatedEntitiesWithCommonAndIdentity)
    MapCommonToEntities(
        IReadOnlyList<(CacheableIdentity Identity, ICacheableCommon Common)> commonsWithIdentity,
        IReadOnlyList<ICacheableEntity> existingEntities)
    {
        var updatedEntities = new List<(CacheableIdentity Identity, ICacheableEntity Entity, ICacheableCommon Common)>();
        var createdEntities = new List<(CacheableIdentity Identity, ICacheableEntity Entity, ICacheableCommon Common)>();
        ICacheableEntity? result = null;

        for (int i = 0; i < commonsWithIdentity.Count; i++)
        {
            var commonWithIdentity = commonsWithIdentity[i];
            var existingEntity = existingEntities.SingleOrDefault(fe => fe.Hash == commonWithIdentity.Identity.Hash);

            switch (commonWithIdentity.Common)
            {
                case Video common:
                    result = CommonToEntityMapper.ToEntity(common, (VideoEntity?)existingEntity);
                    break;
                case VideoMetadata common:
                    result = CommonToEntityMapper.ToEntity(common, (VideoEntity?)existingEntity);
                    break;
                case Channel common:
                    result = CommonToEntityMapper.ToEntity(common, (ChannelEntity?)existingEntity);
                    break;
                case ChannelMetadata common:
                    result = CommonToEntityMapper.ToEntity(common, (ChannelEntity?)existingEntity);
                    break;
                case ImageMetadata common:
                    result = CommonToEntityMapper.ToEntity(common, (ImageEntity?)existingEntity);
                    break;
                case CaptionMetadata common:
                    result = CommonToEntityMapper.ToEntity(common, (CaptionEntity?)existingEntity);
                    break;
                case StreamMetadata common:
                    result = CommonToEntityMapper.ToEntity(common, (StreamEntity?)existingEntity);
                    break;
                // Add cases for other entity types as needed
                default:
                    throw new NotSupportedException($"Entity type {commonWithIdentity.Common.GetType().Name} is not supported common to entity mapping.");
            }

            if (existingEntity is not null)
            {
                updatedEntities.Add((commonWithIdentity.Identity, result, commonWithIdentity.Common));
            }
            else
            {
                createdEntities.Add((commonWithIdentity.Identity, result, commonWithIdentity.Common));
                //_entityCache.AddOrUpdate(result.Hash, result);
            }
        }

        _logger.LogDebug("Updated {Count} entities from common.", updatedEntities.Count);
        _logger.LogDebug("Created {Count} new entities from common.", createdEntities.Count);

        return (UpdatedEntitiesWithCommonAndIdentity: updatedEntities, CreatedEntitiesWithCommonAndIdentity: createdEntities);
    }

    private async Task<List<T>> FetchFromDataBaseAsync<T>(
        IReadOnlyList<CacheableIdentity> identities,
        DataBaseContext dbContext,
        CancellationToken cancellationToken)
        where T : class, ICacheableEntity
    {
        var hashes = identities.Select(id => id.Hash).ToList();

        if (typeof(T) == typeof(VideoEntity))
        {
            var entities = await dbContext.Videos
                // .Include(v => v.Channel).ThenInclude(c => c.Avatars).ThenInclude(a => a.Image)
                // .Include(v => v.Thumbnails).ThenInclude(t => t.Image)
                // .Include(v => v.Captions)
                // .Include(v => v.Streams)
                .Where(v => hashes.Contains(v.Hash)) // TODO: add watching history?
                .ToListAsync(cancellationToken);

            return entities.Cast<T>().ToList();
        }

        if (typeof(T) == typeof(ChannelEntity))
        {
            var entities = await dbContext.Channels
                // .Include(c => c.Avatars).ThenInclude(a => a.Image)
                // .Include(c => c.Banners).ThenInclude(b => b.Image)
                .Where(c => hashes.Contains(c.Hash)) // TODO: add videos?
                .ToListAsync(cancellationToken);

            return entities.Cast<T>().ToList();
        }

        if (typeof(T) == typeof(ImageEntity))
        {
            var entities = await dbContext.Images
                .Where(i => hashes.Contains(i.Hash))
                .ToListAsync(cancellationToken);

            return entities.Cast<T>().ToList();
        }

        if (typeof(T) == typeof(CaptionEntity))
        {
            var entities = await dbContext.Captions
                .Where(i => hashes.Contains(i.Hash))
                .ToListAsync(cancellationToken);

            return entities.Cast<T>().ToList();
        }

        if (typeof(T) == typeof(StreamEntity))
        {
            var entities = await dbContext.Streams
                .Where(i => hashes.Contains(i.Hash))
                .ToListAsync(cancellationToken);

            return entities.Cast<T>().ToList();
        }

        throw new NotSupportedException($"Entity type {typeof(T).Name} is not supported.");
    }

    /// <summary>
    /// Fetches entities from local database/cache and splits them into fresh, stale, and not saved. Does not modify DB.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="identities"></param>
    /// <param name="dbContext"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<(
        List<T> FreshEntities,
        List<T> StaleEntities,
        List<CacheableIdentity> NotSavedIdentities)>
        GetLocallyAsync<T>(
        IReadOnlyList<CacheableIdentity> identities,
        DataBaseContext dbContext,
        CancellationToken cancellationToken)
        where T : class, ICacheableEntity
    {
        var entities = await FetchFromDataBaseAsync<T>(identities, dbContext, cancellationToken);

        var notSavedHashes = identities
            .Select(id => id.Hash)
            .Where(h => !entities.Any(e => e.Hash == h))
            .ToList();

        var staleEntities = entities
            .Where(e => IsStale(e))
            .ToList();

        var freshEntities = entities
            .Except(staleEntities)
            .ToList();

        var NotSavedIdentities = identities
            .Where(id => notSavedHashes.Contains(id.Hash))
            .ToList();

        return (
            FreshEntities: freshEntities.Cast<T>().ToList(),
            StaleEntities: staleEntities.Cast<T>().ToList(),
            NotSavedIdentities: NotSavedIdentities);
    }

    /// <summary>
    /// Used to sync Db with a list of commons that fetched externally, and return the corresponding entities.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="commons"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<CacheResult<T>>
        GetLocallyAsync<T>(
        ICacheableCommon common,
        CancellationToken cancellationToken,
        bool autoSave = true)
        where T : class, ICacheableEntity
    {
        var results = await GetLocallyAsync<T>([common], cancellationToken, autoSave);
        return results.Single();
    }

    /// <summary>
    /// Used to sync Db with a list of commons that fetched externally, and return the corresponding entities.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="commons"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<CacheResult<T>>>
        GetLocallyAsync<T>(
        IReadOnlyList<ICacheableCommon> commons,
        CancellationToken cancellationToken,
        bool autoSave = true)
        where T : class, ICacheableEntity
    {
        var commonsWithIdentities = commons
            .Select(c => (Identity: c.ToIdentity(), Common: c))
            .ToList();

        var identities = commonsWithIdentities
            .Select(ci => ci.Identity)
            .ToList();

        await using var dbContext = await _pool.GetContextAsync(cancellationToken);

        var (freshEntities, staleEntities, notSavedIdentities) = await GetLocallyAsync<T>(identities, dbContext, cancellationToken);

        // isolate commons that has stale entities, or are not saved
        var staleOrNotSavedCommonsWithIdentities = commonsWithIdentities
            .Where(ci => staleEntities.Any(se => se.Hash == ci.Identity.Hash) || notSavedIdentities.Any(nsi => nsi.Hash == ci.Identity.Hash))
            .ToList();

        // update stale entities
        var (updatedEntitiesWithCommonAndIdentity, createdEntitiesWithCommonAndIdentity) = MapCommonToEntities(staleOrNotSavedCommonsWithIdentities, staleEntities);

        // construct results
        var results = new List<CacheResult<T>>();

        // add fresh entities
        results.AddRange(freshEntities.Select(fe => new CacheResult<T>
        (
            Repository.Cache.Enums.EntityStatus.Existed,
            identities.Single(id => id.Hash == fe.Hash),
            fe,
            commonsWithIdentities.SingleOrDefault(ci => ci.Identity.Hash == fe.Hash).Common,
            null)));

        // add updated entities
        results.AddRange(updatedEntitiesWithCommonAndIdentity.Select(ue => new CacheResult<T>
        (
            Repository.Cache.Enums.EntityStatus.Updated,
            ue.Identity,
            (T)ue.Entity,
            ue.Common,
            null)));

        // add created entities
        results.AddRange(createdEntitiesWithCommonAndIdentity.Select(ce => new CacheResult<T>
        (
            Repository.Cache.Enums.EntityStatus.New,
            ce.Identity,
            (T)ce.Entity,
            ce.Common,
            null)));

        // save changes to DB in the background
        if (autoSave)
        {
            var entitiesToAdd = createdEntitiesWithCommonAndIdentity.Select(ue => (T)ue.Entity).ToList();
            var entitiesToUpdate = updatedEntitiesWithCommonAndIdentity.Select(ue => (T)ue.Entity).ToList();
            BackgroundSave(entitiesToAdd, entitiesToUpdate, cancellationToken);
        }

        return results;
    }
    private async Task<(ICacheableCommon? ResultCommon, string? Error)> FetchFromProviderAsync<T>(
        CacheableIdentity identity,
        //ICacheableEntity? parentEntity,
        CancellationToken cancellationToken)
        where T : class, ICacheableEntity
    {
        if (typeof(T) == typeof(VideoEntity))
        {
            var videoId = YouTubeUrlBuilder.ExtractVideoId(identity.AbsoluteRemoteUrlString);

            if (string.IsNullOrEmpty(videoId))
            {
                _logger.LogWarning("Could not extract video ID from URL: {Url}", identity.AbsoluteRemoteUrlString);
                return (null, $"Could not extract video ID from URL: {identity.AbsoluteRemoteUrlString}");
            }

            var videoCommon = await Provider.GetVideoInfoAsync(videoId, cancellationToken);
            if (videoCommon is null)
            {
                return (null, $"Provider returned null for video ID: {videoId}");
            }

            return (videoCommon, null);
        }

        if (typeof(T) == typeof(ChannelEntity))
        {
            var channelId = YouTubeUrlBuilder.ExtractChannelRemoteId(identity.AbsoluteRemoteUrlString);

            if (string.IsNullOrEmpty(channelId))
            {
                _logger.LogWarning("Could not extract channel ID from URL: {Url}", identity.AbsoluteRemoteUrlString);
                return (null, $"Could not extract channel ID from URL: {identity.AbsoluteRemoteUrlString}");
            }

            var channelCommon = await Provider.GetChannelDetailsAsync(channelId, cancellationToken);
            if (channelCommon is null)
            {
                return (null, $"Provider returned null for channel ID: {channelId}");
            }

            return (channelCommon, null);
        }

        throw new NotSupportedException($"Entity type {typeof(T).Name} is not fetchable from providers.");
    }

    /// <summary>
    /// Used to fetch/create missing or stale entities from remote providers. Doesn't touch local database/cache.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="notExistentIdentities"></param>
    /// <param name="staleEntitiesWithIdentities"></param>
    /// <param name="dbContext"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<(
        List<(CacheableIdentity Identity, ICacheableCommon Common)> NewCommonsWithIdentities,
        List<(CacheableIdentity Identity, T Entity, ICacheableCommon Common)> UpdatingCommonsWithIdentitiesAndEntities,
        List<(CacheableIdentity Identity, T? Entity, string Error)> Errors)>
        GetRemotelyAsync<T>(
        IReadOnlyList<CacheableIdentity> notExistentIdentities,
        IReadOnlyList<(CacheableIdentity Identity, T Entity)> staleEntitiesWithIdentities,
        DataBaseContext dbContext,
        CancellationToken cancellationToken) // TODO: add progress reporting
        where T : class, ICacheableEntity
    {
        var results = new List<(CacheableIdentity Identity, ICacheableCommon? Common, string? Error)>();

        // combine identities to fetch
        var allIdentities = staleEntitiesWithIdentities
            .Select(se => se.Identity)
            .Concat(notExistentIdentities)
            .ToList();

        foreach (var identity in allIdentities)
        {
            var result = await FetchFromProviderAsync<T>(identity, cancellationToken);
            _logger.LogDebug("Fetching remotely URL: {Url}", identity.AbsoluteRemoteUrlString);

            results.Add((Identity: identity, Common: result.ResultCommon, result.Error));

            cancellationToken.ThrowIfCancellationRequested();
        }

        // get stale entities only
        var staleEntities = staleEntitiesWithIdentities
            .Select(se => se.Entity)
            .ToList();

        // construct failed results
        var failedResults = results
            .Where(r => r.Common is null)
            .Select(r => (Identity: r.Identity, Entity: staleEntities.SingleOrDefault(se => se.Hash == r.Identity.Hash), r.Error!))
            .ToList();

        // isolate successful results
        var successfulCommonsWithIdentities = results
            .Where(r => r.Common is not null)
            .Select(r => (Identity: r.Identity, Common: r.Common!))
            .ToList();

        // isolate new commons
        var newCommonsWithIdentities = successfulCommonsWithIdentities
            .Where(sc => !staleEntities.Any(se => se.Hash == sc.Identity.Hash))
            .ToList();

        // isolate updating commons
        var updatingCommonsWithIdentitiesAndEntities = successfulCommonsWithIdentities
            .Except(newCommonsWithIdentities)
            .Select(sc => (Identity: sc.Identity, Entity: staleEntities.Single(se => se.Hash == sc.Identity.Hash), sc.Common))
            .ToList();

        return (
            NewCommonsWithIdentities: newCommonsWithIdentities,
            UpdatingCommonsWithIdentitiesAndEntities: updatingCommonsWithIdentitiesAndEntities,
            Errors: failedResults
            );
    }

    /// <summary>
    /// Fetches/creates entities from local database/cache and remote providers. Doesn't save to database.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="identities"></param>
    /// <param name="dbContext"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<(
        List<T> FreshEntities,
        List<(CacheableIdentity Identity, ICacheableCommon Common)> NewCommonsWithIdentities,
        List<(CacheableIdentity Identity, T Entity, ICacheableCommon Common)> UpdatingCommonsWithIdentitiesAndEntities,
        List<(CacheableIdentity Identity, T? Entity, string Error)> Errors)>
        GetGloballyAsync<T>(
        IReadOnlyList<CacheableIdentity> identities,
        DataBaseContext dbContext,
        CancellationToken cancellationToken) // TODO: add progress reporting
        where T : class, ICacheableEntity
    {
        // first get locally
        var (freshEntities, staleEntities, notSavedIdentities) = await GetLocallyAsync<T>(identities, dbContext, cancellationToken);

        // prepare entity lists
        var staleEntitiesWithIdentities = staleEntities
            .Select(e => (Identity: e.ToIdentity(), Entity: e))
            .ToList();

        // then get remotely the not saved ones
        var remoteResults = await GetRemotelyAsync<T>(notSavedIdentities, staleEntitiesWithIdentities, dbContext, cancellationToken);

        return (
            FreshEntities: freshEntities,
            NewCommonsWithIdentities: remoteResults.NewCommonsWithIdentities,
            UpdatingCommonsWithIdentitiesAndEntities: remoteResults.UpdatingCommonsWithIdentitiesAndEntities,
            Errors: remoteResults.Errors
            );
    }

    public async Task<CacheResult<T>>
        GetGloballyAsync<T>(
        CacheableIdentity identity,
        CancellationToken cancellationToken,
        bool autoSave = true)
        where T : class, ICacheableEntity
    {
        var results = await GetGloballyAsync<T>([identity], cancellationToken, autoSave);
        return results.First();
    }

    /// <summary>
    /// Fetches/creates entities from local database/cache and remote providers, and saves them to database.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="identities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<CacheResult<T>>>
        GetGloballyAsync<T>(
        IReadOnlyList<CacheableIdentity> identities,
        CancellationToken cancellationToken,
        bool autoSave = true) // TODO: add progress reporting
        where T : class, ICacheableEntity
    {

        await using var dbContext = await _pool.GetContextAsync(cancellationToken);

        var globalResults = await GetGloballyAsync<T>(identities, dbContext, cancellationToken);

        // prepare lists for mapping
        var newCommonsWithIdentity = globalResults
            .NewCommonsWithIdentities
            .Concat(globalResults.UpdatingCommonsWithIdentitiesAndEntities
                .Select(uc => (uc.Identity, uc.Common)))
            .ToList();

        var existingEntities = globalResults
            .UpdatingCommonsWithIdentitiesAndEntities
            .Select(uc => uc.Entity)
            .ToList();

        // map to entities and add to entity
        var (updatedEntities, createdEntities) = MapCommonToEntities(newCommonsWithIdentity, existingEntities);

        // construct final results
        var finalResults = new List<CacheResult<T>>();

        finalResults.AddRange(
            globalResults.FreshEntities.Select(e =>
                new CacheResult<T>(
                    Status: Repository.Cache.Enums.EntityStatus.Existed,
                    Identity:  identities.Single(id => id.Hash == e.Hash),
                    Entity: (T)e,
                    null,
                    Error: null)));

        finalResults.AddRange(
            createdEntities.Select(e =>
                new CacheResult<T>(
                    Status: Repository.Cache.Enums.EntityStatus.New,
                    Identity: e.Identity,
                    Entity: (T)e.Entity,
                    Common: e.Common,
                    Error: null
                    )));

        finalResults.AddRange(
            updatedEntities.Select(e =>
                new CacheResult<T>(
                    Status: Repository.Cache.Enums.EntityStatus.Updated,
                    Identity: e.Identity,
                    Entity: (T)e.Entity,
                    Common: e.Common,
                    Error: null)));

        finalResults.AddRange(
            globalResults.Errors.Select(e =>
                new CacheResult<T>(
                    Status: Repository.Cache.Enums.EntityStatus.Error,
                    Identity: e.Identity,
                    Entity: (T?)e.Entity,
                    Common: null,
                    Error: e.Error)));

        // Save to database in background
        if (autoSave)
        {
            var entitiesToAdd = createdEntities.Select(e => (T)e.Entity).ToList();
            var entitiesToUpdate = updatedEntities.Select(e => (T)e.Entity).ToList();
            BackgroundSave(entitiesToAdd, entitiesToUpdate, cancellationToken);
        }

        return finalResults;
    }

    private void BackgroundSave<T>(List<T> entitiesToAdd, List<T> entitiesToUpdate, CancellationToken cancellationToken)
    where T : class, ICacheableEntity
    {
        _ = Task.Run(async () =>
        {
            await using var backgroundContext = await _pool.GetContextAsync(cancellationToken);
            
            try
            {
                if (entitiesToAdd.Count > 0)
                {
                    await backgroundContext.Set<T>().AddRangeAsync(entitiesToAdd, cancellationToken);
                }

                if (entitiesToUpdate.Count > 0)
                {
                    backgroundContext.Set<T>().UpdateRange(entitiesToUpdate);
                }

                if (entitiesToAdd.Count > 0 || entitiesToUpdate.Count > 0)
                {
                    await backgroundContext.SaveChangesAsync(cancellationToken);
                    _logger.LogDebug("Background save completed: {AddedCount} added, {UpdatedCount} updated", entitiesToAdd.Count, entitiesToUpdate.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save entities to database (Added: {AddedCount}, Updated: {UpdatedCount})", entitiesToAdd.Count, entitiesToUpdate.Count);
            }
        }, cancellationToken);
    }
}
