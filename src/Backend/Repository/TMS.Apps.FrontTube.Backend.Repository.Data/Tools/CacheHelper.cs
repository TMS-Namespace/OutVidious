using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Interfaces;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Enums;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Models;
using TMS.Apps.FrontTube.Backend.Repository.DataBase;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;
using Microsoft.EntityFrameworkCore;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

namespace TMS.Apps.FrontTube.Backend.Repository.Data.Tools;

public class CacheHelper
{
    private readonly DataBaseContextPool _pool;
    private readonly ILogger<CacheHelper> _logger;

    private readonly ICacheManager _cacheManager;

    private readonly ThreadSafeContainer<(IEntity Entity, EntityStatus Status)> _entitiesContainer = new();

    public CacheHelper(
        DataBaseContextPool pool,
        ICacheManager cacheManager,
        ILoggerFactory loggerFactory)
    {
        _pool = pool;
        _cacheManager = cacheManager;
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
        var cacheResults = await _cacheManager.GetLocallyAsync<T>(
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

        _entitiesContainer.AddRange(videoImagesMaps.Select(m => (Entity: (IEntity)m, Status: EntityStatus.New)).ToList());

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

}
