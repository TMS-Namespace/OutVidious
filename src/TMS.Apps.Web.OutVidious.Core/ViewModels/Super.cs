using Microsoft.Extensions.Logging;
using TMS.Apps.FTube.Backend.DataRepository;
using TMS.Apps.FTube.Backend.DataRepository.Interfaces;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Configuration;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Contracts;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Interfaces;

namespace TMS.Apps.Web.OutVidious.Core.ViewModels;

/// <summary>
/// Top-level ViewModel that manages data repository and other ViewModels.
/// Acts as the central hub for accessing video and channel data.
/// </summary>
public sealed class Super : IDisposable
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<Super> _logger;
    private readonly IDataRepository _dataRepository;
    private readonly IVideoProvider _videoProvider;
    private bool _disposed;

    public Super(
        ILoggerFactory loggerFactory,
        IVideoProvider videoProvider,
        DataRepositoryConfig dataRepositoryConfig)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _videoProvider = videoProvider ?? throw new ArgumentNullException(nameof(videoProvider));
        _logger = loggerFactory.CreateLogger<Super>();

        ArgumentNullException.ThrowIfNull(dataRepositoryConfig);

        _dataRepository = new DataRepository(dataRepositoryConfig, loggerFactory);

        _logger.LogDebug("Super initialized with provider: {ProviderType}", videoProvider.GetType().Name);
    }

    /// <summary>
    /// Gets the underlying video provider.
    /// </summary>
    public IVideoProvider VideoProvider => _videoProvider;

    /// <summary>
    /// Gets video information by remote ID and returns a VideoPlayerViewModel wrapping it.
    /// </summary>
    /// <param name="remoteId">The video's remote identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A VideoPlayerViewModel wrapping the video data, or null if not found.</returns>
    public async Task<VideoPlayerViewModel?> GetVideoByRemoteIdAsync(string remoteId, CancellationToken cancellationToken)
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

            var viewModel = new VideoPlayerViewModel(this, _loggerFactory, videoInfo);
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
    public async Task<ChannelViewModel?> GetChannelDetailsByRemoteIdAsync(string remoteId, CancellationToken cancellationToken)
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

            var viewModel = new ChannelViewModel(this, _loggerFactory, channelDetails);
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
    public async Task<ChannelVideoPage?> GetChannelVideosAsync(
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

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _dataRepository.Dispose();
        _disposed = true;
        _logger.LogDebug("Super disposed");
    }
}
