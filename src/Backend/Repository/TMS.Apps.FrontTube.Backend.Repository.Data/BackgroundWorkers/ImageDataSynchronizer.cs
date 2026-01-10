using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities.Cache;
using TMS.Apps.FrontTube.Backend.Repository.CacheManager.Tools;

namespace TMS.Apps.FrontTube.Backend.Repository.Data.Tools;

internal class ImageDataSynchronizer
{
    private readonly DataBaseContextPool _pool;

    private readonly ILogger<ImageDataSynchronizer> _logger;

    private ConcurrentDictionary<long, (byte[] Data, int Width, int Height, DateTime SyncedAt)> _imagesToSync = new();

    private ConcurrentDictionary<long, CacheImageEntity> _syncedImages = [];

    public bool IsSynchronizing { get; private set; }

    public ImageDataSynchronizer(
        DataBaseContextPool pool,
        ILoggerFactory loggerFactory)
    {
        _pool = pool;
        //_cacheManager = cacheManager;
        _logger = loggerFactory.CreateLogger<ImageDataSynchronizer>();
        //_cacheHelper = cacheHelper;

    }

    public void Enqueue(long hash, int width, int height, byte[] data, DateTime syncedAt)
    {
        if (!_imagesToSync.TryAdd(hash, (data, width, height, syncedAt)))
        {
            _imagesToSync[hash] = (data, width, height, syncedAt);
        }

        _logger.LogDebug("Totally {Count} images queued for synchronization.", _imagesToSync.Count);
    }

    public async Task<(byte[] Data, int Width, int Height)?> GetAsync(long hash, CancellationToken cancellationToken)
    {
        if (_imagesToSync.TryGetValue(hash, out var imageInfo))
        {
            return (imageInfo.Data, imageInfo.Width, imageInfo.Height);
        }

        if (_syncedImages.TryGetValue(hash, out var syncedImage))
        {
            return (syncedImage.Data!, syncedImage.Width!.Value, syncedImage.Height!.Value);
        }

        await using var db = await _pool.GetContextAsync(cancellationToken);
        var imageEntity = await db.Images.FirstOrDefaultAsync(i => i.Hash == hash, cancellationToken);
    
        if (imageEntity != null && imageEntity.Data != null)
        {
            _syncedImages.TryAdd(hash, imageEntity);

            return (imageEntity.Data!, imageEntity.Width!.Value, imageEntity.Height!.Value);
        }

        return null;
    }

    public async Task SynchronizeAsync(CancellationToken cancellationToken)
    {
        if (IsSynchronizing)
        {
            _logger.LogInformation("Database synchronization is already in progress. Skipping this run.");
            return;
        }

        if (_imagesToSync.IsEmpty)
        {
            _logger.LogInformation("No images queued for synchronization. Skipping.");
            return;
        }

        IsSynchronizing = true;

        _logger.LogInformation("Starting image data synchronization...");

        Dictionary<long, (byte[] Data, int Width, int Height, DateTime SyncedAt)> removedImageData = new();

        try
        {

            await using var db = await _pool.GetContextAsync(cancellationToken);

            List<CacheImageEntity> syncedImageEntities = new();

            var snapshot = _imagesToSync.ToArray();

            var hashesToSync = snapshot.Select(s => s.Key).ToList();

            // if the image is not in the database yet (should be synced by ImageDataSynchronizer), we 
            // skip it now, and will sync its content in next runs

            var existingImages = await db.Images
                .Where(i => hashesToSync.Contains(i.Hash))
                .ToListAsync(cancellationToken);

            var syncData = existingImages
                .Join(snapshot,
                    ei => ei.Hash,
                    si => si.Key,
                    (ei, si) => (Entity: ei, Data: si.Value.Data))
                .ToList();

            foreach (var (imageEntity, data) in syncData)
            {
                if (!_imagesToSync.TryRemove(imageEntity.Hash, out var info))
                {
                    continue;
                }

                removedImageData[imageEntity.Hash] = info;

                imageEntity.Data = data;
                imageEntity.LastSyncedAt = info.SyncedAt;

                syncedImageEntities.Add(imageEntity);
            }

            var count = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Synchronized {Count} entities to database.", count);

            foreach (var imageEntity in syncedImageEntities)
            {
                _syncedImages.TryAdd(imageEntity.Hash, imageEntity);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database synchronization.");

            // re-enqueue removed items for next run
            foreach (var kvp in removedImageData)
            {
                _imagesToSync.TryAdd(kvp.Key, kvp.Value);   
            }
        }
        finally
        {
            IsSynchronizing = false;
        }
    }
}
