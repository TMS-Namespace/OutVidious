using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Core.Enums;
using TMS.Apps.FrontTube.Backend.Core.Tools;
using TMS.Apps.FrontTube.Backend.Repository.Data.Contracts;

namespace TMS.Apps.FrontTube.Backend.Core.ViewModels;

/// <summary>
/// ViewModel for managing video data and player state.
/// Wraps VideoDomain internally.
/// </summary>
public sealed class Video : ViewModelBase
{
    private readonly ILogger<Video> _logger;
    private bool _disposed;
    private Streams? _streams;

    internal VideoDomain Domain { get; private set; }

    internal Video(
        Super super,
        Channel channel,
        VideoDomain domain,
        IReadOnlyList<Image> thumbnails)
        : base(super)
    {
        _logger = super.LoggerFactory.CreateLogger<Video>();

        Domain = domain ?? throw new ArgumentNullException(nameof(domain));
        Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        Thumbnails = thumbnails ?? [];

        LoadState = string.IsNullOrWhiteSpace(Domain.FetchingError)
            ? VideoLoadState.Loaded
            : VideoLoadState.Error;

        ErrorMessage = Domain.FetchingError;
    }

    internal void UpdateFromDomain(VideoDomain domain, Channel channel, IReadOnlyList<Image> thumbnails)
    {
        Domain = domain ?? throw new ArgumentNullException(nameof(domain));
        Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        Thumbnails = thumbnails ?? [];

        LoadState = string.IsNullOrWhiteSpace(Domain.FetchingError)
            ? VideoLoadState.Loaded
            : VideoLoadState.Error;

        ErrorMessage = Domain.FetchingError;

        _streams?.Dispose();
        _streams = null;
    }

    /// <summary>
    /// Gets the streams manager for this video. Initialized lazily on first access.
    /// </summary>
    public Streams Streams
    {
        get
        {
            if (_streams == null)
            {
                _streams = new Streams(Super, this);
                _logger.LogDebug("Streams VM initialized with {Count} stream domains",
                    Domain.Streams.Count);
            }

            return _streams;
        }
    }

    /// <summary>
    /// Event raised when the video state changes.
    /// </summary>
    public event EventHandler? StateChanged;

    /// <summary>
    /// The video's absolute remote URL on the original platform.
    /// </summary>
    public Uri AbsoluteRemoteUrl => new Uri(Domain.AbsoluteRemoteUrl);

    /// <summary>
    /// The video title.
    /// </summary>
    public string Title => Domain.Title;

    /// <summary>
    /// The video description.
    /// </summary>
    public string Description => Domain.Description ?? string.Empty;

    /// <summary>
    /// The video duration.
    /// </summary>
    public TimeSpan Duration => TimeSpan.FromSeconds(Domain.DurationSeconds);

    /// <summary>
    /// The view count.
    /// </summary>
    public long ViewCount => Domain.ViewCount;

    /// <summary>
    /// The like count.
    /// </summary>
    public long? LikesCount => Domain.LikesCount;

    /// <summary>
    /// The formatted view count text.
    /// </summary>
    [Obsolete("Use ViewCount and format on client side")]
    public string? ViewCountText { get; }

    /// <summary>
    /// The date/time the video was published.
    /// </summary>
    public DateTimeOffset? PublishedAt => Domain.PublishedAt is null
        ? null
        : new DateTimeOffset(Domain.PublishedAt.Value, TimeSpan.Zero);

    /// <summary>
    /// Human-friendly "published ago" text.
    /// </summary>
    [Obsolete("Use PublishedAt and format on client side")]
    public string? PublishedAgo { get; }

    /// <summary>
    /// Video thumbnails.
    /// </summary>
    public IReadOnlyList<Image> Thumbnails { get; private set; }

    /// <summary>
    /// Channel name.
    /// </summary>
    [Obsolete("Use Channel.Name from Channel property")]
    public string ChannelName => Channel.Name;

    public Channel Channel { get; private set; }

    /// <summary>
    /// Channel absolute remote URL.
    /// </summary>
    [Obsolete("Use Channel.AbsoluteRemoteUrl from Channel property")]
    public Uri ChannelAbsoluteRemoteUrl => Channel.AbsoluteRemoteUrl;

    /// <summary>
    /// Channel avatar images.
    /// </summary>
    [Obsolete("Use Channel.Avatars from Channel property")]
    public IReadOnlyList<Image> ChannelAvatars => Channel.Avatars;

    /// <summary>
    /// Whether this is a short-form video.
    /// </summary>
    public bool IsShort => Domain.IsShort;

    /// <summary>
    /// Whether the video is a live stream.
    /// </summary>
    public bool IsLive => Domain.IsLive;

    /// <summary>
    /// Whether the video is an upcoming premiere.
    /// </summary>
    public bool IsUpcoming => Domain.IsUpcoming;

    /// <summary>
    /// Gets the current loading state.
    /// </summary>
    public VideoLoadState LoadState { get; private set; }

    /// <summary>
    /// Gets the error message if loading failed.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Gets the URL of the best available thumbnail for display.
    /// </summary>
    public string? GetBestThumbnailUrl()
    {
        if (Thumbnails.Count == 0)
        {
            return null;
        }

        var thumbnail = Thumbnails.OrderByDescending(t => t.Width).FirstOrDefault();
        return thumbnail?.AbsoluteRemoteUrl;
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

        _streams?.Dispose();
        _disposed = true;
    }
}
