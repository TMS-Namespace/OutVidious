using TMS.Libs.Frontend.Web.DockPanels.Enums;

namespace TMS.Libs.Frontend.Web.DockPanels.Models;

/// <summary>
/// Represents the current state of a dock panel.
/// </summary>
public sealed record DockPanelState
{
    /// <summary>
    /// Gets the location type of the panel.
    /// </summary>
    public required DockPanelLocationType LocationType { get; init; }

    /// <summary>
    /// Gets whether the panel is in drawer (unpinned) mode.
    /// </summary>
    public bool IsDrawer { get; init; }

    /// <summary>
    /// Gets whether the drawer is currently visible (expanded/slid in).
    /// </summary>
    public bool IsDrawerVisible { get; init; }

    /// <summary>
    /// Gets whether the panel is collapsed (shows only the header).
    /// </summary>
    public bool IsCollapsed { get; init; }

    /// <summary>
    /// Gets whether the panel is maximized.
    /// </summary>
    public bool IsMaximized { get; init; }

    /// <summary>
    /// Gets whether the panel is locked.
    /// </summary>
    public bool IsLocked { get; init; }

    /// <summary>
    /// Gets whether the panel is visible.
    /// </summary>
    public bool IsVisible { get; init; }
}
