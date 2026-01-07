using TMS.Libs.Frontend.Web.DockPanels.Components;
using TMS.Libs.Frontend.Web.DockPanels.Enums;

namespace TMS.Libs.Frontend.Web.DockPanels.Models;

/// <summary>
/// Configuration for a dock panel's initial state after initialization.
/// Used by <see cref="Components.DocksHostComponent"/> to configure behavior.
/// </summary>
public sealed record DockPanelConfiguration
{
    /// <summary>
    /// Gets the panel this configuration applies to.
    /// </summary>
    public required DockPanelComponent Panel { get; init; }

    /// <summary>
    /// Gets the internal GUID of the panel.
    /// </summary>
    public Guid PanelId => Panel.PanelId;

    /// <summary>
    /// Gets the initial pin state of the panel.
    /// </summary>
    public DocksCollectionPinState PinState { get; init; } = DocksCollectionPinState.Pinned;

    /// <summary>
    /// Gets the drawer tab visibility when the panel is a drawer.
    /// Only applicable when <see cref="PinState"/> is <see cref="DocksCollectionPinState.Drawer"/>.
    /// </summary>
    public DrawerTabVisibility DrawerTabVisibility { get; init; } = DrawerTabVisibility.Visible;

    /// <summary>
    /// Gets the default width in pixels for the drawer when expanded.
    /// If null, the default CSS width is used.
    /// </summary>
    public int? DrawerWidthPx { get; init; }

}
