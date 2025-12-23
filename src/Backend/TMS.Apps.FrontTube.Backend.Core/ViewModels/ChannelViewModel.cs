using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

namespace TMS.Apps.FrontTube.Backend.Core.ViewModels;

/// <summary>
/// ViewModel for displaying a channel and its content.
/// Wraps a ChannelDetails contract loaded via Super.
/// </summary>
public sealed class ChannelViewModel : IDisposable
{
    private readonly Super _super;
    private readonly ILogger<ChannelViewModel> _logger;
    private CancellationTokenSource? _loadCts;
    private string? _currentContinuationToken;
    private bool _isDisposed;

    /// <summary>
    /// Creates a new ChannelViewModel wrapping the provided channel details.
    /// </summary>
    /// <param name="super">The parent Super ViewModel.</param>
    /// <param name="loggerFactory">Logger factory for creating loggers.</param>
    /// <param name="channelDetails">The channel details contract to wrap.</param>
    public ChannelViewModel(Super super, ILoggerFactory loggerFactory, ChannelDetails channelDetails)
    {
        _super = super ?? throw new ArgumentNullException(nameof(super));
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<ChannelViewModel>();
        
        Channel = channelDetails ?? throw new ArgumentNullException(nameof(channelDetails));
        ChannelId = channelDetails.ChannelId;

        // Select the videos tab by default
        SelectedTab = Channel.AvailableTabs.FirstOrDefault(t => 
            t.TabId.Equals("videos", StringComparison.OrdinalIgnoreCase))
            ?? Channel.AvailableTabs.FirstOrDefault();

        _logger.LogDebug("ChannelViewModel created for: {ChannelName}", channelDetails.Name);
    }

    /// <summary>
    /// Event raised when the state has changed.
    /// </summary>
    public event EventHandler? StateChanged;

    /// <summary>
    /// The current channel ID.
    /// </summary>
    public string ChannelId { get; }

    /// <summary>
    /// The channel details.
    /// </summary>
    public ChannelDetails Channel { get; }

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
    /// Loads the initial videos for the current tab.
    /// </summary>
    public async Task LoadInitialVideosAsync(CancellationToken cancellationToken)
    {
        if (SelectedTab is null)
        {
            return;
        }

        await LoadVideosForTabAsync(SelectedTab.TabId, isInitial: true, cancellationToken);
    }

    /// <summary>
    /// Loads videos for a specific tab.
    /// </summary>
    public async Task SelectTabAsync(string tabId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tabId))
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
        CancelPendingLoads();
        _loadCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = _loadCts.Token;

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

            var page = await _super.GetChannelVideosAsync(
                ChannelId, 
                tabId, 
                _currentContinuationToken, 
                token);

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
