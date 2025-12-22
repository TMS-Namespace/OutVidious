using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Contracts;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Interfaces;

namespace TMS.Apps.Web.OutVidious.Core.ViewModels;

/// <summary>
/// ViewModel for displaying a channel and its content.
/// </summary>
public sealed class ChannelViewModel : IDisposable
{
    private readonly IVideoProvider _videoProvider;
    private readonly ILogger<ChannelViewModel> _logger;
    private CancellationTokenSource? _loadCts;
    private string? _currentContinuationToken;
    private bool _isDisposed;

    public ChannelViewModel(IVideoProvider videoProvider, ILogger<ChannelViewModel> logger)
    {
        _videoProvider = videoProvider ?? throw new ArgumentNullException(nameof(videoProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Event raised when the state has changed.
    /// </summary>
    public event EventHandler? StateChanged;

    /// <summary>
    /// The current channel ID.
    /// </summary>
    public string? ChannelId { get; private set; }

    /// <summary>
    /// The channel details.
    /// </summary>
    public ChannelDetails? Channel { get; private set; }

    /// <summary>
    /// The list of videos from the channel.
    /// </summary>
    public ObservableCollection<VideoSummary> Videos { get; } = [];

    /// <summary>
    /// The currently selected tab.
    /// </summary>
    public ChannelTab? SelectedTab { get; private set; }

    /// <summary>
    /// Whether the initial content is loading.
    /// </summary>
    public bool IsLoading { get; private set; }

    /// <summary>
    /// Whether more content is being loaded.
    /// </summary>
    public bool IsLoadingMore { get; private set; }

    /// <summary>
    /// Whether there are more videos to load.
    /// </summary>
    public bool HasMore { get; private set; }

    /// <summary>
    /// Error message if loading failed.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Loads a channel by its ID.
    /// </summary>
    public async Task LoadChannelAsync(string channelId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(channelId))
        {
            throw new ArgumentException("Channel ID cannot be empty", nameof(channelId));
        }

        CancelPendingLoads();
        _loadCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = _loadCts.Token;

        ChannelId = channelId;
        Channel = null;
        Videos.Clear();
        SelectedTab = null;
        _currentContinuationToken = null;
        HasMore = false;
        ErrorMessage = null;
        IsLoading = true;
        
        NotifyStateChanged();

        try
        {
            _logger.LogDebug("Loading channel details for {ChannelId}", channelId);
            
            Channel = await _videoProvider.GetChannelDetailsAsync(channelId, token);

            if (Channel is null)
            {
                ErrorMessage = $"Channel '{channelId}' not found.";
                _logger.LogWarning("Channel not found: {ChannelId}", channelId);
                return;
            }

            _logger.LogDebug("Loaded channel: {ChannelName} with {TabCount} tabs", 
                Channel.Name, 
                Channel.AvailableTabs.Count);

            // Select the videos tab by default
            SelectedTab = Channel.AvailableTabs.FirstOrDefault(t => 
                t.TabId.Equals("videos", StringComparison.OrdinalIgnoreCase))
                ?? Channel.AvailableTabs.FirstOrDefault();

            if (SelectedTab is not null)
            {
                await LoadVideosForTabAsync(SelectedTab.TabId, isInitial: true, token);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Channel loading cancelled for {ChannelId}", channelId);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to load channel. Please try again.";
            _logger.LogError(ex, "Error loading channel {ChannelId}", channelId);
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Loads videos for a specific tab.
    /// </summary>
    public async Task SelectTabAsync(string tabId, CancellationToken cancellationToken)
    {
        if (Channel is null || string.IsNullOrWhiteSpace(tabId))
        {
            return;
        }

        var tab = Channel.AvailableTabs.FirstOrDefault(t => 
            t.TabId.Equals(tabId, StringComparison.OrdinalIgnoreCase));
        
        if (tab is null || tab == SelectedTab)
        {
            return;
        }

        SelectedTab = tab;
        Videos.Clear();
        _currentContinuationToken = null;
        
        await LoadVideosForTabAsync(tabId, isInitial: true, cancellationToken);
    }

    /// <summary>
    /// Loads the next page of videos.
    /// </summary>
    public async Task LoadMoreVideosAsync(CancellationToken cancellationToken)
    {
        if (!HasMore || IsLoadingMore || SelectedTab is null)
        {
            return;
        }

        await LoadVideosForTabAsync(SelectedTab.TabId, isInitial: false, cancellationToken);
    }

    private async Task LoadVideosForTabAsync(string tabId, bool isInitial, CancellationToken cancellationToken)
    {
        if (ChannelId is null)
        {
            return;
        }

        if (isInitial)
        {
            Videos.Clear();
            _currentContinuationToken = null;
            IsLoading = true;
        }
        else
        {
            IsLoadingMore = true;
        }
        
        NotifyStateChanged();

        try
        {
            _logger.LogDebug(
                "Loading videos for channel {ChannelId}, tab {TabId}, continuation: {HasContinuation}", 
                ChannelId, 
                tabId, 
                _currentContinuationToken is not null);

            var page = await _videoProvider.GetChannelVideosAsync(
                ChannelId, 
                tabId, 
                _currentContinuationToken, 
                cancellationToken);

            if (page is not null)
            {
                foreach (var video in page.Videos)
                {
                    Videos.Add(video);
                }

                _currentContinuationToken = page.ContinuationToken;
                HasMore = page.HasMore;

                _logger.LogDebug(
                    "Loaded {VideoCount} videos, total: {TotalCount}, hasMore: {HasMore}", 
                    page.Videos.Count, 
                    Videos.Count, 
                    HasMore);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Videos loading cancelled for channel {ChannelId}", ChannelId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading videos for channel {ChannelId}", ChannelId);
            // Don't set error message for pagination failures
            if (isInitial)
            {
                ErrorMessage = "Failed to load videos. Please try again.";
            }
        }
        finally
        {
            IsLoading = false;
            IsLoadingMore = false;
            NotifyStateChanged();
        }
    }

    private void CancelPendingLoads()
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = null;
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        CancelPendingLoads();
        _isDisposed = true;
    }
}
