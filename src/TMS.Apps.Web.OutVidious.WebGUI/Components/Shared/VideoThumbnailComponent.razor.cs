using Microsoft.AspNetCore.Components;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Contracts;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Enums;

namespace TMS.Apps.Web.OutVidious.WebGUI.Components.Shared;

/// <summary>
/// Component for displaying a single video thumbnail with metadata.
/// </summary>
public partial class VideoThumbnailComponentBase : ComponentBase
{
    /// <summary>
    /// The video summary to display.
    /// </summary>
    [Parameter]
    public VideoSummary? Video { get; set; }

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
    public EventCallback<VideoSummary> OnVideoClick { get; set; }

    /// <summary>
    /// Callback when the channel name is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<ChannelInfo> OnChannelClicked { get; set; }

    protected string ThumbnailUrl => GetBestThumbnail();

    protected string FormattedDuration => FormatDuration(Video?.Duration ?? TimeSpan.Zero);

    protected string ChannelUrl => Video?.Channel?.ChannelUrl?.ToString() ?? "#";

    protected string CardStyle => Width.HasValue 
        ? $"width: {Width}px; cursor: pointer;" 
        : "cursor: pointer;";

    protected async Task OnChannelClick()
    {
        if (Video?.Channel is not null)
        {
            await OnChannelClicked.InvokeAsync(Video.Channel);
        }
    }

    private string GetBestThumbnail()
    {
        if (Video?.Thumbnails is null || Video.Thumbnails.Count == 0)
        {
            return "/images/placeholder-video.png";
        }

        // Prefer Medium or High quality for thumbnails
        var preferredQualities = new[] 
        { 
            ThumbnailQuality.Medium, 
            ThumbnailQuality.High, 
            ThumbnailQuality.Standard,
            ThumbnailQuality.Default
        };

        foreach (var quality in preferredQualities)
        {
            var thumbnail = Video.Thumbnails.FirstOrDefault(t => t.Quality == quality);
            
            if (thumbnail is not null)
            {
                return thumbnail.Url.ToString();
            }
        }

        // Fall back to the first available thumbnail
        return Video.Thumbnails.First().Url.ToString();
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
