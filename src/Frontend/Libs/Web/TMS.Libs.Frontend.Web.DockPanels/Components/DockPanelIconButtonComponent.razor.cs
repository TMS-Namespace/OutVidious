

using BootstrapBlazor.Components;

namespace TMS.Libs.Frontend.Web.DockPanels.Components;

/// <summary>
/// Dock panel icon button component.
/// </summary>
public partial class DockPanelIconButtonComponent
{
    /// <summary>
    /// Gets the CSS class string.
    /// </summary>
    private string? ClassString => CssBuilder.Default("bb-dockview-control-icon bb-dockview-control-icon-button")
        .AddClass($"bb-dockview-control-icon-{IconName}")
        .Build();
}
