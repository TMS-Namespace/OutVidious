using System.Net;
using Microsoft.AspNetCore.Components;
using TMS.Apps.FrontTube.Backend.Core.ViewModels;
using TMS.Apps.FrontTube.Frontend.WebUI.Services;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Components;

/// <summary>
/// Component for displaying channel info (banner, avatar, description).
/// Loads only channel metadata without videos.
/// </summary>
public partial class ChannelAboutComponent : ComponentBase, IDisposable
{
    private bool _isDisposed;
    private string? _loadedChannelId;
    private CancellationTokenSource? _cts;
    private bool _wasActive;

    [Inject]
    private Orchestrator Orchestrator { get; set; } = null!;

    [Inject]
    private ILogger<ChannelAboutComponent> Logger { get; set; } = null!;

    /// <summary>
    /// Gets or sets whether the dock panel is currently active/visible.
    /// </summary>
    [Parameter]
    public bool IsActive { get; set; }

    /// <summary>
    /// The channel ID to display.
    /// </summary>
    [Parameter]
    public string? ChannelId { get; set; }

    /// <summary>
    /// Event callback when channel ID changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> ChannelIdChanged { get; set; }

    protected Channel? ViewModel { get; private set; }

    protected bool IsInitialLoading { get; private set; }

    protected string? ErrorMessage { get; private set; }

    protected string? AvatarUrl => GetBestAvatar();

    protected string? BannerUrl => GetBestBanner();

    protected bool HasChannelDescription => !string.IsNullOrWhiteSpace(ViewModel?.DescriptionHtml)
        || !string.IsNullOrWhiteSpace(ViewModel?.Description);

    protected MarkupString ChannelDescriptionMarkup
    {
        get
        {
            if (ViewModel is null)
            {
                return new MarkupString(string.Empty);
            }

            if (!string.IsNullOrWhiteSpace(ViewModel.DescriptionHtml))
            {
                return new MarkupString(ViewModel.DescriptionHtml);
            }

            if (string.IsNullOrWhiteSpace(ViewModel.Description))
            {
                return new MarkupString(string.Empty);
            }

            var encoded = WebUtility.HtmlEncode(ViewModel.Description);
            var withLineBreaks = encoded
                .Replace("\r\n", "<br />")
                .Replace("\n", "<br />");

            return new MarkupString(withLineBreaks);
        }
    }

    protected override void OnInitialized()
    {
        _cts = new CancellationTokenSource();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (IsActive && !_wasActive)
        {
            _wasActive = true;
            if (!string.IsNullOrWhiteSpace(ChannelId) && _loadedChannelId != ChannelId)
            {
                await LoadChannelAsync(ChannelId);
            }
        }
        else if (!IsActive && _wasActive)
        {
            _wasActive = false;
        }
        else if (IsActive && !string.IsNullOrWhiteSpace(ChannelId) && _loadedChannelId != ChannelId)
        {
            await LoadChannelAsync(ChannelId);
        }
    }

    /// <summary>
    /// Loads a channel by ID. Can be called externally to trigger channel loading.
    /// </summary>
    public async Task LoadChannelByIdAsync(string channelId)
    {
        if (ChannelId != channelId)
        {
            ChannelId = channelId;
            await ChannelIdChanged.InvokeAsync(channelId);
        }

        if (IsActive)
        {
            await LoadChannelAsync(channelId);
        }
    }

    private async Task LoadChannelAsync(string channelId)
    {
        if (_cts is null)
        {
            return;
        }

        IsInitialLoading = true;
        ErrorMessage = null;
        StateHasChanged();

        try
        {
            Logger.LogDebug("[{MethodName}] Loading channel '{ChannelId}'.", nameof(LoadChannelAsync), channelId);

            if (ViewModel is not null)
            {
                ViewModel.StateChanged -= OnViewModelStateChanged;
                ViewModel.Dispose();
            }

            ViewModel = await Orchestrator.Super.GetChannelByIdAsync(channelId, _cts.Token);

            if (ViewModel is null)
            {
                ErrorMessage = $"Channel '{channelId}' not found.";
                Logger.LogWarning("[{MethodName}] Channel not found: '{ChannelId}'.", nameof(LoadChannelAsync), channelId);
                IsInitialLoading = false;
                StateHasChanged();
                return;
            }

            ViewModel.StateChanged += OnViewModelStateChanged;

            Logger.LogDebug("[{MethodName}] Channel loaded: '{ChannelName}'.", nameof(LoadChannelAsync), ViewModel.Name);

            _loadedChannelId = channelId;
            IsInitialLoading = false;
            StateHasChanged();
        }
        catch (OperationCanceledException)
        {
            Logger.LogDebug("[{MethodName}] Channel loading cancelled for: '{ChannelId}'.", nameof(LoadChannelAsync), channelId);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to load channel. Please try again.";
            Logger.LogError(ex, "[{MethodName}] Unexpected error loading channel: '{ChannelId}'.", nameof(LoadChannelAsync), channelId);
            IsInitialLoading = false;
            StateHasChanged();
        }
    }

    private void OnViewModelStateChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private string? GetBestAvatar()
    {
        var avatarIdentity = ViewModel?.GetBestAvatarIdentity();
        if (avatarIdentity is null)
        {
            return null;
        }

        var proxyUrl = avatarIdentity.GetProxyUrl(Orchestrator.Super.Proxy);
        return string.IsNullOrWhiteSpace(proxyUrl) ? null : proxyUrl;
    }

    private string? GetBestBanner()
    {
        var bannerIdentity = ViewModel?.GetBestBannerIdentity();
        if (bannerIdentity is null)
        {
            return null;
        }

        var proxyUrl = bannerIdentity.GetProxyUrl(Orchestrator.Super.Proxy);
        return string.IsNullOrWhiteSpace(proxyUrl) ? null : proxyUrl;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        if (ViewModel is not null)
        {
            ViewModel.StateChanged -= OnViewModelStateChanged;
            ViewModel.Dispose();
        }

        _cts?.Cancel();
        _cts?.Dispose();

        _isDisposed = true;
    }
}
