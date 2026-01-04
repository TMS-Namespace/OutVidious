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

    protected string ChannelUrl => Video?.Channel?.RemoteIdentity is null
        ? "#"
        : Video.Channel.RemoteIdentity.GetProxyUrl(Orchestrator.Super.Proxy);

    protected string CardStyle => Width.HasValue 
        ? $"width: {Width}px; cursor: pointer;" 
        : "cursor: pointer;";

    protected async Task OnChannelClick()
    {
        if (Video?.Channel?.RemoteIdentity is not null)
        {
            await OnChannelClicked.InvokeAsync(Video.Channel.RemoteIdentity.GetProxyUrl(Orchestrator.Super.Proxy));
        }
    }

    private string GetBestThumbnail()
    {
        if (Video == null)
        {
            return "/images/placeholder-video.png";
        }

        var thumbnailIdentity = Video.GetBestThumbnailIdentity();
        if (thumbnailIdentity is null)
        {
            return "/images/placeholder-video.png";
        }

        var proxyUrl = thumbnailIdentity.GetProxyUrl(Orchestrator.Super.Proxy);
        return string.IsNullOrWhiteSpace(proxyUrl) ? "/images/placeholder-video.png" : proxyUrl;
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
