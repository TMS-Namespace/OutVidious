using TMS.Libs.Frontend.Web.DockPanels.Components;
using TMS.Libs.Frontend.Web.DockPanels.Enums;

namespace TMS.Libs.Frontend.Web.DockPanels.Models;

/// <summary>
/// Configuration for an individual panel's initial state.
/// </summary>
public sealed class DockPanelInitConfig
{
    /// <summary>
    /// Gets or sets the panel instance this configuration applies to.
    /// </summary>
    public required DockPanelComponent Panel { get; init; }

    /// <summary>
    /// Gets the internal GUID of the panel.
    /// </summary>
    public Guid PanelId => Panel.PanelId;

    /// <summary>
    /// Gets or sets the drawer tab visibility when the panel is a drawer.
    /// </summary>
    public DrawerTabVisibility DrawerTabVisibility { get; init; } = DrawerTabVisibility.Visible;

    /// <summary>
    /// Gets or sets the default width in pixels for the drawer when expanded.
    /// If null, the default CSS width is used.
    /// </summary>
    public int? DrawerWidthPx { get; init; }
}
