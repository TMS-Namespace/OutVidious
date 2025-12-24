using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Repository.Cache;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Interfaces;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Configuration;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;
using TMS.Apps.FrontTube.Backend.Core.Tools;

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
    private readonly ICacheManager _dataRepository;
    private readonly IProvider _videoProvider;
    private readonly bool _ownsDataRepository;
    private bool _disposed;

    /// <summary>
    /// Gets the proxy for video playback, DASH manifests, and image fetching.
    /// </summary>
    public Proxy Proxy { get; }

    /// <summary>
    /// Creates a Super instance with an existing IDataRepository (recommended for shared caching).
    /// </summary>
    public Super(
        ILoggerFactory loggerFactory,
        IHttpClientFactory httpClientFactory,
        IProvider videoProvider,
        ICacheManager dataRepository,
        Action<HttpClientHandler>? proxyHandlerConfigurator = null)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _videoProvider = videoProvider ?? throw new ArgumentNullException(nameof(videoProvider));
        _dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
        _logger = loggerFactory.CreateLogger<Super>();
        _ownsDataRepository = false;

        Proxy = new Proxy(loggerFactory, httpClientFactory, videoProvider.BaseUrl, dataRepository, proxyHandlerConfigurator);

        _logger.LogDebug("Super initialized with shared DataRepository and provider: {ProviderType}", videoProvider.GetType().Name);
    }

    /// <summary>
    /// Gets the underlying video provider.
    /// </summary>
    public IProvider VideoProvider => _videoProvider;

    /// <summary>
    /// Gets video information by remote ID and returns a VideoPlayerViewModel wrapping it.
    /// </summary>
    /// <param name="remoteId">The video's remote identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A VideoPlayerViewModel wrapping the video data, or null if not found.</returns>
    public async Task<Video?> GetVideoByRemoteIdAsync(string remoteId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(remoteId))
        {
            _logger.LogWarning("GetVideoByRemoteIdAsync called with empty remoteId");
            return null;
        }

        _logger.LogDebug("Getting video by remote ID: {RemoteId}", remoteId);

        try
        {
            var videoInfo = await _dataRepository.GetVideoAsync(remoteId, _videoProvider, cancellationToken);

            if (videoInfo is null)
            {
                _logger.LogWarning("Video not found: {RemoteId}", remoteId);
                return null;
            }

            var viewModel = new Video(this, _loggerFactory, videoInfo);
            _logger.LogDebug("Created VideoPlayerViewModel for: {Title}", videoInfo.Title);

            return viewModel;
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("GetVideoByRemoteIdAsync cancelled for: {RemoteId}", remoteId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting video by remote ID: {RemoteId}", remoteId);
            return null;
        }
    }

    /// <summary>
    /// Gets channel details by remote ID and returns a ChannelViewModel wrapping it.
    /// </summary>
    /// <param name="remoteId">The channel's remote identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A ChannelViewModel wrapping the channel data, or null if not found.</returns>
    public async Task<Channel?> GetChannelDetailsByRemoteIdAsync(string remoteId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(remoteId))
        {
            _logger.LogWarning("GetChannelDetailsByRemoteIdAsync called with empty remoteId");
            return null;
        }

        _logger.LogDebug("Getting channel by remote ID: {RemoteId}", remoteId);

        try
        {
            var channelDetails = await _dataRepository.GetChannelAsync(remoteId, _videoProvider, cancellationToken);

            if (channelDetails is null)
            {
                _logger.LogWarning("Channel not found: {RemoteId}", remoteId);
                return null;
            }

            var viewModel = new Channel(this, _loggerFactory, channelDetails);
            _logger.LogDebug("Created ChannelViewModel for: {ChannelName}", channelDetails.Name);

            return viewModel;
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("GetChannelDetailsByRemoteIdAsync cancelled for: {RemoteId}", remoteId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting channel by remote ID: {RemoteId}", remoteId);
            return null;
        }
    }

    /// <summary>
    /// Gets a page of videos from a channel.
    /// </summary>
    /// <param name="channelId">The channel's remote identifier.</param>
    /// <param name="tab">The tab to fetch from.</param>
    /// <param name="continuationToken">Pagination token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A page of video summaries.</returns>
    public async Task<VideosPage?> GetChannelVideosAsync(
        string channelId,
        string tab,
        string? continuationToken,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Getting channel videos: {ChannelId}, tab: {Tab}, hasContinuation: {HasContinuation}",
            channelId,
            tab,
            continuationToken is not null);

        return await _dataRepository.GetChannelVideosAsync(
            channelId,
            tab,
            continuationToken,
            _videoProvider,
            cancellationToken);
    }

    /// <summary>
    /// Invalidates cached video data, forcing a refresh on next access.
    /// </summary>
    /// <param name="remoteId">The video's remote identifier.</param>
    public void InvalidateVideo(string remoteId)
    {
        _dataRepository.InvalidateVideo(remoteId);
    }

    /// <summary>
    /// Invalidates cached channel data, forcing a refresh on next access.
    /// </summary>
    /// <param name="remoteId">The channel's remote identifier.</param>
    public void InvalidateChannel(string remoteId)
    {
        _dataRepository.InvalidateChannel(remoteId);
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

    /// <summary>
    /// Gets an image by its original URL with caching (memory → DB → web).
    /// </summary>
    /// <param name="originalUrl">The original URL of the image (YouTube CDN URL).</param>
    /// <param name="fetchUrl">The URL to fetch the image from (may be provider proxy).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Cached image data or null if failed.</returns>
    public async Task<CachedImage?> GetImageAsync(Uri originalUrl, Uri fetchUrl, CancellationToken cancellationToken)
    {
        return await _dataRepository.GetImageAsync(originalUrl, fetchUrl, cancellationToken);
    }

    /// <summary>
    /// Creates an ImageViewModel for async image loading.
    /// </summary>
    /// <param name="originalUrl">The original URL of the image (YouTube CDN URL).</param>
    /// <param name="fetchUrl">The URL to fetch the image from (may be provider proxy).</param>
    /// <param name="placeholderDataUrl">Optional placeholder.</param>
    /// <returns>An ImageViewModel configured to load the image.</returns>
    public Image CreateImageViewModel(Uri originalUrl, Uri fetchUrl, string? placeholderDataUrl = null)
    {
        return new Image(
            this,
            _loggerFactory.CreateLogger<Image>(),
            originalUrl,
            fetchUrl,
            placeholderDataUrl);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        // Only dispose the DataRepository if we created it ourselves
        if (_ownsDataRepository)
        {
            _dataRepository.Dispose();
        }

        _disposed = true;
        _logger.LogDebug("Super disposed");
    }
}
