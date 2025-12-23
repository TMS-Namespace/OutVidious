using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Core.Enums;

namespace TMS.Apps.FrontTube.Backend.Core.ViewModels;

/// <summary>
/// ViewModel for managing video player state and interactions.
/// Wraps a VideoInfo contract loaded via Super.
/// </summary>
public sealed class Video : IDisposable
{
    private readonly Super _super;
    private readonly ILogger<Video> _logger;
    private bool _disposed;

    /// <summary>
    /// Creates a new VideoPlayerViewModel wrapping the provided video info.
    /// </summary>
    /// <param name="super">The parent Super ViewModel.</param>
    /// <param name="loggerFactory">Logger factory for creating loggers.</param>
    /// <param name="videoInfo">The video info contract to wrap.</param>
    public Video(Super super, ILoggerFactory loggerFactory, VideoInfo videoInfo)
    {
        _super = super ?? throw new ArgumentNullException(nameof(super));
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<Video>();
        
        VideoInfo = videoInfo ?? throw new ArgumentNullException(nameof(videoInfo));
        VideoId = videoInfo.VideoId;
        LoadState = VideoLoadState.Loaded;

        UpdateAvailableQualities();
        UpdateDashQualities();
        UpdateStreamUrl();
        UpdateEmbedUrl();
        UpdateDashManifestUrl();

        _logger.LogDebug("VideoPlayerViewModel created for: {Title}", videoInfo.Title);
    }

    /// <summary>
    /// Event raised when the video state changes.
    /// </summary>
    public event EventHandler? StateChanged;

    /// <summary>
    /// Gets the current video information.
    /// </summary>
    public VideoInfo VideoInfo { get; }

    /// <summary>
    /// Gets the current video ID.
    /// </summary>
    public string VideoId { get; }

    /// <summary>
    /// Gets the current loading state.
    /// </summary>
    public VideoLoadState LoadState { get; }

    /// <summary>
    /// Gets the error message if loading failed.
    /// </summary>
    public string? ErrorMessage { get; }

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
        if (VideoInfo.CombinedStreams.Count == 0)
        {
            AvailableQualities = [];
            SelectedQuality = null;
            return;
        }

        AvailableQualities = VideoInfo.CombinedStreams
            .Where(s => !string.IsNullOrEmpty(s.QualityLabel))
            .Select(s => s.QualityLabel!)
            .Distinct()
            .ToList();

        // Default to highest quality
        SelectedQuality = AvailableQualities.FirstOrDefault();
    }

    private void UpdateStreamUrl()
    {
        if (VideoInfo.CombinedStreams.Count == 0 || string.IsNullOrEmpty(SelectedQuality))
        {
            CurrentStreamUrl = null;
            return;
        }

        var selectedStream = VideoInfo.CombinedStreams
            .FirstOrDefault(s => s.QualityLabel == SelectedQuality);

        CurrentStreamUrl = selectedStream?.Url.ToString();
        _logger.LogDebug("Stream URL updated for quality {Quality}: {HasUrl}", SelectedQuality, CurrentStreamUrl != null);
    }

    private void UpdateEmbedUrl()
    {
        if (string.IsNullOrEmpty(VideoId))
        {
            EmbedUrl = null;
            return;
        }

        EmbedUrl = _super.VideoProvider.GetEmbedUrl(VideoId).ToString();
        _logger.LogDebug("Embed URL updated: {EmbedUrl}", EmbedUrl);
    }

    private void UpdateDashManifestUrl()
    {
        if (string.IsNullOrEmpty(VideoId))
        {
            DashManifestUrl = null;
            return;
        }

        // Use proxied URL to avoid CORS issues
        var proxiedUrl = _super.VideoProvider.GetProxiedDashManifestUrl(VideoId);
        DashManifestUrl = proxiedUrl?.ToString();
        _logger.LogDebug("DASH manifest URL updated: {DashUrl}", DashManifestUrl);
    }

    private void UpdateDashQualities()
    {
        if (VideoInfo.AdaptiveStreams.Count == 0)
        {
            AvailableDashQualities = [];
            return;
        }

        // Get video-only adaptive formats and extract quality labels
        AvailableDashQualities = VideoInfo.AdaptiveStreams
            .Where(s => !string.IsNullOrEmpty(s.QualityLabel) && s.Type == StreamType.Video)
            .Select(s => s.QualityLabel!)
            .Distinct()
            .OrderByDescending(ParseQualityHeight)
            .ToList();

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
