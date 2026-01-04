using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Core.Mappers;
using TMS.Apps.FrontTube.Backend.Core.Tools;
using TMS.Apps.FrontTube.Backend.Repository.Data.Contracts;

namespace TMS.Apps.FrontTube.Backend.Core.ViewModels;

/// <summary>
/// ViewModel for displaying a channel and its content.
/// Wraps a ChannelDomain contract loaded via Super.
/// </summary>
public sealed class Channel : ViewModelBase
{
    private readonly Super _super;
    private readonly ILogger<Channel> _logger;
    private CancellationTokenSource? _loadCts;
    private string? _currentContinuationToken;
    private string? _selectedTab;
    private bool _isDisposed;

    internal ChannelDomain Domain { get; private set; }
    public RemoteIdentity RemoteIdentity { get; private set; }

    internal Channel(
        Super super,
        ChannelDomain domain)
        : base(super)
    {
        _super = super ?? throw new ArgumentNullException(nameof(super));
        _logger = super.LoggerFactory.CreateLogger<Channel>();
        UpdateFromDomain(domain);
    }

    /// <summary>
    /// Event raised when the state has changed.
    /// </summary>
    public event EventHandler? StateChanged;

    /// <summary>
    /// The channel name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// The channel description.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// HTML-formatted channel description.
    /// </summary>
    public string? DescriptionHtml { get; private set; }

    /// <summary>
    /// Number of subscribers.
    /// </summary>
    public long SubscriberCount { get; private set; }

    /// <summary>
    /// Formatted subscriber count text.
    /// </summary>
    [Obsolete("Use SubscriberCount instead")]
    public string? SubscriberCountText { get; }

    /// <summary>
    /// Channel avatar images.
    /// </summary>
    public IReadOnlyList<Image> Avatars { get; private set; } = [];

    /// <summary>
    /// Channel banner images.
    /// </summary>
    public IReadOnlyList<Image> Banners { get; private set; } = [];

    /// <summary>
    /// Available channel tab IDs (e.g., "videos", "shorts", "streams").
    /// </summary>
    public IReadOnlyList<string> AvailableTabs
    {
        get
        {
            if (Domain.AvailableTabs.Count > 0)
            {
                return Domain.AvailableTabs;
            }

            return ["videos"];
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

    internal void UpdateFromDomain(ChannelDomain domain)
    {
        Domain = domain ?? throw new ArgumentNullException(nameof(domain));
        RemoteIdentity = DomainViewModelMapper.ToViewModel(domain.RemoteIdentity);
        Avatars = Domain.Avatars
            .Select(avatar => DomainViewModelMapper.ToViewModel(Super, avatar))
            .ToList();
        Banners = Domain.Banners
            .Select(banner => DomainViewModelMapper.ToViewModel(Super, banner))
            .ToList();
        Name = Domain.Title;
        Description = Domain.Description ?? string.Empty;
        DescriptionHtml = Domain.DescriptionHtml;
        SubscriberCount = Domain.SubscriberCount ?? 0;
        ErrorMessage = Domain.FetchingError;

        var availableTabs = AvailableTabs;
        if (_selectedTab is null || !availableTabs.Contains(_selectedTab))
        {
            _selectedTab = availableTabs.Count > 0 ? availableTabs[0] : null;
        }
    }

    /// <summary>
    /// Gets the channel URL for this channel.
    /// </summary>
    /// <returns>The channel URL.</returns>
    public Uri GetChannelUrl()
    {
        if (string.IsNullOrWhiteSpace(RemoteIdentity.RemoteId))
        {
            _logger.LogWarning(
                "[{MethodName}] Remote ID missing for identity '{@Identity}'.",
                nameof(GetChannelUrl),
                RemoteIdentity);
            return _super.Proxy.ProviderBaseUrl;
        }

        return _super.Proxy.ProxyChannelUrl(RemoteIdentity.RemoteId);
    }

    /// <summary>
    /// Gets the URL of the best available avatar image.
    /// </summary>
    public RemoteIdentity? GetBestAvatarIdentity()
    {
        if (Avatars.Count == 0)
        {
            return null;
        }

        var avatar = Avatars
            .Where(a => a.Width >= 88)
            .OrderByDescending(a => a.Width)
            .FirstOrDefault() ?? Avatars.First();

        return avatar.RemoteIdentity;
    }

    /// <summary>
    /// Gets the URL of the best available banner image.
    /// </summary>
    public RemoteIdentity? GetBestBannerIdentity()
    {
        if (Banners.Count == 0)
        {
            return null;
        }

        var banner = Banners
            .Where(b => b.Width >= 1280 && b.Width <= 2560)
            .OrderByDescending(b => b.Width)
            .FirstOrDefault() ?? Banners.First();

        return banner.RemoteIdentity;
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
            var identity = Domain.RemoteIdentity;

            var page = await Super.RepositoryManager.GetChannelsPageAsync(
                identity,
                tabId,
                _currentContinuationToken,
                token,
                autoSave: true);

            if (page is not null)
            {
                Videos = page.Videos
                    .Select(v => DomainViewModelMapper.ToViewModel(Super, v))
                    .ToList();
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
            _logger.LogDebug("Videos loading cancelled for channel {ChannelUrl}", RemoteIdentity.AbsoluteRemoteUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading videos for channel {ChannelUrl}", RemoteIdentity.AbsoluteRemoteUrl);
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
