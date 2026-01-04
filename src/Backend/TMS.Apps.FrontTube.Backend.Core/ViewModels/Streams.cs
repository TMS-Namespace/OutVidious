using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Core.Enums;
using TMS.Apps.FrontTube.Backend.Core.Tools;
using TMS.Apps.FrontTube.Backend.Repository.Data.Contracts;

namespace TMS.Apps.FrontTube.Backend.Core.ViewModels;

/// <summary>
/// ViewModel for managing video streams, playback modes, and quality selection.
/// </summary>
public sealed class Streams : ViewModelBase
{
    private const int StreamTypeVideo = 1;
    private const int StreamTypeAudio = 2;
    private const int StreamTypeMutex = 3;

    private readonly ILogger<Streams> _logger;
    private readonly Video _videoViewModel;
    private readonly List<StreamDomain> _streamDomains;
    private readonly Uri _videoAbsoluteRemoteUrl;
    private bool _disposed;

    /// <summary>
    /// Event raised when stream state changes (e.g., quality or mode changed).
    /// </summary>
    public event EventHandler? StateChanged;

    internal Streams(
        Super super,
        Video videoViewModel)
        : base(super)
    {
        _logger = super.LoggerFactory.CreateLogger<Streams>();
        _videoViewModel = videoViewModel ?? throw new ArgumentNullException(nameof(videoViewModel));
        _streamDomains = _videoViewModel.Domain.Streams.ToList();
        _videoAbsoluteRemoteUrl = _videoViewModel.AbsoluteRemoteUrl;

        Initialize();
    }

    /// <summary>
    /// Gets the current player mode.
    /// </summary>
    public PlayerMode PlayerMode { get; private set; } = PlayerMode.Native;

    /// <summary>
    /// Gets the selected video quality for native player mode.
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
    /// Gets the available quality options for native player mode (muxed streams).
    /// </summary>
    public IReadOnlyList<string> AvailableQualities { get; private set; } = [];

    /// <summary>
    /// Gets the available DASH quality options (higher quality, separate audio/video).
    /// </summary>
    public IReadOnlyList<string> AvailableDashQualities { get; private set; } = [];

    private List<StreamDomain> _mutexStreams = [];
    private List<StreamDomain> _adaptiveStreams = [];

    /// <summary>
    /// Sets the player mode.
    /// </summary>
    /// <param name="mode">The player mode to set.</param>
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
    /// <param name="quality">The quality label to set.</param>
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

    private void Initialize()
    {
        _mutexStreams = _streamDomains
            .Where(s => s.StreamTypeId == StreamTypeMutex)
            .ToList();

        _adaptiveStreams = _streamDomains
            .Where(s => s.StreamTypeId == StreamTypeVideo ||
                        s.StreamTypeId == StreamTypeAudio)
            .ToList();

        UpdateAvailableQualities();
        UpdateStreamUrl();
        UpdateEmbedUrl();
        UpdateDashManifestUrl();
        UpdateDashQualities();

        _logger.LogDebug(
            "Streams initialized: {MutexCount} muxed, {AdaptiveCount} adaptive",
            _mutexStreams.Count,
            _adaptiveStreams.Count);
    }

    private void UpdateAvailableQualities()
    {
        if (_mutexStreams.Count == 0)
        {
            AvailableQualities = [];
            SelectedQuality = null;
            _logger.LogDebug("No muxed streams available for native playback");
            return;
        }

        AvailableQualities = _mutexStreams
            .Where(s => !string.IsNullOrEmpty(s.QualityLabel))
            .Select(s => s.QualityLabel!)
            .Distinct()
            .OrderByDescending(ParseQualityHeight)
            .ToList();

        SelectedQuality = AvailableQualities.FirstOrDefault();

        _logger.LogDebug(
            "Available qualities: {Qualities}, selected: {Selected}",
            string.Join(", ", AvailableQualities),
            SelectedQuality);
    }

    private void UpdateStreamUrl()
    {
        if (_mutexStreams.Count == 0 || string.IsNullOrEmpty(SelectedQuality))
        {
            CurrentStreamUrl = null;
            _logger.LogDebug("No stream URL available: MutexCount={Count}, Quality={Quality}",
                _mutexStreams.Count, SelectedQuality ?? "(null)");
            return;
        }

        var selectedStream = _mutexStreams
            .FirstOrDefault(s => s.QualityLabel == SelectedQuality);

        CurrentStreamUrl = selectedStream?.AbsoluteRemoteUrl.ToString();
        _logger.LogDebug(
            "Stream URL updated for quality {Quality}: {HasUrl}",
            SelectedQuality,
            CurrentStreamUrl != null);
    }

    private void UpdateEmbedUrl()
    {
        var videoId = YouTubeValidator.ExtractVideoIdFromUrl(_videoAbsoluteRemoteUrl);
        if (string.IsNullOrEmpty(videoId))
        {
            EmbedUrl = null;
            _logger.LogWarning("Cannot extract video ID from URL: {Url}", _videoAbsoluteRemoteUrl);
            return;
        }

        EmbedUrl = Super.Proxy.ProxyEmbedUrl(videoId).ToString();
        _logger.LogDebug("Embed URL updated: {EmbedUrl}", EmbedUrl);
    }

    private void UpdateDashManifestUrl()
    {
        var videoId = YouTubeValidator.ExtractVideoIdFromUrl(_videoAbsoluteRemoteUrl);
        if (string.IsNullOrEmpty(videoId))
        {
            DashManifestUrl = null;
            _logger.LogWarning("Cannot extract video ID from URL: {Url}", _videoAbsoluteRemoteUrl);
            return;
        }

        var proxiedUrl = Super.Proxy.ProxyDashManifestLocalUrl(videoId);
        DashManifestUrl = proxiedUrl.ToString();
        _logger.LogDebug("DASH manifest URL updated: {DashUrl}", DashManifestUrl);
    }

    private void UpdateDashQualities()
    {
        if (_adaptiveStreams.Count == 0)
        {
            AvailableDashQualities = [];
            _logger.LogDebug("No adaptive streams available for DASH playback");
            return;
        }

        AvailableDashQualities = _adaptiveStreams
            .Where(s => !string.IsNullOrEmpty(s.QualityLabel) &&
                       s.StreamTypeId == StreamTypeVideo)
            .Select(s => s.QualityLabel!)
            .Distinct()
            .OrderByDescending(ParseQualityHeight)
            .ToList();

        _logger.LogDebug("Available DASH qualities: {Qualities}", string.Join(", ", AvailableDashQualities));
    }

    private static int ParseQualityHeight(string qualityLabel)
    {
        var numericPart = new string(qualityLabel.TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(numericPart, out var height) ? height : 0;
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

        _disposed = true;
    }
}
