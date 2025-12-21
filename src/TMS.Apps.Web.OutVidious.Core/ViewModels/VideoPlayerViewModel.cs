using Microsoft.Extensions.Logging;
using TMS.Apps.Web.OutVidious.Core.Enums;
using TMS.Apps.Web.OutVidious.Core.Interfaces;
using TMS.Apps.Web.OutVidious.Core.Models;

namespace TMS.Apps.Web.OutVidious.Core.ViewModels;

/// <summary>
/// ViewModel for managing video player state and interactions.
/// </summary>
public sealed class VideoPlayerViewModel : IDisposable
{
    private readonly IInvidiousApiService _apiService;
    private readonly ILogger<VideoPlayerViewModel> _logger;
    private CancellationTokenSource? _loadCts;
    private bool _disposed;

    public VideoPlayerViewModel(IInvidiousApiService apiService, ILogger<VideoPlayerViewModel> logger)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Event raised when the video state changes.
    /// </summary>
    public event EventHandler? StateChanged;

    /// <summary>
    /// Gets the current video details.
    /// </summary>
    public VideoDetails? VideoDetails { get; private set; }

    /// <summary>
    /// Gets the current video ID.
    /// </summary>
    public string? VideoId { get; private set; }

    /// <summary>
    /// Gets the current loading state.
    /// </summary>
    public VideoLoadState LoadState { get; private set; } = VideoLoadState.NotLoaded;

    /// <summary>
    /// Gets the error message if loading failed.
    /// </summary>
    public string? ErrorMessage { get; private set; }

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
    /// Gets the available quality options.
    /// </summary>
    public IReadOnlyList<string> AvailableQualities { get; private set; } = [];

    /// <summary>
    /// Loads a video by its ID.
    /// </summary>
    public async Task LoadVideoAsync(string videoId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(videoId))
        {
            _logger.LogWarning("LoadVideoAsync called with empty videoId");
            return;
        }

        _logger.LogInformation("Loading video with ID: {VideoId}", videoId);

        // Cancel any existing load operation
        await CancelCurrentLoadAsync();
        _loadCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        VideoId = videoId;
        LoadState = VideoLoadState.Loading;
        ErrorMessage = null;
        OnStateChanged();

        try
        {
            VideoDetails = await _apiService.GetVideoDetailsAsync(videoId, _loadCts.Token);

            if (VideoDetails != null)
            {
                UpdateAvailableQualities();
                UpdateStreamUrl();
                UpdateEmbedUrl();
                LoadState = VideoLoadState.Loaded;
                _logger.LogInformation("Successfully loaded video: {Title}", VideoDetails.Title);
            }
            else
            {
                LoadState = VideoLoadState.Error;
                ErrorMessage = "Video not found or unavailable.";
                _logger.LogWarning("Video not found: {VideoId}", videoId);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Video load cancelled for: {VideoId}", videoId);
            LoadState = VideoLoadState.NotLoaded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load video: {VideoId}", videoId);
            LoadState = VideoLoadState.Error;
            ErrorMessage = $"Failed to load video: {ex.Message}";
        }

        OnStateChanged();
    }

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
        if (VideoDetails?.FormatStreams == null || VideoDetails.FormatStreams.Count == 0)
        {
            AvailableQualities = [];
            SelectedQuality = null;
            return;
        }

        AvailableQualities = VideoDetails.FormatStreams
            .Select(f => f.QualityLabel)
            .Distinct()
            .ToList();

        // Default to highest quality
        SelectedQuality = AvailableQualities.FirstOrDefault();
    }

    private void UpdateStreamUrl()
    {
        if (VideoDetails?.FormatStreams == null || string.IsNullOrEmpty(SelectedQuality))
        {
            CurrentStreamUrl = null;
            return;
        }

        var selectedStream = VideoDetails.FormatStreams
            .FirstOrDefault(f => f.QualityLabel == SelectedQuality);

        CurrentStreamUrl = selectedStream?.Url;
        _logger.LogDebug("Stream URL updated for quality {Quality}: {HasUrl}", SelectedQuality, CurrentStreamUrl != null);
    }

    private void UpdateEmbedUrl()
    {
        if (string.IsNullOrEmpty(VideoId))
        {
            EmbedUrl = null;
            return;
        }

        EmbedUrl = _apiService.GetEmbedUrl(VideoId);
        _logger.LogDebug("Embed URL updated: {EmbedUrl}", EmbedUrl);
    }

    private async Task CancelCurrentLoadAsync()
    {
        if (_loadCts != null)
        {
            await _loadCts.CancelAsync();
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

        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _disposed = true;
    }
}
