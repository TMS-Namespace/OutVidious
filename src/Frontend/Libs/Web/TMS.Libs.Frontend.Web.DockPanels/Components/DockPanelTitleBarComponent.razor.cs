

using Microsoft.AspNetCore.Components;

namespace TMS.Libs.Frontend.Web.DockPanels.Components;

/// <summary>
/// Dock panel title bar component.
/// </summary>
public partial class DockPanelTitleBarComponent
{
    /// <summary>
    /// Gets or sets the title bar click callback.
    /// </summary>
    [Parameter]
    public Func<Task>? OnClickBarCallback { get; set; }

    /// <summary>
    /// Gets or sets the title bar icon name.
    /// </summary>
    [Parameter]
    public string? BarIcon { get; set; }

    /// <summary>
    /// Gets or sets the title bar icon URL.
    /// </summary>
    [Parameter]
    public string? BarIconUrl { get; set; }

    private async Task OnClickBar()
    {
        if (OnClickBarCallback != null)
        {
            await OnClickBarCallback();
        }
    }
}
