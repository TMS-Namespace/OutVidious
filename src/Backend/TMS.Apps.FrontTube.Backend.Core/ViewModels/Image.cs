using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Core.Enums;
using TMS.Apps.FrontTube.Backend.Core.Tools;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Models;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;

namespace TMS.Apps.FrontTube.Backend.Core.ViewModels;

/// <summary>
/// ViewModel for managing async image loading with caching.
/// Wraps ImageEntity internally and handles the memory → DB → web caching pattern.
/// </summary>
public sealed class Image : ViewModelBase
{
    private readonly ILogger<Image> _logger;
    private CancellationTokenSource? _loadCts;
    private bool _disposed;

    internal CacheResult<ImageEntity> CacheResult { get; }

    internal Image(
        Super super,
        CacheResult<ImageEntity> cacheResult)
        : base(super)
    {
        _logger = Super.LoggerFactory.CreateLogger<Image>();
        CacheResult = cacheResult ?? throw new ArgumentNullException(nameof(cacheResult));
    }

    /// <summary>
    /// Event raised when the image loading state changes.
    /// </summary>
    public event EventHandler? StateChanged;

    /// <summary>
    /// The original URL of the image (e.g., YouTube CDN URL).
    /// This is the unique identifier for the image.
    /// </summary>
    public Uri? OriginalUrl => CacheResult.Entity?.AbsoluteRemoteUrl != null 
        ? new Uri(CacheResult.Entity.AbsoluteRemoteUrl) 
        : null;

    /// <summary>
    /// The hash of the original URL, used as cache key.
    /// </summary>
    public long? Hash => CacheResult.Entity?.Hash;

    /// <summary>
    /// The current loading state.
    /// </summary>
    public LoadingState LoadState { get; private set; } = LoadingState.NotLoaded;

    /// <summary>
    /// The loaded image data as a data URL (base64 encoded).
    /// </summary>
    public string? DataUrl { get; private set; }

    /// <summary>
    /// The binary image data (only available after loading).
    /// </summary>
    public byte[]? Data => CacheResult.Entity?.Data;

    /// <summary>
    /// The MIME type of the image (only available after loading).
    /// </summary>
    public string? MimeType => CacheResult.Entity?.MimeType;

    /// <summary>
    /// Image width in pixels (only available after loading).
    /// </summary>
    public int? Width => CacheResult.Entity?.Width;

    /// <summary>
    /// Image height in pixels (only available after loading).
    /// </summary>
    public int? Height => CacheResult.Entity?.Height;

    /// <summary>
    /// Gets the URL to display - either the loaded data URL or the original URL.
    /// </summary>
    public string? DisplayUrl => DataUrl ?? CacheResult.Entity?.AbsoluteRemoteUrl;

    public string? AbsoluteRemoteUrl => CacheResult.Entity?.AbsoluteRemoteUrl;

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

    public override void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        CancelPendingLoad();
        _disposed = true;
    }
}
