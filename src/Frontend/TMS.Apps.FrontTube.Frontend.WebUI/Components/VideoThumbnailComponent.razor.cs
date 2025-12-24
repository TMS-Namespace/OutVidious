using Microsoft.AspNetCore.Components;
using TMS.Apps.FrontTube.Backend.Core.ViewModels;
using TMS.Apps.FrontTube.Frontend.WebUI.Services;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Components;

/// <summary>
/// Component for displaying a single video thumbnail with metadata.
/// </summary>
public partial class VideoThumbnailComponentBase : ComponentBase
{
    private readonly string _containerId = $"thumb-{Guid.NewGuid():N}";

    [Inject]
    private Orchestrator Orchestrator { get; set; } = default!;

    /// <summary>
    /// The video to display.
    /// </summary>
    [Parameter]
    public Video? Video { get; set; }

    /// <summary>
    /// Whether to show the channel name.
    /// </summary>
    [Parameter]
    public bool ShowChannelName { get; set; } = true;

    /// <summary>
    /// Card width in pixels. If null, fills container.
    /// </summary>
    [Parameter]
    public int? Width { get; set; }

    /// <summary>
    /// Callback when the video is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<Video> OnVideoClick { get; set; }

    /// <summary>
    /// Callback when the channel name is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnChannelClicked { get; set; }

    protected string ThumbnailContainerId => _containerId;

    protected string ThumbnailUrl => GetBestThumbnail();

    protected string FormattedDuration => FormatDuration(Video?.Duration ?? TimeSpan.Zero);

    protected string ChannelUrl => $"/channel/{Video?.ChannelRemoteId}";

    protected string CardStyle => Width.HasValue 
        ? $"width: {Width}px; cursor: pointer;" 
        : "cursor: pointer;";

    protected async Task OnChannelClick()
    {
        if (!string.IsNullOrEmpty(Video?.ChannelRemoteId))
        {
            await OnChannelClicked.InvokeAsync(Video.ChannelRemoteId);
        }
    }

    private string GetBestThumbnail()
    {
        if (Video == null)
        {
            return "/images/placeholder-video.png";
        }

        var thumbnailUrl = Video.GetBestThumbnailUrl();
        if (thumbnailUrl == null)
        {
            return "/images/placeholder-video.png";
        }

        return Orchestrator.Super.BuildImageProxyUrl(new Uri(thumbnailUrl));
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
        {
            return $"{(int)duration.TotalHours}:{duration.Minutes:D2}:{duration.Seconds:D2}";
        }

        return $"{duration.Minutes}:{duration.Seconds:D2}";
    }
}
