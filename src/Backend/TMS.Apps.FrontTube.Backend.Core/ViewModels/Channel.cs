using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Core.Tools;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Models;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;

namespace TMS.Apps.FrontTube.Backend.Core.ViewModels;

/// <summary>
/// ViewModel for displaying a channel and its content.
/// Wraps a ChannelDetails contract loaded via Super.
/// </summary>
public sealed class Channel : ViewModelBase
{
    private readonly Super _super;
    private readonly ILogger<Channel> _logger;
    //private readonly Common.ProviderCore.Contracts.Channel _channelDetails;
    private CancellationTokenSource? _loadCts;
    private string? _currentContinuationToken;
    private string? _selectedTab;
    private bool _isDisposed;

    internal CacheResult<ChannelEntity> CacheResult { get; }

    internal Channel(Super super, CacheResult<ChannelEntity> channelCachingResult, IReadOnlyList<Image> avatars, IReadOnlyList<Image> banners)
    : base(super)
    {
        _super = super ?? throw new ArgumentNullException(nameof(super));
        CacheResult = channelCachingResult ?? throw new ArgumentNullException(nameof(channelCachingResult));
        _logger = super.LoggerFactory.CreateLogger<Channel>();
        
        Avatars = avatars;
        Banners = banners;
        
        // Initialize properties - prefer Entity over Common (Entity is always available, Common only for new fetches)
        var channelEntity = CacheResult.Entity;
        var channelCommon = CacheResult.Common as Common.ProviderCore.Contracts.Channel;
        
        AbsoluteRemoteUrl = new Uri(CacheResult.Identity.AbsoluteRemoteUrlString);
        Name = channelEntity?.Title ?? channelCommon?.Name ?? string.Empty;
        Description = channelEntity?.Description ?? channelCommon?.Description ?? string.Empty;
        SubscriberCount = channelEntity?.SubscriberCount ?? channelCommon?.SubscriberCount ?? 0;
#pragma warning disable CS0618
        SubscriberCountText = channelCommon?.SubscriberCountText;
#pragma warning restore CS0618
        
        // Auto-select the first available tab (defaults to "videos" for cached channels)
        var availableTabs = AvailableTabs;
        if (availableTabs.Count > 0)
        {
            _selectedTab = availableTabs[0];
        }
    }

    /// <summary>
    /// Event raised when the state has changed.
    /// </summary>
    public event EventHandler? StateChanged;

    /// <summary>
    /// The channel's absolute remote URL on the original platform.
    /// </summary>
    public Uri AbsoluteRemoteUrl { get; }

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
    public IReadOnlyList<Image> Avatars { get; }

    /// <summary>
    /// Channel banner images.
    /// </summary>
    public IReadOnlyList<Image> Banners { get; }

    /// <summary>
    /// Available channel tab IDs (e.g., "videos", "shorts", "streams").
    /// </summary>
    public IReadOnlyList<string> AvailableTabs
    {
        get
        {
            var channelCommon = CacheResult.Common as Common.ProviderCore.Contracts.Channel;
            if (channelCommon is null)
            {
                // For existing cached channels, default to "videos" tab
                return ["videos"];  
            }
#pragma warning disable CS0618
            return channelCommon.AvailableTabs
                .Select(t => t.RemoteTabId)
                .ToList();
#pragma warning restore CS0618
            
        }
    }

    /// <summary>
    /// The list of video ViewModels from the channel.
    /// </summary>
    public IReadOnlyList<Video> Videos { get; private set; } = [];

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
        var channelId = YouTubeValidator.ExtractChannelIdFromUrl(AbsoluteRemoteUrl);
        return _super.Proxy.ProxyChannelUrl(channelId ?? string.Empty);
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

        return avatar.AbsoluteRemoteUrl?.ToString();
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

        return banner.AbsoluteRemoteUrl?.ToString();
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
        Videos = [];
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
            Videos = [];
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
            var page = await Super.RepositoryManager.GetChannelsPageAsync(
                CacheResult.Identity,
                tabId,
                _currentContinuationToken,
                token,
                autoSave: true);

            if (page is not null)
            {
                Videos = page.Videos;
                HasMore = page.HasMore;
                _currentContinuationToken = page.ContinuationToken;
                ErrorMessage = null;
            }
            else
            {
                Videos = [];
                HasMore = false;
                if (isInitial)
                {
                    ErrorMessage = "No videos found or channel unavailable.";
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Videos loading cancelled for channel {ChannelUrl}", AbsoluteRemoteUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading videos for channel {ChannelUrl}", AbsoluteRemoteUrl);
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

    public override void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        CancelPendingLoads();
        _isDisposed = true;
    }
}
