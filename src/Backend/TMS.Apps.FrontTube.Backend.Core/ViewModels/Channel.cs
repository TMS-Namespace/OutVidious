using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

namespace TMS.Apps.FrontTube.Backend.Core.ViewModels;

/// <summary>
/// ViewModel for displaying a channel and its content.
/// Wraps a ChannelDetails contract loaded via Super.
/// </summary>
public sealed class Channel : IDisposable
{
    private readonly Super _super;
    private readonly ILogger<Channel> _logger;
    private readonly Common.ProviderCore.Contracts.Channel _channelDetails;
    private CancellationTokenSource? _loadCts;
    private string? _currentContinuationToken;
    private string? _selectedTab;
    private bool _isDisposed;

    /// <summary>
    /// Creates a new ChannelViewModel wrapping the provided channel details.
    /// </summary>
    /// <param name="super">The parent Super ViewModel.</param>
    /// <param name="loggerFactory">Logger factory for creating loggers.</param>
    /// <param name="channelDetails">The channel details contract to wrap.</param>
    public Channel(Super super, ILoggerFactory loggerFactory, Common.ProviderCore.Contracts.Channel channelDetails)
    {
        _super = super ?? throw new ArgumentNullException(nameof(super));
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<Channel>();
        
        _channelDetails = channelDetails ?? throw new ArgumentNullException(nameof(channelDetails));
        RemoteId = channelDetails.RemoteId;
        Name = channelDetails.Name;
        Description = channelDetails.Description;
        SubscriberCount = channelDetails.SubscriberCount ?? 0;
        SubscriberCountText = channelDetails.SubscriberCountText;
        Avatars = channelDetails.Avatars;
        Banners = channelDetails.Banners;

        // Select the videos tab by default (use RemoteTabId for API calls)
#pragma warning disable CS0618 // Type or member is obsolete - RemoteTabId is the correct API identifier
        _selectedTab = _channelDetails.AvailableTabs
            .FirstOrDefault(t => t.RemoteTabId.Equals("videos", StringComparison.OrdinalIgnoreCase))?.RemoteTabId
            ?? _channelDetails.AvailableTabs.FirstOrDefault()?.RemoteTabId;
#pragma warning restore CS0618

        _logger.LogDebug("ChannelViewModel created for: {ChannelName}", channelDetails.Name);
    }

    /// <summary>
    /// Event raised when the state has changed.
    /// </summary>
    public event EventHandler? StateChanged;

    /// <summary>
    /// The channel's remote identifier.
    /// </summary>
    public string RemoteId { get; }

    /// <summary>
    /// The channel name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The channel description.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Number of subscribers.
    /// </summary>
    public long SubscriberCount { get; }

    /// <summary>
    /// Formatted subscriber count text.
    /// </summary>
    [Obsolete("Use SubscriberCount instead")]
    public string? SubscriberCountText { get; }

    /// <summary>
    /// Channel avatar images.
    /// </summary>
    public IReadOnlyList<Common.ProviderCore.Contracts.Image> Avatars { get; }

    /// <summary>
    /// Channel banner images.
    /// </summary>
    public IReadOnlyList<Common.ProviderCore.Contracts.Image> Banners { get; }

    /// <summary>
    /// Available channel tab IDs (e.g., "videos", "shorts", "streams").
    /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete - RemoteTabId is the correct API identifier
    public IReadOnlyList<string> AvailableTabs => _channelDetails.AvailableTabs
        .Select(t => t.RemoteTabId)
        .ToList();
#pragma warning restore CS0618

    /// <summary>
    /// The list of video ViewModels from the channel.
    /// </summary>
    public ObservableCollection<Video> Videos { get; } = [];

    /// <summary>
    /// The currently selected tab ID.
    /// </summary>
    public string? SelectedTab
    {
        get => _selectedTab;
        private set => _selectedTab = value;
    }

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
    /// Gets the channel URL for this channel.
    /// </summary>
    /// <returns>The channel URL.</returns>
    public Uri GetChannelUrl()
    {
        return _super.Proxy.ProxyChannelUrl(RemoteId);
    }

    /// <summary>
    /// Gets the URL of the best available avatar image.
    /// </summary>
    public string? GetBestAvatarUrl()
    {
        if (Avatars.Count == 0)
        {
            return null;
        }

        var avatar = Avatars
            .Where(a => a.Width >= 88)
            .OrderByDescending(a => a.Width)
            .FirstOrDefault() ?? Avatars.First();

        return avatar.RemoteUrl.ToString();
    }

    /// <summary>
    /// Gets the URL of the best available banner image.
    /// </summary>
    public string? GetBestBannerUrl()
    {
        if (Banners.Count == 0)
        {
            return null;
        }

        var banner = Banners
            .Where(b => b.Width >= 1280 && b.Width <= 2560)
            .OrderByDescending(b => b.Width)
            .FirstOrDefault() ?? Banners.First();

        return banner.RemoteUrl.ToString();
    }

    /// <summary>
    /// Loads the initial videos for the current tab.
    /// </summary>
    public async Task LoadInitialVideosAsync(CancellationToken cancellationToken)
    {
        if (SelectedTab is null)
        {
            return;
        }

        await LoadVideosForTabAsync(SelectedTab, isInitial: true, cancellationToken);
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

        if (tabId == SelectedTab)
        {
            return;
        }

        SelectedTab = tabId;
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

        await LoadVideosForTabAsync(SelectedTab, isInitial: false, cancellationToken);
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
                RemoteId, 
                tabId, 
                _currentContinuationToken is not null);

            var videosPage = await _super.GetChannelVideosAsync(
                RemoteId, 
                tabId, 
                _currentContinuationToken, 
                token);

            if (videosPage is not null)
            {
                foreach (var videoMetadata in videosPage.Videos)
                {
                    Videos.Add(new Video(_super, _super.LoggerFactory, videoMetadata));
                }

                _currentContinuationToken = videosPage.ContinuationToken;
                HasMore = videosPage.HasMore;

                _logger.LogDebug(
                    "Loaded {VideoCount} videos, total: {TotalCount}, hasMore: {HasMore}", 
                    videosPage.Videos.Count, 
                    Videos.Count, 
                    HasMore);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Videos loading cancelled for channel {ChannelId}", RemoteId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading videos for channel {ChannelId}", RemoteId);
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
