using System.Net;
using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools;
using TMS.Apps.FrontTube.Backend.Providers.Invidious;
using TMS.Apps.FrontTube.Backend.Repository.Data.Contracts;
using TMS.Apps.FrontTube.Backend.Repository.Data.Interfaces;
using TMS.Apps.FrontTube.Backend.Repository.Data.Mappers;
using TMS.Apps.FrontTube.Backend.Repository.Data.Tools;
using TMS.Apps.FrontTube.Backend.Repository.DataBase;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;
using TMS.Apps.FrontTube.Backend.Repository.CacheManager.Tools;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools.YouTube;
using TMS.Apps.FrontTube.Backend.Repository.Data.Enums;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;
using TMS.Apps.FrontTube.Backend.Repository.Tools;


namespace TMS.Apps.FrontTube.Backend.Repository.Data;

public sealed class RepositoryManager
{
    private readonly DataBaseContextPool _pool;
    private readonly ILogger<RepositoryManager> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Data.Contracts.Configuration.DatabaseConfig _databaseConfig;
    //private readonly Data.Contracts.Configuration.CacheConfig _cacheConfig;
    //private readonly ICacheManager _cacheManager;
    private readonly HttpClient _httpClient;
    private readonly CacheHelper _cacheHelper;
    private readonly IProvider _provider;

    private readonly DatabaseSynchronizer _dbSynchronizer;

    private readonly ImageDataSynchronizer _imageDataSynchronizer;

    private PeriodicBackgroundWorker _dbSyncWorker;

    private PeriodicBackgroundWorker _imageDataSyncWorker;

    private readonly Data.Contracts.Configuration.CacheConfig _cacheConfig;

    public RepositoryManager(
        Data.Contracts.Configuration.DatabaseConfig databaseConfig,
        Data.Contracts.Configuration.CacheConfig cacheConfig,
        Data.Contracts.Configuration.ProviderConfig providerConfig,
        ILoggerFactory loggerFactory,
        IHttpClientFactory httpClientFactory)
    {
        ArgumentNullException.ThrowIfNull(databaseConfig);
        ArgumentNullException.ThrowIfNull(cacheConfig);
        ArgumentNullException.ThrowIfNull(providerConfig);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(httpClientFactory);

        _databaseConfig = databaseConfig;
        _loggerFactory = loggerFactory;
        _cacheConfig = cacheConfig;
        _logger = loggerFactory.CreateLogger<RepositoryManager>();

        var commonDbConfig = CommonDomainMapper.FromDomain(databaseConfig);
        var commonCacheConfig = CommonDomainMapper.FromDomain(cacheConfig);
        var commonProviderConfig = CommonDomainMapper.FromDomain(providerConfig);

        _pool = new DataBaseContextPool(commonDbConfig, commonCacheConfig, loggerFactory);

        _provider = new InvidiousVideoProvider(loggerFactory, httpClientFactory, commonProviderConfig);

        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
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

        //_cacheManager = new Repository.Cache.CacheManager(commonCacheConfig, _pool, _provider, _httpClient, loggerFactory);
        _cacheHelper = new CacheHelper(  IsStale, loggerFactory);

        _dbSynchronizer = new DatabaseSynchronizer(_pool, _cacheHelper, loggerFactory);
        _imageDataSynchronizer = new ImageDataSynchronizer(_pool,  loggerFactory);

        _dbSyncWorker = new PeriodicBackgroundWorker(
            TimeSpan.FromSeconds(3),
            _dbSynchronizer.SynchronizeAsync,
            (ex) => _logger.LogError(ex, "Database synchronization worker encountered an error."));
    
        _imageDataSyncWorker = new PeriodicBackgroundWorker(
            TimeSpan.FromSeconds(5),
            _imageDataSynchronizer.SynchronizeAsync,
            (ex) => _logger.LogError(ex, "Image data synchronization worker encountered an error."));

        }

    public async Task InitAsync(CancellationToken initCancellationToken, CancellationToken workerCancellationToken)
    {
        await using var dbContext = await _pool.GetContextAsync(initCancellationToken);
        await dbContext.Database.EnsureCreatedAsync(initCancellationToken);
        _logger.LogDebug("Database initialized/ensured created");

        if (_databaseConfig.IsDevMode)
        {
            var seeder = new DevModeSeeder(_loggerFactory);
            await seeder.SeedDevUserAsync(dbContext, initCancellationToken);
            _logger.LogDebug("Development user seeded");
        }

        _dbSyncWorker.Start(workerCancellationToken);
        _imageDataSyncWorker.Start(workerCancellationToken);

    }

    public bool TryCreateRemoteIdentity(
        string remoteIdentity, 
        RemoteIdentityTypeDomain? expectedIdentityType, 
        [NotNullWhen(true)] out RemoteIdentityDomain? identity, 
        out string? errorMessage)
    {
        if (expectedIdentityType is not null)
        {
            identity = new RemoteIdentityDomain
            {
                IdentityType = expectedIdentityType.Value,
                AbsoluteRemoteUrl = remoteIdentity,
                AbsoluteRemoteUri = new Uri(remoteIdentity, UriKind.RelativeOrAbsolute),
                Hash = HashHelper.ComputeHash(remoteIdentity),
                RemoteId = null
            };

            errorMessage = null;
            return true;
        }

        var isValid = YouTubeIdentityParser.TryParse(remoteIdentity, out var parts);

        if (!isValid)
        {
            errorMessage = $"The provided URL '{remoteIdentity}' is not a valid YouTube URL, because: {string.Join(", ", parts.Errors)}.";
            identity = null;
            return false;
        }

        if (!parts.IsSupported())
        {
            errorMessage = $"The provided URL '{remoteIdentity}' is not supported by FrontTube, the recognized identity type is '{parts.IdentityType}'.";
            identity = null;
            return false;
        }

        var canonicalUrl = parts.ToUrl() ?? parts.AbsoluteRemoteUrl;
        if (canonicalUrl is null)
        {
            errorMessage = $"The provided URL '{remoteIdentity}' could not be normalized to a canonical URL.";
            identity = null;
            return false;
        }

        RemoteIdentityTypeDomain? identityType = null;

        if (parts.IsVideo)
        {
            identityType = RemoteIdentityTypeDomain.Video;
        }
        else if (parts.IsChannel)
        {
            identityType = RemoteIdentityTypeDomain.Channel;
        }

        if (identityType is null)
        {
            errorMessage = $"The provided URL '{remoteIdentity}' has an unsupported identity type '{parts.IdentityType}'.";
            identity = null;
            return false;
        }

        var remoteId = parts.PrimaryRemoteId;
        if (string.IsNullOrWhiteSpace(remoteId))
        {
            errorMessage = $"The provided URL '{remoteIdentity}' does not contain a valid remote ID.";
            identity = null;
            return false;
        }

        var absoluteRemoteUrl = canonicalUrl.ToString();
        identity = new RemoteIdentityDomain
        {
            IdentityType = identityType.Value,
            AbsoluteRemoteUrl = absoluteRemoteUrl,
            AbsoluteRemoteUri = canonicalUrl,
            Hash = HashHelper.ComputeHash(absoluteRemoteUrl),
            RemoteId = remoteId
        };
        errorMessage = null;
        return true;
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
    public async Task<ImageDomain> GetImageContentsAsync(
        RemoteIdentityDomain imageIdentity,
        string? providerRedirectedUrl,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(imageIdentity.AbsoluteRemoteUrl);

        try
        {
            var info = await _imageDataSynchronizer.GetAsync(imageIdentity.Hash, cancellationToken);

            if (info != null)
            {
                _logger.LogDebug("Image data found in synchronized images: {@ImageIdentity}", imageIdentity);

                return new ImageDomain
                {
                    RemoteIdentity = imageIdentity,
                    Data = info.Value.Data,
                    Width = info.Value.Width,
                    Height = info.Value.Height,
                    LastSyncedAt = DateTime.UtcNow
                };
            }

            _logger.LogWarning("Image data not found, fetching for identity: {@ImageIdentity}", imageIdentity);

            var (data, statusCode) = await DownloadDataAsync(
                    providerRedirectedUrl ?? imageIdentity.AbsoluteRemoteUrl,
                    cancellationToken);

            if (data == null || statusCode != HttpStatusCode.OK)
            {
                _logger.LogWarning("Failed to download image data, status: {StatusCode}, originalUrl: {OriginalUrl}, fetchUrl: {FetchUrl}",
                    statusCode, imageIdentity.AbsoluteRemoteUrl, providerRedirectedUrl ?? imageIdentity.AbsoluteRemoteUrl);
                
                return new ImageDomain
                {
                    RemoteIdentity = imageIdentity,
                    Data = null,
                    LastSyncedAt = DateTime.UtcNow,
                    FetchingError = $"Failed to download image data, status: {statusCode}"
                };
            }

            var (width, height) = ImageDimensionParser.GetImageDimensions(data);

            _imageDataSynchronizer.Enqueue(imageIdentity.Hash, width, height, data, DateTime.UtcNow);

            return new ImageDomain
            {
                RemoteIdentity = imageIdentity,
                Data = data,
                Width = width,
                Height = height,
                LastSyncedAt = DateTime.UtcNow                
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in {MethodName} for identity: {@ImageIdentity}", nameof(GetImageContentsAsync), imageIdentity);
            
            return new ImageDomain
            {
                RemoteIdentity = imageIdentity,
                Data = null,
                LastSyncedAt = DateTime.UtcNow,
                FetchingError = $"Unexpected error: {ex.Message}"
            };
        }
    }

    private async Task<VideoDomain?> GetVideoFromProviderAsync(
        RemoteIdentityDomain videoIdentity,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(videoIdentity.AbsoluteRemoteUrl);
        var commonIdentity = CommonDomainMapper.FromDomain(videoIdentity);
        var response = await _provider.GetVideoAsync(commonIdentity, cancellationToken);

        if (response.HasError)
        {
            _logger.LogWarning(
                "[{Method}] Failed to fetch video from remote provider: '{VideoIdentity}'. Response: {Response}",
                nameof(GetVideoFromProviderAsync),
                videoIdentity,
                response);
            return null;
        }

        if (response.Data == null)
        {
            _logger.LogWarning(
                "[{Method}] Video not found from remote provider: '{VideoIdentity}'.",
                nameof(GetVideoFromProviderAsync),
                videoIdentity);
            return null;
        }

        _dbSynchronizer.Enqueue(response.Data);

        return CommonDomainMapper.ToDomain(response.Data);
    }

    private async Task<VideoDomain?> GetVideoFromDataBaseAsync(
        RemoteIdentityDomain videoIdentity,
        CancellationToken cancellationToken)
    {
        await using var db = await _pool.GetContextAsync(cancellationToken);

        var videoEntity = await db
            .BuildVideosQuery( true, true)
            .SingleOrDefaultAsync( v => v.Hash == videoIdentity.Hash, cancellationToken);

        if (videoEntity != null)
        {
            return EntityDomainMapper.ToDomain(videoEntity);
        }

        return null;
    }

    private async Task<ChannelDomain?> GetChannelFromDataBaseAsync(
        RemoteIdentityDomain channelIdentity,
        CancellationToken cancellationToken)
    {
        await using var db = await _pool.GetContextAsync(cancellationToken);

        var channelEntity = await db
            .BuildChannelsQuery(true, true)
            .SingleOrDefaultAsync(c => c.Hash == channelIdentity.Hash, cancellationToken);

        if (channelEntity != null)
        {
            return EntityDomainMapper.ToDomain(channelEntity);
        }

        return null;
    }

    private async Task<ChannelDomain?> GetChannelFromProviderAsync(
        RemoteIdentityDomain channelIdentity,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channelIdentity.AbsoluteRemoteUrl);
        var commonIdentity = CommonDomainMapper.FromDomain(channelIdentity);
        var response = await _provider.GetChannelAsync(commonIdentity, cancellationToken);

        if (response.HasError)
        {
            _logger.LogWarning(
                "[{Method}] Failed to fetch channel from remote provider: '{ChannelIdentity}'. Response: {Response}",
                nameof(GetChannelFromProviderAsync),
                channelIdentity,
                response);
            return null;
        }

        if (response.Data == null)
        {
            _logger.LogWarning(
                "[{Method}] Channel not found from remote provider: '{ChannelIdentity}'.",
                nameof(GetChannelFromProviderAsync),
                channelIdentity);
            return null;
        }

        _dbSynchronizer.Enqueue(response.Data);

        return CommonDomainMapper.ToDomain(response.Data);
    }

    private async Task<VideosPageDomain?> GetChannelPageFromProviderAsync(
        RemoteIdentityDomain channelIdentity,
        ChannelTab tab,
        string? continuationToken,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channelIdentity.AbsoluteRemoteUrl);
        var commonIdentity = CommonDomainMapper.FromDomain(channelIdentity);
        var tabCommon = Common.ProviderCore.Tools.ChannelTabExtensions.ToChannelTabEnum(tab.ToLowerString());
        var response = await _provider.GetChannelVideosTabAsync(
            commonIdentity,
            tabCommon,
            page: null, // Using continuation token pagination
            continuationToken,
            cancellationToken);

        if (response.HasError)
        {
            _logger.LogWarning(
                "[{Method}] Failed to fetch channel videos page from remote provider: '{ChannelIdentity}', Tab: '{Tab}'. Response: {Response}",
                nameof(GetChannelPageFromProviderAsync),
                channelIdentity,
                tab,
                response);
            return null;
        }

        if (response.Data == null)
        {
            _logger.LogWarning(
                "[{Method}] Channel videos page not found from remote provider: '{ChannelIdentity}', Tab: '{Tab}', Continuation: '{HasContinuation}'.",
                nameof(GetChannelPageFromProviderAsync),
                channelIdentity,
                tab,
                continuationToken is not null);
            return null;
        }

        _dbSynchronizer.Enqueue(response.Data);

        return CommonDomainMapper.ToDomain(response.Data);
    }

    private async Task<VideosPageDomain?> GetChannelPageFromDataBaseAsync(
        RemoteIdentityDomain channelIdentity,
        ChannelTab tab,
        string? continuationToken,
        CancellationToken cancellationToken)
    {
        // TODO: make it page and tab aware

        await using var db = await _pool.GetContextAsync(cancellationToken);

        var videos = await db
            .BuildVideosQuery(false, true)
            .Where(v => v.Channel!.Hash == channelIdentity.Hash)
            .ToListAsync(cancellationToken);

        if (videos.Any())
        {
            var pageDomain = new VideosPageDomain
            {
                ChannelRemoteIdentity = channelIdentity,
                Videos = videos
                    .Select(v => EntityDomainMapper.ToDomain(v))
                    .ToList(),
                ContinuationToken = null
            };

            return pageDomain;
        }

        return null;
    }

    public async Task<VideoDomain?> GetVideoAsync(
        RemoteIdentityDomain videoIdentity,
        CancellationToken cancellationToken,
        bool autoSave = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(videoIdentity.AbsoluteRemoteUrl);
        
        // try to get it from synchronization queue
        if (_dbSynchronizer.TryGetQueued<VideoCommon>(videoIdentity.Hash, false, out var queuedCommon))
        {
            _logger.LogDebug("Video found in synchronization queue: {@VideoIdentity}", videoIdentity);
            return CommonDomainMapper.ToDomain((VideoCommon)queuedCommon!);
        }

        // try to get it from DB
        var videoDomain = await GetVideoFromDataBaseAsync(videoIdentity, cancellationToken);

        if (videoDomain != null)
        {
            if (IsStale(videoDomain) || IsStale(videoDomain.Channel!) || videoDomain.Streams.Any(IsStale))
            {
                _logger.LogDebug("Video found in database but is stale, refetching: {@VideoIdentity}", videoIdentity);

                return await GetVideoFromProviderAsync(videoIdentity, cancellationToken);
            }
            
            return videoDomain;
        }

        // nowhere found, fetch from provider
        _logger.LogDebug("Video not found locally, fetching from provider: {@VideoIdentity}", videoIdentity);

        return await GetVideoFromProviderAsync(videoIdentity, cancellationToken);
    }

    public async Task<ChannelDomain?> GetChannelAsync(
        RemoteIdentityDomain channelIdentity,
        CancellationToken cancellationToken,
        bool autoSave = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channelIdentity.AbsoluteRemoteUrl);

        // try to get it from synchronization queue
        if (_dbSynchronizer.TryGetQueued<ChannelCommon>(channelIdentity.Hash, false, out var queuedCommon))        
        {
            _logger.LogDebug("Channel found in synchronization queue: {@ChannelIdentity}", channelIdentity);
            return CommonDomainMapper.ToDomain((ChannelCommon)queuedCommon!);
        }

        // try to get it from DB
        var channelDomain = await GetChannelFromDataBaseAsync(channelIdentity, cancellationToken);
        
        if (channelDomain != null)
        {
            if (IsStale(channelDomain))
            {
                _logger.LogDebug("Channel found in database but is stale, refetching: {@ChannelIdentity}", channelIdentity);

                return await GetChannelFromProviderAsync(channelIdentity, cancellationToken);
            }

            return channelDomain;
        }

        // nowhere found, fetch from provider
        _logger.LogDebug("Channel not found locally, fetching from provider: {@ChannelIdentity}", channelIdentity);
        return await GetChannelFromProviderAsync(channelIdentity, cancellationToken);

    }

    public async Task<VideosPageDomain?> GetChannelsPageAsync(
        RemoteIdentityDomain channelIdentity,
        ChannelTab tab,
        string? continuationToken,
        CancellationToken cancellationToken,
        bool autoSave = true)
    {
        // try to get it from synchronization queue
        if (_dbSynchronizer.TryGetQueued<VideosPageCommon>(channelIdentity.Hash, true, out var queuedCommon))
        {
            _logger.LogDebug("Channel videos page found in synchronization queue: {@ChannelIdentity}, Tab: {Tab}, Continuation: {HasContinuation}",
                channelIdentity, tab, continuationToken is not null);  
 
            return CommonDomainMapper.ToDomain((VideosPageCommon)queuedCommon!);
        }

        // try to get it from DB
        var pageDomain = await GetChannelPageFromDataBaseAsync(
            channelIdentity,
            tab,
            continuationToken,
            cancellationToken);

        if (pageDomain != null)
        {
            if (pageDomain.Videos.Any(v => IsStale(v) || IsStale(v.Channel!)))
            {
                _logger.LogDebug("Channel videos page found in database but is stale, refetching: {@ChannelIdentity}, Tab: {Tab}, Continuation: {HasContinuation}",
                    channelIdentity, tab, continuationToken is not null);  
                
                return await GetChannelPageFromProviderAsync(
                    channelIdentity,
                    tab,
                    continuationToken,
                    cancellationToken);
            }

            return pageDomain;
        }

        // nowhere found, fetch from provider
        _logger.LogDebug("Channel videos page not found locally, fetching from provider: {@ChannelIdentity}, Tab: {Tab}, Continuation: {HasContinuation}",
            channelIdentity, tab, continuationToken is not null);  
        return await GetChannelPageFromProviderAsync(
            channelIdentity,
            tab,
            continuationToken,
            cancellationToken);

    }

    private bool IsStale(ICacheableEntity entity)
    {
        if (entity.LastSyncedAt is null)
        {
            return true;
        }

        var threshold = entity switch
        {
            VideoEntity => _cacheConfig.StalenessConfigs.VideoStalenessThreshold,
            ChannelEntity => _cacheConfig.StalenessConfigs.ChannelStalenessThreshold,
            ImageEntity => _cacheConfig.StalenessConfigs.ImageStalenessThreshold,
            _ => throw new InvalidOperationException("Unknown cacheable domain type.")

        };

        return DateTime.UtcNow - entity.LastSyncedAt.Value > threshold;
    }

    private bool IsStale(ICacheableDomain domain)
    {
        if (domain.LastSyncedAt is null)
        {
            return true;
        }

        var threshold = domain switch
        {
            VideoEntity => _cacheConfig.StalenessConfigs.VideoStalenessThreshold,
            ChannelEntity => _cacheConfig.StalenessConfigs.ChannelStalenessThreshold,
            ImageEntity => _cacheConfig.StalenessConfigs.ImageStalenessThreshold,
            _ => throw new InvalidOperationException("Unknown cacheable domain type.")
        };

        return DateTime.UtcNow - domain.LastSyncedAt.Value > threshold;
    }
}
