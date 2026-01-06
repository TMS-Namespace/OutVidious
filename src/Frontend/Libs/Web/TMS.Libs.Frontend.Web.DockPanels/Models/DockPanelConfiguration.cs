using TMS.Libs.Frontend.Web.DockPanels.Enums;

namespace TMS.Libs.Frontend.Web.DockPanels.Models;

/// <summary>
/// Configuration for a dock panel's initial state after initialization.
/// Used by <see cref="Components.DocksCollectionDrawerComponent"/> to configure behavior.
/// </summary>
public sealed record DockPanelConfiguration
{
    /// <summary>
    /// Gets the title of the panel this configuration applies to.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the initial pin state of the panel.
    /// </summary>
    public DockPanelPinState PinState { get; init; } = DockPanelPinState.Pinned;

    /// <summary>
    /// Gets the drawer tab visibility when the panel is a drawer.
    /// Only applicable when <see cref="PinState"/> is <see cref="DockPanelPinState.Drawer"/>.
    /// </summary>
    public DrawerTabVisibility DrawerTabVisibility { get; init; } = DrawerTabVisibility.Visible;

    /// <summary>
    /// Gets the default width in pixels for the drawer when expanded.
    /// If null, the default CSS width is used.
    /// </summary>
    public int? DrawerWidthPx { get; init; }

    /// <summary>
    /// Gets the 0-based group index for unpinning order.
    /// When multiple panels are in the same group, this is the group index to use.
    /// </summary>
    public int? GroupIndex { get; init; }
}
