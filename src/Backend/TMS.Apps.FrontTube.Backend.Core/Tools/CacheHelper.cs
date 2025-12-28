using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Interfaces;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Enums;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Models;
using TMS.Apps.FrontTube.Backend.Repository.DataBase;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TMS.Apps.FrontTube.Backend.Core.Tools;

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


    /// <summary>
    /// Ensures that the images common metadata are cached, and creates mapping entities to the parent entity.
    /// This overload is for the case when all images belong to the same parent entity.
    /// </summary>
    public async Task<(
        IReadOnlyList<CacheResult<ImageEntity>> ImagesCacheResults,
        IReadOnlyList<TMapingEntity> NewMaps)>
    InvalidateImagesCacheAsync<TEntity, TMapingEntity>(
        IReadOnlyList<ICacheableCommon> imagesCommon,
        CacheResult<TEntity> parentCacheResult,
        CancellationToken cancellationToken)
        where TEntity : class, ICacheableEntity
        where TMapingEntity : class, IImageMap
    {

        var parentsCacheResultsWithImages = new List<(CacheResult<TEntity> CacheResult, IReadOnlyList<ICacheableCommon> ImagesCommon)>
        {
            (parentCacheResult, imagesCommon)
        };

        var results = await InvalidateImagesCacheAsync<TEntity, TMapingEntity>(
            parentsCacheResultsWithImages,
            cancellationToken);

        return (results[0].ImagesCacheResults, results[0].NewMaps);

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
            // get current parent's images cache results
            var parentImagesCacheResults = imagesCacheResults
                .Where(ci => goodParentCacheResultWithImages.ImagesCommon.Contains(ci.Common))
                .ToList();

            // create maps
            var maps = CreateMapsToImages<TEntity, TMapingEntity>(
                goodParentCacheResultWithImages.CacheResult,
                parentImagesCacheResults,
                cancellationToken);

            cacheResultsWithImagesAndMaps.Add((goodParentCacheResultWithImages.CacheResult, parentImagesCacheResults, maps));

            cancellationToken.ThrowIfCancellationRequested();
        }

        return cacheResultsWithImagesAndMaps;
    }

    public async Task<CacheResult<T>> InvalidateCachedAsync<T>(
        ICacheableCommon commons,
        CancellationToken cancellationToken,
        Action<T>? parentSetter)
        where T : class, ICacheableEntity
    {
        var cacheResult = await InvalidateCachedAsync<T>(
            new List<ICacheableCommon> { commons },
            cancellationToken,
            parentSetter);

        return cacheResult.First();
    }

    public async Task<List<CacheResult<T>>> InvalidateCachedAsync<T>(
        IReadOnlyList<ICacheableCommon> commons,
        CancellationToken cancellationToken,
        Action<T>? parentSetter)
        where T : class, ICacheableEntity
    {
        var cacheResults = await _cacheManager.GetLocallyAsync<T>(
            commons,
            cancellationToken,
            false);

        // set parent for new entities
        var newEntities = cacheResults
            .Where(r => r.Status == EntityStatus.New)
            .Select(r => r.Entity!);

        if (parentSetter != null)
        {
            newEntities.ToList().ForEach(e => parentSetter(e));
        }

        AddToContainer(cacheResults);

        return cacheResults;
    }

    public void AddToContainer<T>(CacheResult<T> cacheResults)
    where T : class, ICacheableEntity
    => AddToContainer(new List<CacheResult<T>> { cacheResults });

    public void AddToContainer<T>(List<CacheResult<T>> cacheResults)
    where T : class, ICacheableEntity
    {
        var newAlteredEntities = cacheResults
            .Where(r => r.Status is EntityStatus.New or EntityStatus.Updated or EntityStatus.Existed)
            .Select(r => (Entity: (IEntity)r.Entity!, Status: r.Status))
            .ToList();

        _entitiesContainer.AddRange(newAlteredEntities);
    }

    public ICacheableEntity? FindInContainer(long hash)
    => _entitiesContainer
        .TakeSnapshot()
        .Select(es => es.Entity)
        .OfType<ICacheableEntity>()
        .SingleOrDefault(e => e.Hash == hash);

    public void AddToContainer(IEntity entity, EntityStatus status)
    {
        _entitiesContainer.Add((entity, status));
    }

    private List<TMapingEntity> CreateMapsToImages<TEntity, TMapingEntity>(
        CacheResult<TEntity> cacheResult,
        IReadOnlyList<CacheResult<ImageEntity>> imagesCacheResults,
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

//  public void BackgroundSave2(bool save, CancellationToken cancellationToken)
//     {
//         _ = Task.Run(async () =>
//         {
//             var entitiesContainerWithStatuses = _entitiesContainer.ExtractAll();

//             foreach (var (entity, state) in entitiesContainerWithStatuses)
//             {
//                 ((TrackableEntitiesBase)entity).TrackingState = state switch
//                 {
//                     EntityStatus.New => TrackingState.Added,
//                     EntityStatus.Updated => TrackingState.Modified,
//                     EntityStatus.Existed => TrackingState.Unchanged,
//                     _ => TrackingState.Unchanged
//                 };
//             }

//             await using var backgroundContext = await _pool.GetContextAsync(cancellationToken);

//             try
//             {

//             backgroundContext.ApplyChanges(entitiesContainerWithStatuses
//                 .Select(es => es.Entity)
//                 .Cast<ITrackable>()
//                 .ToList());

//                 // TODO: add save?
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Failed to save {Count} entities to database.", entitiesContainerWithStatuses.Count);
//             }

//         }, cancellationToken);
//     }

    // TODO: add a param for a threshold of items count to actually save (image loading)
    public void BackgroundSave(bool save, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var entitiesContainerWithStatuses = _entitiesContainer.ExtractAll();

            // a check that we have distinct hashes for all CacheableEntity entities
            DuplicateCheck(entitiesContainerWithStatuses, "Entities Container");

            var existedEntities = entitiesContainerWithStatuses
                .Where(es => es.Status == EntityStatus.Existed)
                .Select(e => e.Entity)
                .ToList();

            _logger.LogDebug("Found {Count} existent entities.", existedEntities.Count);

            var updatedEntities = entitiesContainerWithStatuses
                .Where(es => es.Status == EntityStatus.Updated)
                .Select(e => e.Entity)
                .ToList();

            _logger.LogDebug("Found {Count} entities to update.", updatedEntities.Count);

            var newEntities = entitiesContainerWithStatuses
                .Where(es => es.Status == EntityStatus.New)
                .Select(e => e.Entity)
                .ToList();

            _logger.LogDebug("Found {Count} new entities to add.", newEntities.Count);

            if (updatedEntities.Count == 0 && newEntities.Count == 0)
            {
                return;
            }

            await using var backgroundContext = await _pool.GetContextAsync(cancellationToken);

            try
            {
                // if (existedEntities.Count > 0)
                // {
                //     //backgroundContext.AttachRange(existedEntities);
                //     int safelyAttachedCount = 0;

                //     existedEntities.Reverse(); // a hacky way to make E1.Parent = E2 attached before E2. Good solution is more compilcated...

                //     foreach (var entity in existedEntities)
                //     {
                //         if (SafeAttach(backgroundContext, entity))
                //         {
                //             safelyAttachedCount++;
                //         }
                //     }

                //     if (safelyAttachedCount != existedEntities.Count)
                //     {
                //         _logger.LogDebug("Safely attached {Count} (out of {Total}) existing entities.", safelyAttachedCount, existedEntities.Count);
                //     }
                // }

                var updatedEntitiesSafe = updatedEntities;//.Except(GetRelatedEntries(backgroundContext, updatedEntities)).ToList();

                if (updatedEntitiesSafe.Count > 0)
                {
                    backgroundContext.UpdateRange(updatedEntitiesSafe);
                }

                if (newEntities.Count > 0)
                {
                    await backgroundContext.AddRangeAsync(newEntities, cancellationToken);
                }

                UnchangeExistingEntities(backgroundContext, existedEntities);

                if (save)
                {
                    await backgroundContext.SaveChangesAsync(cancellationToken);
                }

                _logger.LogDebug("Background save completed: {Count} entities saved.", entitiesContainerWithStatuses.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save {Count} entities to database.", entitiesContainerWithStatuses.Count);
            }
        }, cancellationToken);
    }

    /// <summary>
    /// We need to set E1.Parent = E2, where E1 is a new entity, and E2 already exist in the DB.
    /// if we do not attach E2, EF will think that it is a new entity, and will try to insert it,
    /// although it has an Id, what causes duplicate primary key error. If we star to attach it 
    /// instead, EF complains that E2 is already tracked, since we attached the E1 too.
    /// So, this function is a work around.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="dbContext"></param>
    /// <param name="entity"></param>
    /// 
    // private bool SafeAttach<TEntity>(DataBaseContext dbContext, TEntity entity)
    //     where TEntity : class, IEntity
    // {
    //     if (entity.Id == 0)
    //     {
    //         throw new ArgumentException("Entity must have a valid Id to be safely attached.", nameof(entity));
    //     }

    //     var existing = dbContext
    //         .ChangeTracker
    //         .Entries()
    //         .FirstOrDefault(e => 
    //             e.Entity.GetType() == entity.GetType() &&
    //             ((IEntity)e.Entity).Id == entity.Id ||
    //             e.Entity.Equals(entity)
    //             );

    //     //var set = db.Set<TEntity>();
    //     //var existing = set.Local.FirstOrDefault(x => x.Id == entity.Id);

    //     if (existing == null)
    //     {
    //         dbContext.Attach(entity);
    //         return true;
    //     }

    //     dbContext.Entry(existing.Entity).State = EntityState.Unchanged;
    //     return false;
        
    // }

    private void UnchangeExistingEntities(DataBaseContext dbContext, List<IEntity> existingEntities)
    {
        if(existingEntities.Count == 0)
        {
            return;
        }

        var trackedExistingEntries = dbContext
            .ChangeTracker
            .Entries()
            .Where(e => 
                existingEntities.Any(existing =>
                    e.Entity.GetType() == existing.GetType() &&
                    ((IEntity)e.Entity).Id == existing.Id ||
                    e.Entity.Equals(existing)
                    )
            );

        foreach (var entry in trackedExistingEntries)
        {
            entry.State = EntityState.Unchanged;
        }
    }

    // public List<IEntity> GetRelatedEntries(
    //     DbContext db,
    //     IEnumerable<IEntity> entities)
    // {
    //     var results = new List<IEntity>();

    //     foreach (var entity in entities)
    //     {
    //         var entityEntry = db.Entry(entity);
            
    //         var flattened = GetRelatedEntries(db, (EntityEntry)entityEntry);

    //         results.AddRange(flattened.Select(e => (IEntity)e.Entity));
    //     }

    //     return results.Distinct().ToList();
    // }

    // private List<EntityEntry> GetRelatedEntries(DbContext db, EntityEntry entityEntry)
    // {
    //     var relatedEntries = new List<EntityEntry>();

    //     foreach (var navigation in entityEntry.Navigations)
    //     {
    //         //if (!nav.IsLoaded)
    //          //   continue; // prevents touching DB (also avoids lazy-loading side-effects)
    //         if (navigation is ReferenceEntry referenceEntry)
    //         {
    //             if (referenceEntry.EntityEntry is null)
    //                 continue;


    //             relatedEntries.Add(referenceEntry.EntityEntry);
    //             relatedEntries.AddRange(GetRelatedEntries(db, referenceEntry.EntityEntry));

    //             continue;
    //         }

    //         if (navigation is CollectionEntry collectionEntry)
    //         {   
    //             if (collectionEntry.CurrentValue is null)
    //                 continue;

    //             foreach (var item in collectionEntry.CurrentValue)
    //             {
    //                 if (item is null)
    //                     continue;

    //                 var itemEntry = db.Entry(item);
    //                 relatedEntries.Add(itemEntry);
    //                 relatedEntries.AddRange(GetRelatedEntries(db, itemEntry));
    //             }

    //             continue;
    //         }

    //         throw new InvalidOperationException("Unknown navigation entry type.");
    //         // foreach (var relatedObject in GetLoadedRelatedEntities(navigation))
    //         // {
    //         //     if (relatedObject is null)
    //         //         continue;

    //         //     var relatedEntry = db.Entry(relatedObject);
    //         //     relatedEntries.Add(relatedEntry);
    //         // }
    //     }

    //     return relatedEntries;
    // } 

    // private static IEnumerable<object?> GetLoadedRelatedEntities(NavigationEntry nav) =>
    //     nav switch
    //     {
    //         CollectionEntry c => Enumerate(c.CurrentValue),
    //         ReferenceEntry r => new[] { r.EntityEntry },
    //         _ => Array.Empty<object?>()
    //     };

    // private static IEnumerable<object?> Enumerate(object? value)
    // {
    //     if (value is null)
    //         yield break;

    //     if (value is IEnumerable enumerable)
    //     {
    //         foreach (var item in enumerable)
    //             yield return item;
    //     }
    // }

}