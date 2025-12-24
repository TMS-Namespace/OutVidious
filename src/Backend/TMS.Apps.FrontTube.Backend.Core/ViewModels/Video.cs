using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Core.Enums;

namespace TMS.Apps.FrontTube.Backend.Core.ViewModels;

/// <summary>
/// ViewModel for managing video data and player state.
/// Can wrap either full Video or VideoMetadata (summary).
/// </summary>
public sealed class Video : IDisposable
{
    private readonly Super _super;
    private readonly ILogger<Video> _logger;
    private readonly Common.ProviderCore.Contracts.Video? _videoInfo;
    private readonly VideoMetadata? _metadata;
    private bool _disposed;

    /// <summary>
    /// Creates a Video ViewModel wrapping full video info.
    /// </summary>
    internal Video(Super super, ILoggerFactory loggerFactory, Common.ProviderCore.Contracts.Video videoInfo)
    {
        _super = super ?? throw new ArgumentNullException(nameof(super));
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<Video>();
        
        _videoInfo = videoInfo ?? throw new ArgumentNullException(nameof(videoInfo));
        RemoteId = videoInfo.RemoteId;
        Title = videoInfo.Title;
        Description = videoInfo.DescriptionText;
        Duration = videoInfo.Duration;
        ViewCount = videoInfo.ViewCount;
        ViewCountText = videoInfo.ViewCountText;
        LikeCount = videoInfo.LikeCount;
        PublishedAt = videoInfo.PublishedAt;
        PublishedAgo = videoInfo.PublishedAgo;
        Thumbnails = videoInfo.Thumbnails;
        ChannelName = videoInfo.Channel.Name;
        ChannelRemoteId = videoInfo.Channel.RemoteId;
        ChannelAvatars = videoInfo.Channel.Avatars;
        IsLive = videoInfo.IsLive;
        IsUpcoming = videoInfo.IsUpcoming;
        LoadState = VideoLoadState.Loaded;

        UpdateAvailableQualities();
        UpdateDashQualities();
        UpdateStreamUrl();
        UpdateEmbedUrl();
        UpdateDashManifestUrl();

        _logger.LogDebug("Video ViewModel created for full video: {Title}", videoInfo.Title);
    }

    /// <summary>
    /// Creates a Video ViewModel wrapping video metadata (summary).
    /// </summary>
    internal Video(Super super, ILoggerFactory loggerFactory, VideoMetadata metadata)
    {
        _super = super ?? throw new ArgumentNullException(nameof(super));
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<Video>();
        
        _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        RemoteId = metadata.RemoteId;
        Title = metadata.Title;
        Description = null;
        Duration = metadata.Duration;
        ViewCount = metadata.ViewCount;
        ViewCountText = metadata.ViewCountText;
        LikeCount = 0;
        PublishedAt = metadata.PublishedAt;
        PublishedAgo = metadata.PublishedAgo;
        Thumbnails = metadata.Thumbnails;
        ChannelName = metadata.Channel.Name;
        ChannelRemoteId = metadata.Channel.RemoteId;
        ChannelAvatars = metadata.Channel.Avatars;
        IsLive = metadata.IsLive;
        IsUpcoming = metadata.IsUpcoming;
        IsShort = metadata.IsShort;
        LoadState = VideoLoadState.NotLoaded;

        _logger.LogDebug("Video ViewModel created for metadata: {Title}", metadata.Title);
    }

    /// <summary>
    /// Event raised when the video state changes.
    /// </summary>
    public event EventHandler? StateChanged;

    /// <summary>
    /// The video's remote identifier.
    /// </summary>
    public string RemoteId { get; }

    /// <summary>
    /// The video title.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// The video description.
    /// </summary>
    public string Description { get; } = string.Empty;

    /// <summary>
    /// The video duration.
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// The view count.
    /// </summary>
    public long ViewCount { get; }

    /// <summary>
    /// The like count.
    /// </summary>
    public long LikeCount { get; }

    /// <summary>
    /// The formatted view count text.
    /// </summary>
    [Obsolete("Use ViewCount and format on client side")]
    public string? ViewCountText { get; }

    /// <summary>
    /// The date/time the video was published.
    /// </summary>
    public DateTimeOffset? PublishedAt { get; }

    /// <summary>
    /// Human-friendly "published ago" text.
    /// </summary>
    [Obsolete("Use PublishedAt and format on client side")]
    public string? PublishedAgo { get; }

    /// <summary>
    /// Video thumbnails.
    /// </summary>
    public IReadOnlyList<Common.ProviderCore.Contracts.Image> Thumbnails { get; }

    /// <summary>
    /// Channel name.
    /// </summary>
    public string ChannelName { get; }

    /// <summary>
    /// Channel remote ID.
    /// </summary>
    public string ChannelRemoteId { get; }

    /// <summary>
    /// Channel avatar images.
    /// </summary>
    public IReadOnlyList<Common.ProviderCore.Contracts.Image> ChannelAvatars { get; }

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
        return thumbnail?.RemoteUrl.ToString();
    }
    /// <summary>
    /// Gets the current player mode.
    /// </summary>
    public PlayerMode PlayerMode { get; private set; } = PlayerMode.Native;

    /// <summary>
    /// Gets the selected video quality.
    /// </summary>
    public string? SelectedQuality { get; private set; }

    /// <summary>
    /// Gets the current stream URL for native player mode.
    /// </summary>
    public string? CurrentStreamUrl { get; private set; }

    /// <summary>
    /// Gets the embed URL for embedded player mode.
    /// </summary>
    public string? EmbedUrl { get; private set; }

    /// <summary>
    /// Gets the DASH manifest URL for adaptive streaming with Shaka Player.
    /// </summary>
    public string? DashManifestUrl { get; private set; }

    /// <summary>
    /// Gets the available quality options.
    /// </summary>
    public IReadOnlyList<string> AvailableQualities { get; private set; } = [];

    /// <summary>
    /// Gets the available DASH quality options (higher quality, separate audio/video).
    /// </summary>
    public IReadOnlyList<string> AvailableDashQualities { get; private set; } = [];

    /// <summary>
    /// Sets the player mode.
    /// </summary>
    public void SetPlayerMode(PlayerMode mode)
    {
        if (PlayerMode == mode)
        {
            return;
        }

        _logger.LogDebug("Player mode changed from {OldMode} to {NewMode}", PlayerMode, mode);
        PlayerMode = mode;
        OnStateChanged();
    }

    /// <summary>
    /// Sets the video quality for native player mode.
    /// </summary>
    public void SetQuality(string quality)
    {
        if (SelectedQuality == quality)
        {
            return;
        }

        _logger.LogDebug("Quality changed from {OldQuality} to {NewQuality}", SelectedQuality, quality);
        SelectedQuality = quality;
        UpdateStreamUrl();
        OnStateChanged();
    }

    private void UpdateAvailableQualities()
    {
        if (_videoInfo?.CombinedStreams.Count == 0)
        {
            AvailableQualities = [];
            SelectedQuality = null;
            return;
        }

        AvailableQualities = _videoInfo?.CombinedStreams
            .Where(s => !string.IsNullOrEmpty(s.QualityLabel))
            .Select(s => s.QualityLabel!)
            .Distinct()
            .ToList() ?? [];

        // Default to highest quality
        SelectedQuality = AvailableQualities.FirstOrDefault();
    }

    private void UpdateStreamUrl()
    {
        if (_videoInfo?.CombinedStreams.Count == 0 || string.IsNullOrEmpty(SelectedQuality))
        {
            CurrentStreamUrl = null;
            return;
        }

        var selectedStream = _videoInfo?.CombinedStreams
            .FirstOrDefault(s => s.QualityLabel == SelectedQuality);

        CurrentStreamUrl = selectedStream?.RemoteUrl.ToString();
        _logger.LogDebug("Stream URL updated for quality {Quality}: {HasUrl}", SelectedQuality, CurrentStreamUrl != null);
    }

    private void UpdateEmbedUrl()
    {
        if (string.IsNullOrEmpty(RemoteId))
        {
            EmbedUrl = null;
            return;
        }

        EmbedUrl = _super.VideoProvider.GetEmbedUrl(RemoteId).ToString();
        _logger.LogDebug("Embed URL updated: {EmbedUrl}", EmbedUrl);
    }

    private void UpdateDashManifestUrl()
    {
        if (string.IsNullOrEmpty(RemoteId))
        {
            DashManifestUrl = null;
            return;
        }

        // Use proxied URL to avoid CORS issues
        var proxiedUrl = _super.Proxy.ProxyDashManifestLocalUrl(RemoteId);
        DashManifestUrl = proxiedUrl.ToString();
        _logger.LogDebug("DASH manifest URL updated: {DashUrl}", DashManifestUrl);
    }

    private void UpdateDashQualities()
    {
        if (_videoInfo?.AdaptiveStreams.Count == 0)
        {
            AvailableDashQualities = [];
            return;
        }

        // Get video-only adaptive formats and extract quality labels
        AvailableDashQualities = _videoInfo?.AdaptiveStreams
            .Where(s => !string.IsNullOrEmpty(s.QualityLabel) && s.Type == StreamType.Video)
            .Select(s => s.QualityLabel!)
            .Distinct()
            .OrderByDescending(ParseQualityHeight)
            .ToList() ?? [];

        _logger.LogDebug("Available DASH qualities: {Qualities}", string.Join(", ", AvailableDashQualities));
    }

    private static int ParseQualityHeight(string qualityLabel)
    {
        // Parse quality labels like "1080p", "720p60", "1440p", "2160p60" etc.
        var numericPart = new string(qualityLabel.TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(numericPart, out var height) ? height : 0;
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

        _disposed = true;
    }
}
