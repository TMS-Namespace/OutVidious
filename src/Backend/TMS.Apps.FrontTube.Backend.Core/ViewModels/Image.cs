using TMS.Apps.FrontTube.Backend.Core.Enums;
using TMS.Apps.FrontTube.Backend.Core.Mappers;
using TMS.Apps.FrontTube.Backend.Core.Tools;
using TMS.Apps.FrontTube.Backend.Repository.Data.Contracts;

namespace TMS.Apps.FrontTube.Backend.Core.ViewModels;

/// <summary>
/// ViewModel for managing async image loading with caching.
/// Wraps ImageDomain internally and handles the memory → DB → web caching pattern.
/// </summary>
public sealed class Image : ViewModelBase
{
    private CancellationTokenSource? _loadCts;
    private bool _disposed;

    internal ImageDomain Domain { get; private set; }

    public RemoteIdentity RemoteIdentity { get; private set; }

    internal Image(
        Super super,
        ImageDomain domain)
        : base(super)
    {
        UpdateFromDomain(domain);
    }

    internal void UpdateFromDomain(ImageDomain domain)
    {
        Domain = domain ?? throw new ArgumentNullException(nameof(domain));
        RemoteIdentity = DomainViewModelMapper.ToViewModel(domain.RemoteIdentity);

        if (!string.IsNullOrWhiteSpace(Domain.FetchingError))
        {
            LoadState = LoadingState.Failed;
        }
    }

    /// <summary>
    /// Event raised when the image loading state changes.
    /// </summary>
    public event EventHandler? StateChanged;

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
    public byte[]? Data => Domain.Data;

    /// <summary>
    /// The MIME type of the image (only available after loading).
    /// </summary>
    public string? MimeType => Domain.MimeType;

    /// <summary>
    /// Image width in pixels (only available after loading).
    /// </summary>
    public int? Width => Domain.Width;

    /// <summary>
    /// Image height in pixels (only available after loading).
    /// </summary>
    public int? Height => Domain.Height;

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
