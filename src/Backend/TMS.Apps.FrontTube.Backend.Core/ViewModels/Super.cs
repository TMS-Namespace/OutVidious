using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;
using TMS.Apps.FrontTube.Backend.Core.Tools;
using TMS.Apps.FrontTube.Backend.Providers.Invidious;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Interfaces;
using TMS.Apps.FrontTube.Backend.Repository.DataBase;

namespace TMS.Apps.FrontTube.Backend.Core.ViewModels;

/// <summary>
/// Top-level ViewModel that manages data repository and other ViewModels.
/// Acts as the central hub for accessing video and channel data.
/// </summary>
public sealed class Super : IDisposable
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<Super> _logger;
    //private readonly ICacheManager _cacheManager;
    private readonly IProvider _videoProvider;
    //private readonly DataBaseContextPool _pool;
    private bool _disposed;

    /// <summary>
    /// Gets the logger factory for creating loggers in child ViewModels.
    /// </summary>
    internal ILoggerFactory LoggerFactory => _loggerFactory;

    /// <summary>
    /// Gets the proxy for video playback, DASH manifests, and image fetching.
    /// </summary>
    public ProxyToProvider Proxy { get; }

    /// <summary>
    /// Gets the application configurations.
    /// UI can modify these to change application behavior.
    /// </summary>
    public Configurations Configurations { get; }

    internal RepositoryManager RepositoryManager {get;}

    /// <summary>
    /// Creates a Super instance with default configurations.
    /// All dependencies are created internally.
    /// </summary>
    public Super(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(httpClientFactory);

        _loggerFactory = loggerFactory;
        _httpClientFactory = httpClientFactory;
        _logger = loggerFactory.CreateLogger<Super>();

        // Initialize configurations with defaults
        Configurations = new Configurations();

        // Create InvidiousVideoProvider (needed for CacheManager)
        _videoProvider = new InvidiousVideoProvider(
            loggerFactory,
            httpClientFactory,
            Configurations.Provider);

        // Create database context pool
        //_pool = new DataBaseContextPool(Configurations.DataBase, Configurations.Cache, loggerFactory);

        // create RepositoryManager
        RepositoryManager = new RepositoryManager(this, _videoProvider);

        // Create Proxy with SSL bypass handler if configured
        Action<HttpClientHandler>? proxyHandlerConfigurator = Configurations.Provider.BypassSslValidation
            ? handler => handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            : null;

        Proxy = new ProxyToProvider(
            this,
            httpClientFactory,
            _videoProvider.BaseUrl,
            proxyHandlerConfigurator);

        _logger.LogDebug("Super initialized with provider: {ProviderType}", _videoProvider.GetType().Name);
    }

    public async Task InitAsync(CancellationToken cancellationToken)
    {
        await RepositoryManager.InitAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the underlying video provider.
    /// </summary>
    public IProvider VideoProvider => _videoProvider;

    /// <summary>
    /// Gets video information by video ID. Constructs the canonical URL to compute hash.
    /// </summary>
    /// <param name="videoId">The video's remote identifier (e.g., YouTube video ID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Video ViewModel wrapping the video data, or null if not found.</returns>
    public async Task<Video?> GetVideoByIdAsync(string videoId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(videoId))
        {
            _logger.LogWarning("GetVideoByIdAsync called with empty videoId");
            return null;
        }

        var absoluteRemoteUrl = YouTubeValidator.BuildVideoUrl(videoId);
        var identity = new CacheableIdentity
        {
            AbsoluteRemoteUrlString = absoluteRemoteUrl.ToString()
        };

        return await RepositoryManager.GetVideoAsync(identity, cancellationToken);
    }


    /// <summary>
    /// Gets channel details by channel ID. Constructs the canonical URL to compute hash.
    /// </summary>
    /// <param name="channelId">The channel's remote identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Channel ViewModel wrapping the channel data, or null if not found.</returns>
    public async Task<Channel?> GetChannelByIdAsync(string channelId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(channelId))
        {
            _logger.LogWarning("GetChannelByIdAsync called with empty channelId");
            return null;
        }

        var absoluteRemoteUrl = YouTubeValidator.BuildChannelUrl(channelId);
        var identity = new CacheableIdentity
        {
            AbsoluteRemoteUrlString = absoluteRemoteUrl.ToString()
        };

        return await  RepositoryManager.GetChannelAsync(identity, cancellationToken);
    }

    /// <summary>
    /// Gets a page of videos from a channel.
    /// </summary>
    /// <param name="absoluteRemoteUrl">The channel's absolute remote URL.</param>
    /// <param name="tab">The tab to fetch from.</param>
    /// <param name="continuationToken">Pagination token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A page of video summaries.</returns>
    public async Task<VideosPage?> GetChannelVideosAsync(
        Uri absoluteRemoteUrl,
        string tab,
        string? continuationToken,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Getting channel videos: {Url}, tab: {Tab}, hasContinuation: {HasContinuation}",
            absoluteRemoteUrl,
            tab,
            continuationToken is not null);

        var identities = new CacheableIdentity
        {
            AbsoluteRemoteUrlString = absoluteRemoteUrl.ToString()
        };

        return await RepositoryManager.GetChannelsPageAsync(
            identities,
            tab,
            continuationToken,
            cancellationToken);
    }

    /// <summary>
    /// Gets the provider fetch URL for an original image URL.
    /// </summary>
    /// <param name="originalUrl">The original YouTube CDN URL.</param>
    /// <returns>The provider-specific fetch URL.</returns>
    public Uri GetImageFetchUrl(Uri originalUrl)
    {
        return Proxy.ProxyImageRemoteUrl(originalUrl);
    }

    /// <summary>
    /// Builds a complete image proxy URL for use in HTML img tags.
    /// The proxy will cache the image using originalUrl as the key and fetch via fetchUrl.
    /// </summary>
    /// <param name="originalUrl">The original YouTube CDN URL.</param>
    /// <returns>A URL to the local image proxy endpoint.</returns>
    public string BuildImageProxyUrl(Uri originalUrl)
    {
        var fetchUrl = Proxy.ProxyImageRemoteUrl(originalUrl);
        var encodedOriginalUrl = Uri.EscapeDataString(originalUrl.ToString());
        var encodedFetchUrl = Uri.EscapeDataString(fetchUrl.ToString());
        return $"/api/ImageProxy?originalUrl={encodedOriginalUrl}&fetchUrl={encodedFetchUrl}";
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        //_cacheManager.Dispose();
        _videoProvider.Dispose();

        _disposed = true;
        _logger.LogDebug("Super disposed");
    }
}
