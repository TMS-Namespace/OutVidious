using Microsoft.AspNetCore.Components;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Contracts;

namespace TMS.Apps.Web.OutVidious.WebGUI.Components.Shared;

/// <summary>
/// Component for displaying a grid of video thumbnails.
/// </summary>
public partial class VideosGridComponentBase : ComponentBase
{
    /// <summary>
    /// The list of videos to display.
    /// </summary>
    [Parameter]
    public IReadOnlyList<VideoSummary>? Videos { get; set; }

    /// <summary>
    /// Whether to show the channel name on each thumbnail.
    /// </summary>
    [Parameter]
    public bool ShowChannelName { get; set; } = true;

    /// <summary>
    /// Whether the initial content is still loading.
    /// </summary>
    [Parameter]
    public bool IsLoading { get; set; }

    /// <summary>
    /// Whether more content is being loaded (pagination).
    /// </summary>
    [Parameter]
    public bool IsLoadingMore { get; set; }

    /// <summary>
    /// Whether there are more videos to load.
    /// </summary>
    [Parameter]
    public bool HasMore { get; set; }

    /// <summary>
    /// Whether to show the load more button.
    /// </summary>
    [Parameter]
    public bool ShowLoadMore { get; set; } = true;

    /// <summary>
    /// Message to display when there are no videos.
    /// </summary>
    [Parameter]
    public string EmptyMessage { get; set; } = "No videos found.";

    /// <summary>
    /// Additional CSS class for the grid container.
    /// </summary>
    [Parameter]
    public string? GridClass { get; set; }

    /// <summary>
    /// Callback when a video is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<VideoSummary> OnVideoClick { get; set; }

    /// <summary>
    /// Callback when a channel name is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<ChannelInfo> OnChannelClick { get; set; }

    /// <summary>
    /// Callback when the load more button is clicked.
    /// </summary>
    [Parameter]
    public EventCallback OnLoadMore { get; set; }

    protected async Task HandleVideoClick(VideoSummary video)
    {
        await OnVideoClick.InvokeAsync(video);
    }

    protected async Task HandleChannelClick(ChannelInfo channel)
    {
        await OnChannelClick.InvokeAsync(channel);
    }

    protected async Task HandleLoadMore()
    {
        await OnLoadMore.InvokeAsync();
    }
}
