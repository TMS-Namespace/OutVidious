using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Repository.CacheManager.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Core.ViewModels;

/// <summary>
/// Represents the loading state of an image.
/// </summary>
public enum ImageLoadState
{
    /// <summary>
    /// Image has not started loading.
    /// </summary>
    NotLoaded,

    /// <summary>
    /// Image is currently being loaded.
    /// </summary>
    Loading,

    /// <summary>
    /// Image has been successfully loaded.
    /// </summary>
    Loaded,

    /// <summary>
    /// Image loading failed.
    /// </summary>
    Failed
}

/// <summary>
/// ViewModel for managing async image loading with caching.
/// Handles the memory → DB → web caching pattern via repository.
/// </summary>
public sealed class Image : IDisposable
{
    private readonly Super _super;
    private readonly ILogger<Image> _logger;
    private CancellationTokenSource? _loadCts;
    private bool _disposed;

    /// <summary>
    /// Creates a new ImageViewModel for loading an image.
    /// </summary>
    /// <param name="super">The parent Super ViewModel.</param>
    /// <param name="logger">Logger for debugging.</param>
    /// <param name="originalUrl">The original URL of the image (YouTube CDN URL).</param>
    /// <param name="fetchUrl">The URL to fetch the image from (may be provider proxy).</param>
    /// <param name="placeholderDataUrl">Optional placeholder data URL to show while loading.</param>
    public Image(
        Super super,
        ILogger<Image> logger,
        Uri originalUrl,
        Uri fetchUrl,
        string? placeholderDataUrl = null)
    {
        _super = super ?? throw new ArgumentNullException(nameof(super));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        OriginalUrl = originalUrl ?? throw new ArgumentNullException(nameof(originalUrl));
        FetchUrl = fetchUrl ?? throw new ArgumentNullException(nameof(fetchUrl));
        PlaceholderDataUrl = placeholderDataUrl;
    }

    /// <summary>
    /// Event raised when the image loading state changes.
    /// </summary>
    public event EventHandler? StateChanged;

    /// <summary>
    /// The original URL of the image (e.g., YouTube CDN URL).
    /// This is the unique identifier for the image.
    /// </summary>
    public Uri OriginalUrl { get; }

    /// <summary>
    /// The URL to fetch the image from (may be different from OriginalUrl, e.g., provider proxy).
    /// </summary>
    public Uri FetchUrl { get; }

    /// <summary>
    /// Optional placeholder to show while loading.
    /// </summary>
    public string? PlaceholderDataUrl { get; }

    /// <summary>
    /// The current loading state.
    /// </summary>
    public ImageLoadState LoadState { get; private set; } = ImageLoadState.NotLoaded;

    /// <summary>
    /// The loaded image data as a data URL (base64 encoded).
    /// </summary>
    public string? DataUrl { get; private set; }

    /// <summary>
    /// The cached image data.
    /// </summary>
    public CachedImage? CachedImage { get; private set; }

    /// <summary>
    /// Gets the URL to display - either the loaded data URL, placeholder, or fetch URL.
    /// </summary>
    public string DisplayUrl => DataUrl ?? PlaceholderDataUrl ?? FetchUrl.ToString();

    /// <summary>
    /// Loads the image asynchronously.
    /// </summary>
    /// <param name="cancellationToken">External cancellation token.</param>
    /// <returns>True if loading succeeded.</returns>
    public async Task<bool> LoadAsync(CancellationToken cancellationToken)
    {
        if (_disposed)
        {
            return false;
        }

        // Cancel any pending load
        CancelPendingLoad();

        _loadCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = _loadCts.Token;

        LoadState = ImageLoadState.Loading;
        OnStateChanged();

        try
        {
            _logger.LogDebug("Loading image: {OriginalUrl}", OriginalUrl);

            var cachedImage = await _super.GetImageAsync(OriginalUrl, FetchUrl, token);

            if (cachedImage is null)
            {
                _logger.LogWarning("Failed to load image: {OriginalUrl}", OriginalUrl);
                LoadState = ImageLoadState.Failed;
                OnStateChanged();
                return false;
            }

            CachedImage = cachedImage;
            DataUrl = $"data:{cachedImage.MimeType};base64,{Convert.ToBase64String(cachedImage.Data)}";
            LoadState = ImageLoadState.Loaded;

            _logger.LogDebug("Image loaded successfully: {OriginalUrl}, size: {Size} bytes", 
                OriginalUrl, cachedImage.Data.Length);

            OnStateChanged();
            return true;
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Image loading cancelled: {OriginalUrl}", OriginalUrl);
            LoadState = ImageLoadState.NotLoaded;
            OnStateChanged();
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading image: {OriginalUrl}", OriginalUrl);
            LoadState = ImageLoadState.Failed;
            OnStateChanged();
            return false;
        }
    }

    /// <summary>
    /// Cancels any pending image load operation.
    /// </summary>
    public void CancelPendingLoad()
    {
        if (_loadCts is not null && !_loadCts.IsCancellationRequested)
        {
            _loadCts.Cancel();
            _loadCts.Dispose();
            _loadCts = null;
        }
    }

    private void OnStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        CancelPendingLoad();
        _disposed = true;
    }
}
