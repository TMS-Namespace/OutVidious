using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;
using TMS.Apps.FrontTube.Backend.Repository.Enums;
using TMS.Apps.FrontTube.Backend.Repository.Models;
using TMS.Apps.FrontTube.Backend.Repository.DataBase;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;
using Microsoft.EntityFrameworkCore;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Repository.Mappers;
using TMS.Apps.FrontTube.Backend.Repository.CacheManager.Tools;
using TMS.Apps.FrontTube.Backend.Repository.Tools;

namespace TMS.Apps.FrontTube.Backend.Repository.Data.Tools;

internal class CacheHelper
{
    private readonly ILogger<CacheHelper> _logger;

    private readonly Func<ICacheableEntity, bool> _isStaleFunc;

    public CacheHelper(
        Func<ICacheableEntity, bool> isStaleCallBack,
        ILoggerFactory loggerFactory)
    {
        _isStaleFunc = isStaleCallBack;
        _logger = loggerFactory.CreateLogger<CacheHelper>();
    }

    public async Task<
        IReadOnlyList<(
            CacheResult<TEntity> CacheResult,
            IReadOnlyList<CacheResult<ImageEntity>> ImagesCacheResults,
            IReadOnlyList<TMapingEntity> NewMaps)>
            >
    InvalidateImagesCacheAsync<TEntity, TMapingEntity, TParentCommon>(
        List<CacheResult<TEntity>> parentsCacheResults,
        Func<TParentCommon, IReadOnlyList<ImageMetadataCommon>> imagesCommonsSelector,
        DataBaseContext dataBaseContext,
        CancellationToken cancellationToken)
        where TEntity : class, ICacheableEntity
        where TMapingEntity : class, IImageMap
        where TParentCommon : ICacheableCommon
    {
        var parentsCacheResultsWithCommonImages = parentsCacheResults
            .Select(r => (r, (IReadOnlyList<ICacheableCommon>)imagesCommonsSelector(((TParentCommon)(r.Common!))).DistinctBy(img => img.RemoteIdentity.Hash).ToList()))
            .ToList();

        var results = await InvalidateImagesCacheAsync<TEntity, TMapingEntity>(
            parentsCacheResultsWithCommonImages,
            dataBaseContext,
            cancellationToken);

        return results;
    }

    /// <summary>
    /// Ensures that the images common metadata are cached, and creates mapping entities to the parent entity.
    /// This overload is for the case when images belong to different parent entities.
    /// </summary>
    public async Task<
        IReadOnlyList<(
            CacheResult<TEntity> CacheResult,
            IReadOnlyList<CacheResult<ImageEntity>> ImagesCacheResults,
            IReadOnlyList<TMapingEntity> NewMaps)>
            >
    InvalidateImagesCacheAsync<TEntity, TMapingEntity>(
        IReadOnlyList<(CacheResult<TEntity> CacheResult, IReadOnlyList<ICacheableCommon> ImagesCommon)> parentsCacheResultsWithCommonImages,
        DataBaseContext dataBaseContext,
        CancellationToken cancellationToken)
        where TEntity : class, ICacheableEntity
        where TMapingEntity : class, IImageMap
    {
        var goodParentsCacheResultWithImages = parentsCacheResultsWithCommonImages
            .Where(r => r.CacheResult.Status != EntityStatus.Error);

        if (!goodParentsCacheResultWithImages.Any()) // all parents have errors
        {
            return parentsCacheResultsWithCommonImages.Select(r =>
                (r.CacheResult,
                (IReadOnlyList<CacheResult<ImageEntity>>)[], (IReadOnlyList<TMapingEntity>)[]))
                .ToList();
        }

        // the parents that has error, we do not process their images, and just add empty lists for function return
        // also no maps are created for them
        var cacheResultsWithImagesAndMaps = parentsCacheResultsWithCommonImages
            .Where(r =>
                r.CacheResult.Status == EntityStatus.Error)
            .Select(r =>
                    (r.CacheResult,
                    (IReadOnlyList<CacheResult<ImageEntity>>)[], (IReadOnlyList<TMapingEntity>)[])
                )
            .ToList();

        // for good ones, we invalidate those in bulk for performance
        var allGoodImagesCommon = goodParentsCacheResultWithImages
            .SelectMany(r => r.ImagesCommon)
            .Distinct()
            .ToList();

        var imagesCacheResults = await InvalidateCachedAsync<ImageEntity>(
            allGoodImagesCommon,
            dataBaseContext,
            cancellationToken,
            null);

        //AddToContainer(imagesCacheResults.Where(r => r.Status is EntityStatus.New or EntityStatus.Existed).ToList());

        // check for duplicates
        DuplicateCheck(
            imagesCacheResults
                .Select(c => (Entity: ((IEntity)c.Entity!), Status: c.Status)),
            "Bulk invalidated cached Images");

        foreach (var goodParentCacheResultWithImages in goodParentsCacheResultWithImages)
        {
            // get current parent's images new cache results
            var parentNewImagesCacheResults = imagesCacheResults
                .Where(ci => goodParentCacheResultWithImages.ImagesCommon.Contains(ci.Common))
                .Where(ci => ci.Status == EntityStatus.New)
                .ToList();

            // create maps
            var maps = CreateMapsToImages<TEntity, TMapingEntity>(
                goodParentCacheResultWithImages.CacheResult,
                parentNewImagesCacheResults,
                dataBaseContext,
                cancellationToken);

            cacheResultsWithImagesAndMaps.Add((goodParentCacheResultWithImages.CacheResult, parentNewImagesCacheResults, maps));

            cancellationToken.ThrowIfCancellationRequested();
        }

        return cacheResultsWithImagesAndMaps;
    }

    public async Task<List<CacheResult<T>>> InvalidateCachedAsync<T>(
        IReadOnlyList<ICacheableCommon> commons,
        DataBaseContext dataBaseContext,
        CancellationToken cancellationToken,
        Action<CacheResult<T>>? parentSetter)
        where T : class, ICacheableEntity
    {
        var cacheResults = await GetLocallyAsync<T>(
            commons,
            dataBaseContext,
            cancellationToken);

        // set parent for new entities
        var newCacheResults = cacheResults
            .Where(r => r.Status == EntityStatus.New);

        if (parentSetter != null)
        {
            newCacheResults.ToList().ForEach(e => parentSetter(e));
        }

        // attach new entities to db context
        newCacheResults
            .Select(r => r.Entity!)
            .ToList()
            .ForEach(e => dataBaseContext.Attach(e));

        return cacheResults;
    }

    private List<TMapingEntity> CreateMapsToImages<TEntity, TMapingEntity>(
        CacheResult<TEntity> cacheResult,
        IReadOnlyList<CacheResult<ImageEntity>> imagesCacheResults,
        DataBaseContext dataBaseContext,
        CancellationToken cancellationToken)
        where TEntity : class, ICacheableEntity
        where TMapingEntity : class, IImageMap
    {
        var videoImagesMaps = new List<TMapingEntity>();

        var newImagesEntities = imagesCacheResults
            .Where(r => r.Status == EntityStatus.New)
            .Select(r => r.Entity!);

        foreach (var imageEntity in newImagesEntities)
        {
            var map = TMapingEntity.Create(
                imageEntity,
                cacheResult.Entity!);

            videoImagesMaps.Add((TMapingEntity)map);

            // attach to db context
            dataBaseContext.Attach(map);

            cancellationToken.ThrowIfCancellationRequested();
        }

        //_entitiesContainer.AddRange(videoImagesMaps.Select(m => (Entity: (IEntity)m, Status: EntityStatus.New)).ToList());

        return videoImagesMaps;
    }

    public void DuplicateCheck(IEnumerable<(IEntity Entity, EntityStatus Status)> entitiesWithStatuses, string caption)
    {

        var cacheableEntitiesHashes = entitiesWithStatuses
                .Select(es => es.Entity)
                .OfType<ICacheableEntity>()
                .Select(e => e.Hash);

        if (cacheableEntitiesHashes.Count() != cacheableEntitiesHashes.Distinct().Count())
        {
            _logger.LogDebug("========= Duplicate Hash Check Start: {Caption} =======", caption);

            // find duplicates
            var duplicateHashes = cacheableEntitiesHashes
                .GroupBy(h => h)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            // log duplicates
            foreach (var duplicateHash in duplicateHashes)
            {
                var duplicateEntities = entitiesWithStatuses
                    .Select(es => es.Entity)
                    .OfType<ICacheableEntity>()
                    .Where(e => e.Hash == duplicateHash)
                    .ToList();

                _logger.LogError("Duplicate hash: {Hash}", duplicateHash);

                foreach (var entity in duplicateEntities)
                {
                    _logger.LogError("Duplicate entities: Type {Type} {@Entity}: ", entity.GetType().Name, entity);

                    // if entity is ImageEntity, log its parent too
                    if (entity is ImageEntity imageEntity)
                    {
                        var parentMaps = entitiesWithStatuses
                            .Select(es => es.Entity)
                            .OfType<IImageMap>()
                            .Where(m => m.Image.Hash == duplicateHash)
                            .ToList();

                        foreach (var map in parentMaps)
                        {
                            _logger.LogError("Parent entity: Type {Type} {@Entity}", map.GetType().Name, map);
                        }
                    }
                }
            }

            _logger.LogDebug("========= Duplicate Hash Check End: {Caption} =======", caption);

            throw new InvalidOperationException("Duplicate hashes found in altered entities container during background save");
        }

        // checking duplicates by IEntity.Id for grouped by type entities
        var entitiesByType = entitiesWithStatuses
            .Select(es => es.Entity)
            .GroupBy(e => e.GetType());


        foreach (var entityGroup in entitiesByType)
        {
            var entityIds = entityGroup
                .Where(e => e.Id != 0) // ignore entities with Id = 0 (not yet assigned)
                .Select(e => e.Id);

            if (entityIds.Count() != entityIds.Distinct().Count())
            {
                _logger.LogDebug("========= Duplicate Id Check Start: {Caption}, Type: {Type} =======", caption, entityGroup.Key.Name);

                // find duplicates
                var duplicateIds = entityIds
                    .GroupBy(id => id)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                // log duplicates
                foreach (var duplicateId in duplicateIds)
                {
                    var duplicateEntities = entityGroup
                        .Where(e => e.Id == duplicateId)
                        .ToList();

                    _logger.LogError("Duplicate Id: {Id}", duplicateId);

                    foreach (var entity in duplicateEntities)
                    {
                        _logger.LogError("Duplicate entities: Type {Type} {@Entity}: ", entity.GetType().Name, entity);
                    }
                }

                _logger.LogDebug("========= Duplicate Id Check End: {Caption}, Type: {Type} =======", caption, entityGroup.Key.Name);

                throw new InvalidOperationException($"Duplicate Ids found in altered entities container during background save for type {entityGroup.Key.Name}");
            }

        }

        _logger.LogDebug("========= Duplicate Check passed: {Caption} =======", caption);
    }

    private (
        List<(RemoteIdentityCommon Identity, ICacheableEntity Entity, ICacheableCommon Common)> UpdatedEntitiesWithCommonAndIdentity,
        List<(RemoteIdentityCommon Identity, ICacheableEntity Entity, ICacheableCommon Common)> CreatedEntitiesWithCommonAndIdentity)
    MapCommonToEntities(
        IReadOnlyList<(RemoteIdentityCommon Identity, ICacheableCommon Common)> commonsWithIdentity,
        IReadOnlyList<ICacheableEntity> existingEntities)
    {
        var updatedEntities = new List<(RemoteIdentityCommon Identity, ICacheableEntity Entity, ICacheableCommon Common)>();
        var createdEntities = new List<(RemoteIdentityCommon Identity, ICacheableEntity Entity, ICacheableCommon Common)>();
        ICacheableEntity? result = null;

        for (int i = 0; i < commonsWithIdentity.Count; i++)
        {
            var commonWithIdentity = commonsWithIdentity[i];
            var existingEntity = existingEntities.SingleOrDefault(fe => fe.Hash == commonWithIdentity.Identity.Hash);

            switch (commonWithIdentity.Common)
            {
                case VideoCommon common:
                    result = CommonToEntityMapper.ToEntity(common, (VideoEntity?)existingEntity);
                    break;
                case VideoMetadataCommon common:
                    result = CommonToEntityMapper.ToEntity(common, (VideoEntity?)existingEntity);
                    break;
                case ChannelCommon common:
                    result = CommonToEntityMapper.ToEntity(common, (ChannelEntity?)existingEntity);
                    break;
                case ChannelMetadataCommon common:
                    result = CommonToEntityMapper.ToEntity(common, (ChannelEntity?)existingEntity);
                    break;
                case ImageMetadataCommon common:
                    result = CommonToEntityMapper.ToEntity(common, (ImageEntity?)existingEntity);
                    break;
                case CaptionMetadataCommon common:
                    result = CommonToEntityMapper.ToEntity(common, (CaptionEntity?)existingEntity);
                    break;
                case StreamMetadataCommon common:
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
        IReadOnlyList<RemoteIdentityCommon> identities,
        DataBaseContext dbContext,
        CancellationToken cancellationToken)
        where T : class, ICacheableEntity
    {
        var hashes = identities.Select(id => id.Hash).ToList();

        if (typeof(T) == typeof(VideoEntity))
        {
            var entities = await dbContext
                .BuildVideosQuery(full: false, noTracking: false)
                .Where(v => hashes.Contains(v.Hash)) // TODO: add watching history?
                .ToListAsync(cancellationToken);

            return entities.Cast<T>().ToList();
        }

        if (typeof(T) == typeof(ChannelEntity))
        {
            var entities = await dbContext
                .BuildChannelsQuery(full: false, noTracking: false)
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
        List<RemoteIdentityCommon> NotSavedIdentities)>
        GetLocallyAsync<T>(
        IReadOnlyList<RemoteIdentityCommon> identities,
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
            .Where(e =>  _isStaleFunc(e))
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

    // /// <summary>
    // /// Used to sync Db with a list of commons that fetched externally, and return the corresponding entities.
    // /// </summary>
    // /// <typeparam name="T"></typeparam>
    // /// <param name="commons"></param>
    // /// <param name="cancellationToken"></param>
    // /// <returns></returns>
    // private async Task<CacheResult<T>>
    //     GetLocallyAsync<T>(
    //     ICacheableCommon common,
    //     DataBaseContext dataBaseContext,
    //     CancellationToken cancellationToken)
    //     where T : class, ICacheableEntity
    // {
    //     var results = await GetLocallyAsync<T>([common], dataBaseContext, cancellationToken);
    //     return results.Single();
    // }

    /// <summary>
    /// Used to sync Db with a list of commons that fetched externally, and return the corresponding entities.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="commons"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<List<CacheResult<T>>>
        GetLocallyAsync<T>(
        IReadOnlyList<ICacheableCommon> commons,
        DataBaseContext dataBaseContext,
        CancellationToken cancellationToken)
        where T : class, ICacheableEntity
    {
        var commonsWithIdentities = commons
            .Select(c => (Identity: c.ToIdentity(), Common: c))
            .ToList();

        var identities = commonsWithIdentities
            .Select(ci => ci.Identity)
            .ToList();

        var (freshEntities, staleEntities, notSavedIdentities) = await GetLocallyAsync<T>(identities, dataBaseContext, cancellationToken);

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
            Repository.Enums.EntityStatus.Existed,
            identities.Single(id => id.Hash == fe.Hash),
            fe,
            commonsWithIdentities.SingleOrDefault(ci => ci.Identity.Hash == fe.Hash).Common,
            null)));

        // add updated entities
        results.AddRange(updatedEntitiesWithCommonAndIdentity.Select(ue => new CacheResult<T>
        (
            Repository.Enums.EntityStatus.Updated,
            ue.Identity,
            (T)ue.Entity,
            ue.Common,
            null)));

        // add created entities
        results.AddRange(createdEntitiesWithCommonAndIdentity.Select(ce => new CacheResult<T>
        (
            Repository.Enums.EntityStatus.New,
            ce.Identity,
            (T)ce.Entity,
            ce.Common,
            null)));

        return results;
    }

}
