using Microsoft.AspNetCore.Components;
using TMS.Apps.FrontTube.Backend.Core.ViewModels;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Components;

/// <summary>
/// Component that contains and displays the video player.
/// </summary>
public partial class VideoContainerComponent : ComponentBase
{
    /// <summary>
    /// Gets or sets the video view model.
    /// </summary>
    [Parameter]
    public Video? ViewModel { get; set; }

    /// <summary>
    /// Gets or sets the provider base URL.
    /// </summary>
    [Parameter]
    public string ProviderBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the video is currently loading.
    /// </summary>
    [Parameter]
    public bool IsLoading { get; set; }
}
