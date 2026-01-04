using Microsoft.AspNetCore.Components;
using MudBlazor;
using TMS.Apps.FrontTube.Backend.Core.Enums;
using TMS.Apps.FrontTube.Backend.Core.ViewModels;
using TMS.Apps.FrontTube.Frontend.WebUI.Services;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Components;

/// <summary>
/// Component for loading images asynchronously with caching support.
/// Shows a skeleton while loading, and a fallback icon on failure.
/// </summary>
public partial class ImageComponent : ComponentBase, IDisposable
{
    private CancellationTokenSource? _cts;
    private Image? _imageViewModel;
    private bool _isDisposed;
    private string? _currentImageUrl;

    [Inject]
    private Orchestrator Orchestrator { get; set; } = null!;

    [Inject]
    private ILogger<ImageComponent> Logger { get; set; } = null!;

    /// <summary>
    /// The URL of the image to load.
    /// </summary>
    [Parameter]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Alternative text for the image.
    /// </summary>
    [Parameter]
    public string? Alt { get; set; }

    /// <summary>
    /// Width in pixels. If null, uses auto.
    /// </summary>
    [Parameter]
    public int? Width { get; set; }

    /// <summary>
    /// Height in pixels. If null, uses auto.
    /// </summary>
    [Parameter]
    public int? Height { get; set; }

    /// <summary>
    /// Object fit mode for the image.
    /// </summary>
    [Parameter]
    public ObjectFit ObjectFit { get; set; } = ObjectFit.Cover;

    /// <summary>
    /// CSS class to apply.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Whether to use async loading with caching (true) or direct URL (false).
    /// </summary>
    [Parameter]
    public bool UseAsyncLoading { get; set; } = true;

    /// <summary>
    /// Optional placeholder URL to show while loading.
    /// </summary>
    [Parameter]
    public string? PlaceholderUrl { get; set; }

    protected bool IsLoading => _imageViewModel?.LoadState == LoadingState.Loading;

    protected bool HasFailed => _imageViewModel?.LoadState == LoadingState.Failed;

    protected bool IsLoaded => _imageViewModel?.LoadState == LoadingState.Loaded;

    protected string DisplayUrl
    {
        get
        {
            if (!UseAsyncLoading)
            {
                // Use the image URL directly for non-async loading
                // The URL should already be a proxy URL if needed
                return GetDisplayUrl(ImageUrl) ?? string.Empty;
            }

            // If loaded, use the cached data URL
            if (IsLoaded && _imageViewModel?.DataUrl is not null)
            {
                return _imageViewModel.DataUrl;
            }

            // Otherwise show the original URL (browser will load it directly)
            return ImageUrl ?? PlaceholderUrl ?? string.Empty;
        }
    }

    /// <summary>
    /// Gets the URL to use for displaying the image.
    /// If the ImageUrl is already a proxy URL (starts with /api/ImageProxy), use it directly.
    /// Otherwise, use the URL as-is.
    /// </summary>
    private static string? GetDisplayUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return null;
        }

        // If it's already a proxy URL, use it directly
        if (imageUrl.StartsWith("/api/ImageProxy", StringComparison.OrdinalIgnoreCase))
        {
            return imageUrl;
        }

        // Otherwise return as-is (could be an external URL or placeholder)
        return imageUrl;
    }

    protected string WidthStyle => Width.HasValue ? $"{Width}px" : "100%";

    protected string HeightStyle => Height.HasValue ? $"{Height}px" : "100%";

    protected string CombinedClass => $"async-image-fallback {Class}";

    protected string FallbackStyle => $"width: {WidthStyle}; height: {HeightStyle}; display: flex; align-items: center; justify-content: center; background-color: var(--mud-palette-surface);";

    protected override void OnInitialized()
    {
        _cts = new CancellationTokenSource();
    }

    protected override void OnParametersSet()
    {
        // Track if URL changed - actual loading happens in OnAfterRenderAsync
        if (ImageUrl != _currentImageUrl)
        {
            _currentImageUrl = ImageUrl;
            _shouldLoadImage = true;
        }
    }

    private bool _shouldLoadImage;

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (!UseAsyncLoading || string.IsNullOrWhiteSpace(ImageUrl))
        {
            return Task.CompletedTask;
        }

        // Only load when we have a new image URL to load
        if (!_shouldLoadImage)
        {
            return Task.CompletedTask;
        }

        _shouldLoadImage = false;

        // Fire and forget - don't await, let the image load in background
        // Note: We don't await because we want the render to complete immediately
        _ = LoadImageInBackgroundAsync();
        return Task.CompletedTask;
    }

    private async Task LoadImageInBackgroundAsync()
    {
        if (_isDisposed || _cts is null || string.IsNullOrWhiteSpace(ImageUrl))
        {
            return;
        }

        // Clean up previous ViewModel
        if (_imageViewModel is not null)
        {
            _imageViewModel.StateChanged -= OnImageStateChanged;
            _imageViewModel.Dispose();
        }

        // Async loading requires remoteId and fetchUrl which are now embedded in proxy URLs
        // For now, async loading via Blazor SignalR is not supported with the new image system
        // Use UseAsyncLoading=false to use browser-based loading via the ImageProxy controller
        Logger.LogWarning("Async image loading via Blazor is not supported with proxy URLs. Use UseAsyncLoading=false. URL: {ImageUrl}", ImageUrl);
        return;
    }

    private void OnImageStateChanged(object? sender, EventArgs e)
    {
        _ = InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        if (_cts is not null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        if (_imageViewModel is not null)
        {
            _imageViewModel.StateChanged -= OnImageStateChanged;
            _imageViewModel.Dispose();
            _imageViewModel = null;
        }
    }
}
