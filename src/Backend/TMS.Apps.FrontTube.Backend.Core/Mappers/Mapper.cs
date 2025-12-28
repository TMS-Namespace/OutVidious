using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMS.Apps.FrontTube.Backend.Core.Tools;
using TMS.Apps.FrontTube.Backend.Core.ViewModels;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Enums;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Models;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Tools;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;

// TODO: Remove the passed additional info in the view model constructors, they are already available in the entity

namespace TMS.Apps.FrontTube.Backend.Core.Mappers;
    public static class Mapper
    {
        public static Image ToVM(Super super, CacheResult<ImageEntity> imageCacheResult)
        {
            return new Image(
                super,
                imageCacheResult);
        }

        public static Video ToVM(
            Super super, 
            Channel channel, 
            CacheResult<VideoEntity> videoCacheResult, 
            IReadOnlyList<CacheResult<ImageEntity>> thumbnailsCacheResult)
        {
            return new Video(
                super, 
                channel, 
                videoCacheResult, 
                thumbnailsCacheResult.Select(img => ToVM(super, img)).ToList());
        }

        public static Video ToVM(
            Super super, 
            CacheResult<ChannelEntity> channelCacheResult, 
            CacheResult<VideoEntity> videoCacheResult, 
            IReadOnlyList<CacheResult<ImageEntity>> thumbnailsCacheResult)
        {
            return new Video(
                super, 
                ToVM(super, channelCacheResult, [], []),
                videoCacheResult, 
                thumbnailsCacheResult.Select(img => ToVM(super, img)).ToList());
        }

        public static Channel ToVM(Super super, CacheResult<ChannelEntity> channelCacheResult, IReadOnlyList<CacheResult<ImageEntity>> bannersCacheResult, IReadOnlyList<CacheResult<ImageEntity>> avatarsCacheResult)
        {
            return new Channel(
                super, 
                channelCacheResult, 
                bannersCacheResult.Select(img => ToVM(super, img)).ToList(), 
                avatarsCacheResult.Select(img => ToVM(super, img)).ToList());
        }

        public static VideosPage ToVM(
            Super super, 
            Common.ProviderCore.Contracts.VideosPage pageCommon,
            CacheResult<ChannelEntity> channelCacheResult, 
            IReadOnlyList<(CacheResult<VideoEntity> VideoCacheResult, IReadOnlyList<CacheResult<ImageEntity>> ThumbnailsCacheResult)> videosWithImagesCacheResults)
        {
            var channel = ToVM(super, channelCacheResult, bannersCacheResult: [], avatarsCacheResult: []);

            return new VideosPage(
                super, 
                pageCommon,
                    videosWithImagesCacheResults
                    .Select(v => ToVM(super, channel, v.VideoCacheResult, v.ThumbnailsCacheResult))
                    .ToList(),
                channel
            );
        }

        public static CacheResult<ChannelEntity> ToCacheResult(this ChannelEntity entity)
        {
            return new CacheResult<ChannelEntity>(
                EntityStatus.Existed,
                entity.ToIdentity(),
                entity,
                null,
                null
            );
        }

        public static CacheResult<ImageEntity> ToCacheResult(this ImageEntity entity)
        {
            return new CacheResult<ImageEntity>(
                EntityStatus.Existed,
                entity.ToIdentity(),
                entity,
                null,
                null
            );
        }

    public static void AddCacheResults<T>(this List<IEntity> alteredEntities, List<CacheResult<T>> cacheResults)
    where T : class, ICacheableEntity
    {
        var newAlteredEntities = cacheResults
            .Where(r => r.Status is EntityStatus.New or EntityStatus.Updated)
            .Select(r => r.Entity!);

        alteredEntities.AddRange(newAlteredEntities);
    }

    public static void AddCacheResults<T>(this List<IEntity> alteredEntities, CacheResult<T> cacheResults)
    where T : class, ICacheableEntity
    => alteredEntities.AddCacheResults(new List<CacheResult<T>> { cacheResults });

    public static void AddCacheResults<T>(this ThreadSafeContainer<IEntity> alteredEntities, List<CacheResult<T>> cacheResults)
    where T : class, ICacheableEntity
    {
        var newAlteredEntities = cacheResults
            .Where(r => r.Status is EntityStatus.New or EntityStatus.Updated)
            .Select(r => r.Entity!);

        alteredEntities.AddRange(newAlteredEntities);
    }

    public static void AddCacheResults<T>(this ThreadSafeContainer<IEntity> alteredEntities, CacheResult<T> cacheResults)
    where T : class, ICacheableEntity
    => alteredEntities.AddCacheResults(new List<CacheResult<T>> { cacheResults });

    }