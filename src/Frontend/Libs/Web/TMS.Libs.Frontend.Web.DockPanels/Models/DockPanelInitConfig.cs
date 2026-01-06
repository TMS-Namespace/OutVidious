using TMS.Libs.Frontend.Web.DockPanels.Enums;

namespace TMS.Libs.Frontend.Web.DockPanels.Models;

/// <summary>
/// Configuration for an individual panel's initial state.
/// </summary>
public sealed class DockPanelInitConfig
{
    /// <summary>
    /// Gets or sets the unique key (identifier) of the panel.
    /// This must match the Key property set on the DockPanelComponent.
    /// Using a stable key instead of title makes the configuration robust to title changes.
    /// </summary>
    public required string Key { get; init; }

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
