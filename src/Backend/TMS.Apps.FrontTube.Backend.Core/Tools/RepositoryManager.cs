using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;
using TMS.Apps.FrontTube.Backend.Repository.Cache;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Interfaces;
using TMS.Apps.FrontTube.Backend.Repository.DataBase;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Enums;
using TMS.Apps.FrontTube.Backend.Core.ViewModels;
using TMS.Apps.FrontTube.Backend.Core.Mappers;
using Microsoft.EntityFrameworkCore;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;
using System.Net;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Models;

namespace TMS.Apps.FrontTube.Backend.Core.Tools;
    internal class RepositoryManager
    {
        //private DataBaseContext? _dbContext;
        private readonly DataBaseContextPool _pool;
        private readonly ILogger<RepositoryManager> _logger;

        private readonly ICacheManager _cacheManager;

        private readonly HttpClient _httpClient;

        private readonly Super _super;

        private readonly CacheHelper _cacheHelper;

        //private readonly ThreadSafeContainer<(IEntity, CacheResultType)> _alteredEntitiesContainer = new();

        public IProvider Provider
        {
            get => _cacheManager.Provider;
            set => _cacheManager.Provider = value;
        }

        public RepositoryManager(
        Super super,
        IProvider provider)
        {
            _super = super;


            _pool = new DataBaseContextPool(_super.Configurations.DataBase, _super.LoggerFactory);


            _logger = _super.LoggerFactory.CreateLogger<RepositoryManager>();

            var handler = new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Accept.ParseAdd("image/avif,image/webp,image/apng,image/svg+xml,image/*,*/*;q=0.8");
            _httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
            _httpClient.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
            
            _cacheManager = new CacheManager( _super.Configurations.Cache, _pool, provider, _httpClient, _super.LoggerFactory);

            _cacheHelper = new CacheHelper(_pool, _cacheManager, _super.LoggerFactory);

            //Provider = provider;

            //_dbContext = _pool.GetContext();

            // _dbContext.Database.EnsureCreated();
            // _logger.LogDebug("Database initialized/ensured created");

            // // Seed development data if configured
            // if (_super.Configurations.DataBase.IsDevMode)
            // {
            //     var seeder = new DevModeSeeder(_super.LoggerFactory);
            //     seeder.SeedDevUserAsync(_dbContext, CancellationToken.None).GetAwaiter().GetResult();
            //     _logger.LogDebug("Development user seeded");
            // }
        }

        public async Task InitAsync(CancellationToken cancellationToken)
        {
            var dbContext = _pool.GetContext();
            await dbContext!.Database.EnsureCreatedAsync(cancellationToken);
            _logger.LogDebug("Database initialized/ensured created");

            // Seed development data if configured
            if (_super.Configurations.DataBase.IsDevMode)
            {
                var seeder = new DevModeSeeder(_super.LoggerFactory);
                await seeder.SeedDevUserAsync(dbContext,  cancellationToken);
                _logger.LogDebug("Development user seeded");
            }
        }

    private async Task<(ImageEntity? Entity, bool AlreadyInContainer)> LocateImageEntity(
        CacheableIdentity imageIdentity,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(imageIdentity.AbsoluteRemoteUrlString);

        var dbContext = _pool.GetContext();

        var imageEntity = await dbContext!
            .Images
            .SingleOrDefaultAsync(i => i.Hash == imageIdentity.Hash, cancellationToken);

        if (imageEntity == null)
        {
            // maybe it is in the queue and not yet saved
            imageEntity =  _cacheHelper.FindInContainer(imageIdentity.Hash) as ImageEntity;

            if (imageEntity == null)
            {
                _logger.LogWarning("Image not found in cache, so it will be downloaded, Identity: {@ImageIdentity}",
                    imageIdentity);

                return (null, false);
            }

            return (imageEntity, true);
        }

        return (imageEntity, false);
    }

    private async Task<(byte[]? Data, HttpStatusCode StatusCode)> DownloadDataAsync(string url, CancellationToken cancellationToken)
    {
        var uri = new Uri(url);
        using var response = await _httpClient.GetAsync(uri, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return (null, response.StatusCode);
        }

        var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        
        return (data, response.StatusCode);
    }

    /// <summary>
    /// Downloads image binary contents, and saved it. Assumes that the image metadata is already cached.
    /// </summary>
    public async Task<ImageEntity?> GetImageContentsAsync(
        CacheableIdentity imageIdentity,
        string? providerRedirectedUrl,
        CancellationToken cancellationToken,
        bool autoSave = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(imageIdentity.AbsoluteRemoteUrlString);

        try
        {
            var (imageEntity, isInEntitiesContainer) = await LocateImageEntity(
                imageIdentity,
                cancellationToken);

            if (imageEntity == null)
            {
                _logger.LogWarning("Image entity not found for identity: {@ImageIdentity}", imageIdentity);
                
                // we try to download anyway
                var (data, statusCode) = await DownloadDataAsync(
                    providerRedirectedUrl ?? imageIdentity.AbsoluteRemoteUrlString,
                    cancellationToken);

                if (data == null)
                {
                    _logger.LogWarning("Failed to download image data for missing image entity, status: {StatusCode}, originalUrl: {OriginalUrl}, fetchUrl: {FetchUrl}",
                        statusCode, imageIdentity.AbsoluteRemoteUrlString, providerRedirectedUrl ?? imageIdentity.AbsoluteRemoteUrlString);
                    return null;
                }

                // try to locate again after download, maybe another thread added it meanwhile
                (imageEntity, isInEntitiesContainer) = await LocateImageEntity(
                    imageIdentity,
                    cancellationToken);

                if (imageEntity == null)
                {
                    _logger.LogWarning("Even after downloading is finished, image entity not found for identity: {@ImageIdentity}", imageIdentity);
                    _logger.LogDebug("Creating new dummy image entity for identity: {@ImageIdentity}", imageIdentity);
                    
                    // retrieve image dimensions from binary data
                    var (width, height) = ImageDimensionParser.GetImageDimensions(data);

                    // create new image entity
                    imageEntity = new ImageEntity
                    {
                        Hash = imageIdentity.Hash,
                        AbsoluteRemoteUrl = imageIdentity.AbsoluteRemoteUrlString,
                        Data = data,
                        LastSyncedAt = DateTime.UtcNow,
                        Width = width,
                        Height = height
                    };

                    return imageEntity; // TODO: consider actually saving it
                }

                _logger.LogDebug("Image entity found after download for identity: {@ImageIdentity}", imageIdentity);
                imageEntity.Data = data;
                imageEntity.LastSyncedAt = DateTime.UtcNow;

                if (!isInEntitiesContainer)
                {
                    _cacheHelper.AddToContainer(imageEntity, EntityStatus.Updated);
                }

                _cacheHelper.BackgroundSave(autoSave, cancellationToken);

                return imageEntity;
            }

            var (dataOuter, statusCodeOuter) = await DownloadDataAsync(
                    providerRedirectedUrl ?? imageIdentity.AbsoluteRemoteUrlString,
                    cancellationToken);

            if (dataOuter == null)
            {
                _logger.LogWarning("Failed to download image data for missing image entity, status: {StatusCode}, originalUrl: {OriginalUrl}, fetchUrl: {FetchUrl}",
                    statusCodeOuter, imageIdentity.AbsoluteRemoteUrlString, providerRedirectedUrl ?? imageIdentity.AbsoluteRemoteUrlString);
                return null;
            }

            imageEntity.Data = dataOuter;
            imageEntity.LastSyncedAt = DateTime.UtcNow;

            if (!isInEntitiesContainer)
            {
                _cacheHelper.AddToContainer(imageEntity, EntityStatus.Updated);
            }

            _cacheHelper.BackgroundSave(autoSave, cancellationToken);

            return imageEntity;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in {MethodName} for identity: {@ImageIdentity}", nameof(GetImageContentsAsync), imageIdentity);
            throw;
        }
    }

    public async Task<ViewModels.Video?> GetVideoAsync(
        CacheableIdentity videoIdentity,
        CancellationToken cancellationToken,
        bool autoSave = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(videoIdentity.AbsoluteRemoteUrlString);

        // try
        // {
            //var __alteredEntitiesContainer = new ThreadSafeContainer<IEntity>();

            var videoCacheResult = await _cacheManager.GetGloballyAsync<VideoEntity>(
                videoIdentity,
                cancellationToken,
                false);

            // if this is an existing video, we just return it
            if (videoCacheResult.Status == EntityStatus.Existed)
            {
            _logger.LogDebug("Video found in cache as existing: {@VideoIdentity}", videoIdentity);

                // re-fetch video from DB to include navigation properties
                var db = _pool.GetContext(); // to ensure context is created
                var videoEntity = await db!
                    .Videos
                    .Include(v => v.Thumbnails)
                        .ThenInclude(vt => vt.Image)
                    .Include(v => v.Channel)
                        .ThenInclude(c => c.Avatars)
                            .ThenInclude(ca => ca.Image)
                    .Include(v => v.Channel)
                        .ThenInclude(c => c.Avatars)
                            .ThenInclude(cb => cb.Image)
                    .Include(v => v.Streams)
                    .SingleAsync(v => v.Id == videoCacheResult.Entity!.Id, cancellationToken);

                videoCacheResult = new CacheResult<VideoEntity>(
                    videoCacheResult.Status,
                    videoCacheResult.Identity,
                    videoEntity,
                    videoCacheResult.Common,
                    videoCacheResult.Error);

                return Mapper.ToVM(
                    _super, 
                    videoCacheResult.Entity!.Channel.ToCacheResult(), 
                    videoCacheResult, 
                    videoCacheResult.Entity.Thumbnails.Select(t => t.Image.ToCacheResult()).ToList());
            }

            var channelCacheResult = await _cacheHelper.InvalidateCachedAsync<ChannelEntity>(
                ((Common.ProviderCore.Contracts.Video) videoCacheResult.Common!).Channel,
                cancellationToken,
                null);

            if (videoCacheResult.Status == EntityStatus.Error)
            {
            _logger.LogWarning("Video not found or error caching video: {@VideoIdentity}, Error: {Error}",
                    videoIdentity, videoCacheResult.Error);

                return Mapper.ToVM(_super, channelCacheResult, videoCacheResult, []);
            }

            if (channelCacheResult.Status == EntityStatus.Error)
            {
            _logger.LogWarning("Channel not found or error caching channel for video: {@ChannelIdentity}, Error: {Error}",
                    channelCacheResult.Identity, channelCacheResult.Error);

                return Mapper.ToVM(_super, channelCacheResult, videoCacheResult, []);
            }

            _cacheHelper.AddToContainer(videoCacheResult);

            if (videoCacheResult.Status == EntityStatus.New)
            {
                videoCacheResult.Entity!.Channel = channelCacheResult.Entity!;
            }

            // ensure channel avatars are cached
            await _cacheHelper.InvalidateImagesCacheAsync<ChannelEntity, ChannelAvatarMapEntity>(
                ((Common.ProviderCore.Contracts.ChannelMetadata) channelCacheResult.Common!).Avatars.DistinctBy(a => a.AbsoluteRemoteUrl).ToList(),
                channelCacheResult,
                cancellationToken);

            var videoCommon = (Common.ProviderCore.Contracts.Video) videoCacheResult.Common!;

            // ensure that all images metadata are cached
            var (imagesCacheResults, _) = await _cacheHelper.InvalidateImagesCacheAsync<VideoEntity, VideoThumbnailMapEntity>(
                videoCommon.Thumbnails.DistinctBy(img => img.AbsoluteRemoteUrl).ToList(),
                videoCacheResult,
                cancellationToken);

            // ensure that all streams are cached
            await _cacheHelper.InvalidateCachedAsync<StreamEntity>(
                videoCommon
                    .MutexStreams
                    .Concat(videoCommon.AdaptiveStreams)
                    .DistinctBy(s => s.AbsoluteRemoteUrl)
                    .Cast<ICacheableCommon>()
                    .ToList(),
                cancellationToken,
                (StreamEntity s) => s.Video = videoCacheResult.Entity!);

            // ensure that all captions are cached
            await _cacheHelper.InvalidateCachedAsync<CaptionEntity>(
                videoCommon
                    .Captions
                    .DistinctBy(c => c.AbsoluteRemoteUrl)
                    .Cast<ICacheableCommon>()
                    .ToList(),
                cancellationToken,
                (CaptionEntity c) => c.Video = videoCacheResult.Entity!);

            // map to view model
            var videoViewModel = Mapper.ToVM(
                _super, 
                channelCacheResult,
                videoCacheResult, 
                imagesCacheResults);

            _cacheHelper.BackgroundSave(  autoSave,  cancellationToken);

            return videoViewModel;
        // }
        // catch (Exception ex)
        // {
        //     _logger.LogError(ex, "Unexpected error in {MethodName} for identity: {@VideoIdentity}", nameof(GetVideoAsync), videoIdentity);
        //     throw;
        // }
    }

    public async Task<ViewModels.Channel?> GetChannelAsync(
        CacheableIdentity channelIdentity,
        CancellationToken cancellationToken,
        bool autoSave = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channelIdentity.AbsoluteRemoteUrlString);

        try
        {
            var channelCacheResult = await _cacheManager.GetGloballyAsync<ChannelEntity>(
                channelIdentity,
                cancellationToken,
                false);

            if (channelCacheResult.Status == EntityStatus.Error)
            {
                _logger.LogWarning("Channel not found or error caching channel: {@ChannelIdentity}, Error: {Error}",
                    channelIdentity, channelCacheResult.Error);

                return Mapper.ToVM(_super, channelCacheResult, [], []);
            }

            // if this is an existing channel, we just return it
            if (channelCacheResult.Status == EntityStatus.Existed)
            {
                _logger.LogDebug("Channel found to be existing: {@ChannelIdentity}", channelIdentity);

                // re-fetch channel from DB to include navigation properties
                var db = _pool.GetContext(); // to ensure context is created
                
                var channelEntity = await db!
                    .Channels
                    .Include(c => c.Banners)
                        .ThenInclude(cb => cb.Image)
                    .Include(c => c.Avatars)
                        .ThenInclude(ca => ca.Image)
                    .SingleAsync(c => c.Id == channelCacheResult.Entity!.Id, cancellationToken);

                channelCacheResult = new CacheResult<ChannelEntity>(
                    channelCacheResult.Status,
                    channelCacheResult.Identity,
                    channelEntity,
                    channelCacheResult.Common,
                    channelCacheResult.Error);

                return Mapper.ToVM(
                    _super,
                    channelCacheResult,
                    channelEntity.Banners.Select(b => b.Image.ToCacheResult()).ToList(), 
                    channelEntity.Avatars.Select(a => a.Image.ToCacheResult()).ToList());
            }

            //var _alteredEntitiesContainer = new ThreadSafeContainer<IEntity>();

            // TODO: add if existed check

            _cacheHelper.AddToContainer(channelCacheResult);

            // ensure that banners are cached
            var (bannersCacheResults, _) = await _cacheHelper.InvalidateImagesCacheAsync<ChannelEntity, ChannelBannerMapEntity>(
                ((Common.ProviderCore.Contracts.Channel) channelCacheResult.Common!).Banners.DistinctBy(b => b.AbsoluteRemoteUrl).ToList(),
                channelCacheResult,
                cancellationToken);

            //_logger.LogDebug("Channel Banners: {@Banners}",
            //((Common.ProviderCore.Contracts.Channel) channelCacheResult.Common!).Banners.Select(b => b.AbsoluteRemoteUrl).ToList());

            _cacheHelper.DuplicateCheck(
                bannersCacheResults
                    .Select(icr => ((IEntity)icr.Entity!, icr.Status)),
                $"Channel Banners in {nameof(GetChannelAsync)}");

            // ensure that avatars are cached
            var (avatarsCacheResults, _) = await _cacheHelper.InvalidateImagesCacheAsync<ChannelEntity, ChannelAvatarMapEntity>(
                ((Common.ProviderCore.Contracts.Channel) channelCacheResult.Common).Avatars.DistinctBy(a => a.AbsoluteRemoteUrl).ToList(),
                channelCacheResult,
                cancellationToken);

            _cacheHelper.DuplicateCheck(
                avatarsCacheResults
                    .Select(icr => ((IEntity)icr.Entity!, icr.Status)),
                $"Channel Avatars in {nameof(GetChannelAsync)}");

            // map to view model

            var channelViewModel = Mapper.ToVM(
                _super, 
                channelCacheResult, 
                bannersCacheResults, 
                avatarsCacheResults);

            _cacheHelper.BackgroundSave( autoSave,  cancellationToken);

            return channelViewModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in {MethodName} for identity: {@ChannelIdentity}", nameof(GetChannelAsync), channelIdentity);
            throw;
        }
    } 

   
    /// <summary>
    /// If the channel has no videos, or the channel not found, in both cases returns null.
    /// </summary>
    /// <param name="channelIdentity"></param>
    /// <param name="tab"></param>
    /// <param name="continuationToken"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ViewModels.VideosPage?> GetChannelsPageAsync(
        CacheableIdentity channelIdentity,
        string tab, // TODO: replace with enum
        string? continuationToken,
        CancellationToken cancellationToken,
        bool autoSave = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channelIdentity.AbsoluteRemoteUrlString);

        var remoteChannelId = YouTubeUrlBuilder.ExtractChannelRemoteId(channelIdentity.AbsoluteRemoteUrlString);
        
        if (string.IsNullOrEmpty(remoteChannelId))
        {
            _logger.LogWarning("Could not extract channel ID from URL: {Url}", channelIdentity.AbsoluteRemoteUrlString);
            return null;
        }

        _logger.LogDebug(
            "GetChannelVideosAsync for channel: {ChannelId}, tab: {Tab}, continuation: {HasContinuation}",
            remoteChannelId,
            tab,
            continuationToken is not null);

        var pageCommon = await Provider.GetChannelVideosAsync(remoteChannelId, tab, continuationToken, cancellationToken);

        if (pageCommon?.Videos is not null && pageCommon.Videos.Count > 0)
        {        
            //var _alteredEntitiesContainer = new ThreadSafeContainer<IEntity>();
            
            // ensure channel is cached, we already have its metadata from the channel page fetch 
            //TODO: may got broken if channel page not loaded
            var channelCacheResult = await _cacheHelper.InvalidateCachedAsync<ChannelEntity>(
                pageCommon.Videos[0].Channel,
                cancellationToken,
                null);

                // if (channelCacheResult.Status is EntityStatus.Existed or EntityStatus.Updated)
                // {
                //     _cacheHelper.AddToContainer(channelCacheResult);
                // }

            // ensure that all videos are cached
            var videosCacheResults = await _cacheHelper.InvalidateCachedAsync<VideoEntity>(
                pageCommon.Videos,
                cancellationToken,
                parentSetter: (VideoEntity v) => v.Channel = channelCacheResult.Entity!);

            // _cacheHelper.AddToContainer(videosCacheResults
            //     .Where(vcr => vcr.Status is EntityStatus.Existed or EntityStatus.Updated)
            //     .ToList());

            _cacheHelper.DuplicateCheck(
                videosCacheResults.Select(vcr => ((IEntity)vcr.Entity!, vcr.Status)), 
                $"Videos in {nameof(GetChannelsPageAsync)}");

            // ensure that all images metadata are cached           
            // var imagesCacheResults = await InvalidateCachedAsync<ImageEntity>(
            //     pageCommon
            //         .Videos
            //         .SelectMany(v => v.Thumbnails)
            //         .DistinctBy(img => img.AbsoluteRemoteUrl)
            //         .ToList(),
            //     cancellationToken,
            //     _alteredEntitiesContainer,
            //     null,
            //     false);

            //DuplicateHashCheck(imagesCacheResults.Select(icr => icr.Entity!), 
            //$"Images in {nameof(GetChannelsPageAsync)}");

            // ensure that all images metadata are cached
            // first, group images with images common
            var videosCacheResultsWithCommonImages = videosCacheResults
                .Select(vc => 
                {
                    var imagesCommon = ((Common.ProviderCore.Contracts.VideoMetadata)vc.Common!)
                        .Thumbnails
                        .DistinctBy(img => img.AbsoluteRemoteUrl).ToList();

                    return (CacheResult : vc, ImagesCommon: (IReadOnlyList<ICacheableCommon>)imagesCommon.Cast<ICacheableCommon>().ToList());
                }
                ).ToList();

            var imageMappingResults = await _cacheHelper.InvalidateImagesCacheAsync<VideoEntity, VideoThumbnailMapEntity>(
                videosCacheResultsWithCommonImages,
                cancellationToken);

            _cacheHelper.DuplicateCheck(
                imageMappingResults
                    .SelectMany(r => r.ImagesCacheResults)
                    .Select(icr => ((IEntity)icr.Entity!, icr.Status)),
                $"Mapped Images in {nameof(GetChannelsPageAsync)}");

            // extract cache results with image cache results, for mapping to view model
            var videosCacheResultsWithImageCacheResults = imageMappingResults
                .Select(r => (VideoCacheResult: r.CacheResult, ThumbnailsCacheResult: r.ImagesCacheResults))
                .Distinct()
                .ToList();


            _logger.LogDebug("Fetched {Count} videos from channel", pageCommon.Videos.Count);

            var pageViewModel = Mapper.ToVM(_super, pageCommon!, channelCacheResult, videosCacheResultsWithImageCacheResults);
            
            _cacheHelper.BackgroundSave(autoSave,  cancellationToken);

            return pageViewModel;
        }
        else
        {
            _logger.LogInformation("No videos found for channel ID: {ChannelId}, tab: {Tab}", remoteChannelId, tab);
            return null;    
    }
    }

    
}