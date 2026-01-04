using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Core.Mappers;
using TMS.Apps.FrontTube.Backend.Core.Tools;
using TMS.Apps.FrontTube.Backend.Repository.Data;
using TMS.Apps.FrontTube.Backend.Repository.Data.Contracts;

namespace TMS.Apps.FrontTube.Backend.Core.ViewModels;

/// <summary>
/// Top-level ViewModel that manages data repository and other ViewModels.
/// Acts as the central hub for accessing video and channel data.
/// </summary>
public sealed class Super : IDisposable
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<Super> _logger;
    private readonly CancellationTokenSource _repositoryManagerCts = new();
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

    internal RepositoryManager RepositoryManager { get; }

    /// <summary>
    /// Creates a Super instance with default configurations.
    /// All dependencies are created internally.
    /// </summary>
    public Super(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(httpClientFactory);

        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<Super>();

        Configurations = new Configurations();

        RepositoryManager = new RepositoryManager(
            Configurations.DataBase,
            Configurations.Cache,
            Configurations.Provider,
            loggerFactory,
            httpClientFactory);

        var providerBaseUri = Configurations.Provider.BaseUri ?? new Uri("https://youtube.srv1.tms.com");

        Action<HttpClientHandler>? proxyHandlerConfigurator = Configurations.Provider.BypassSslValidation
            ? handler => handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            : null;

        Proxy = new ProxyToProvider(
            this,
            httpClientFactory,
            providerBaseUri,
            proxyHandlerConfigurator);

        _logger.LogDebug("Super initialized with provider base URL: {BaseUrl}", providerBaseUri);
    }

    public async Task InitAsync(CancellationToken cancellationToken)
    {
        await RepositoryManager.InitAsync(cancellationToken, _repositoryManagerCts.Token);
    }

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
        var identity = new IdentityDomain
        {
            AbsoluteRemoteUrlString = absoluteRemoteUrl.ToString()
        };

        var videoDomain = await RepositoryManager.GetVideoAsync(identity, cancellationToken);
        if (videoDomain is null)
        {
            return null;
        }

        if (videoDomain.Channel is null)
        {
            _logger.LogWarning("Video domain missing channel for URL: {Url}", videoDomain.AbsoluteRemoteUrl);
            return null;
        }

        var channelVm = DomainViewModelMapper.ToViewModel(this, videoDomain.Channel);
        return DomainViewModelMapper.ToViewModel(this, videoDomain, channelVm);
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
        var identity = new IdentityDomain
        {
            AbsoluteRemoteUrlString = absoluteRemoteUrl.ToString()
        };

        var channelDomain = await RepositoryManager.GetChannelAsync(identity, cancellationToken);
        return channelDomain is null ? null : DomainViewModelMapper.ToViewModel(this, channelDomain);
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

        var identity = new IdentityDomain
        {
            AbsoluteRemoteUrlString = absoluteRemoteUrl.ToString()
        };

        var pageDomain = await RepositoryManager.GetChannelsPageAsync(
            identity,
            tab,
            continuationToken,
            cancellationToken);

        if (pageDomain is null)
        {
            return null;
        }

        var channelDomain = await RepositoryManager.GetChannelAsync(identity, cancellationToken);
        if (channelDomain is null)
        {
            return null;
        }

        var channelVm = DomainViewModelMapper.ToViewModel(this, channelDomain);
        return DomainViewModelMapper.ToViewModel(this, pageDomain, channelVm);
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

        _repositoryManagerCts.Cancel();
        _repositoryManagerCts.Dispose();

        _disposed = true;
        _logger.LogDebug("Super disposed");
    }
}
