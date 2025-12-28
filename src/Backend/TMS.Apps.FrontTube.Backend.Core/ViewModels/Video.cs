using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Core.Enums;
using TMS.Apps.FrontTube.Backend.Core.Tools;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Enums;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Models;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;

namespace TMS.Apps.FrontTube.Backend.Core.ViewModels;

/// <summary>
/// ViewModel for managing video data and player state.
/// Can wrap either full Video or VideoMetadata (summary).
/// </summary>
public sealed class Video : ViewModelBase
{
    private readonly ILogger<Video> _logger;
    private bool _disposed;
    private Streams? _streams;

    internal CacheResult<VideoEntity> CacheResult { get; }
    
    internal Video(
        Super super, 
        Channel channel, 
        CacheResult<VideoEntity> videoCachingResult, 
        List<Image> thumbnails)
        : base(super)
    {
        _logger = super.LoggerFactory.CreateLogger<Video>();

        CacheResult = videoCachingResult ?? throw new ArgumentNullException(nameof(videoCachingResult));

        Channel = channel ?? throw new ArgumentNullException(nameof(channel));

        Thumbnails = thumbnails;

        // Set LoadState based on cache result
        LoadState = videoCachingResult.Status == EntityStatus.Error 
            ? VideoLoadState.Error 
            : VideoLoadState.Loaded;
        
        ErrorMessage = videoCachingResult.Error;
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
                _logger.LogDebug("Streams VM initialized with {Count} stream entities", 
                    CacheResult.Entity?.Streams?.Count ?? 0);
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
    public Uri AbsoluteRemoteUrl => CacheResult.Identity.AbsoluteRemoteUrl;

    /// <summary>
    /// The video title.
    /// </summary>
    public string Title => CacheResult.Entity!.Title;

    /// <summary>
    /// The video description.
    /// </summary>
    public string Description => CacheResult.Entity!.Description;

    /// <summary>
    /// The video duration.
    /// </summary>
    public TimeSpan Duration => TimeSpan.FromSeconds(CacheResult.Entity!.DurationSeconds);

    /// <summary>
    /// The view count.
    /// </summary>
    public long ViewCount => CacheResult.Entity!.ViewCount;

    /// <summary>
    /// The like count.
    /// </summary>
    public long? LikesCount => CacheResult.Entity!.LikesCount;

    /// <summary>
    /// The formatted view count text.
    /// </summary>
    [Obsolete("Use ViewCount and format on client side")]
    public string? ViewCountText { get; }

    /// <summary>
    /// The date/time the video was published.
    /// </summary>
    public DateTimeOffset? PublishedAt => CacheResult.Entity!.PublishedAt;

    /// <summary>
    /// Human-friendly "published ago" text.
    /// </summary>
    [Obsolete("Use PublishedAt and format on client side")]
    public string? PublishedAgo { get; }

    /// <summary>
    /// Video thumbnails.
    /// </summary>
    public IReadOnlyList<Image> Thumbnails { get; }

    /// <summary>
    /// Channel name.
    /// </summary>
    [Obsolete("Use Channel.Name from Channel property")]
    public string ChannelName { get; }

    public Channel Channel { get; init; }

    /// <summary>
    /// Channel absolute remote URL.
    /// </summary>
    [Obsolete("Use Channel.AbsoluteRemoteUrl from Channel property")]
    public Uri ChannelAbsoluteRemoteUrl { get; }

    /// <summary>
    /// Channel avatar images.
    /// </summary>
    [Obsolete("Use Channel.Avatars from Channel property")]
    public IReadOnlyList<Common.ProviderCore.Contracts.ImageMetadata> ChannelAvatars { get; }

    /// <summary>
    /// Whether this is a short-form video.
    /// </summary>
    public bool IsShort { get; }

    /// <summary>
    /// Whether the video is a live stream.
    /// </summary>
    public bool IsLive { get; }

    /// <summary>
    /// Whether the video is an upcoming premiere.
    /// </summary>
    public bool IsUpcoming { get; }

    /// <summary>
    /// Gets the current loading state.
    /// </summary>
    public VideoLoadState LoadState { get; }

    /// <summary>
    /// Gets the error message if loading failed.
    /// </summary>
    public string? ErrorMessage { get; }
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
