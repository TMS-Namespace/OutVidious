using Microsoft.AspNetCore.Components;
using TMS.Apps.FrontTube.Backend.Core.ViewModels;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Components;

/// <summary>
/// Component for displaying a grid of video thumbnails.
/// </summary>
public partial class VideosGridComponentBase : ComponentBase
{
    /// <summary>
    /// The list of videos to display.
    /// </summary>
    [Parameter]
    public IReadOnlyList<Video>? Videos { get; set; }

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
    public EventCallback<Video> OnVideoClick { get; set; }

    /// <summary>
    /// Callback when a channel name is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnChannelClick { get; set; }

    /// <summary>
    /// Callback when the load more button is clicked.
    /// </summary>
    [Parameter]
    public EventCallback OnLoadMore { get; set; }

    protected async Task HandleVideoClick(Video video)
    {
        await OnVideoClick.InvokeAsync(video);
    }

    protected async Task HandleChannelClick(string channelRemoteId)
    {
        await OnChannelClick.InvokeAsync(channelRemoteId);
    }

    protected async Task HandleLoadMore()
    {
        await OnLoadMore.InvokeAsync();
    }
}
