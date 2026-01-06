namespace TMS.Libs.Frontend.Web.DockPanels.Enums;

/// <summary>
/// Represents the initial visibility of a dock panel's drawer tab when the layout initializes.
/// Only applicable when <see cref="DockPanelPinState"/> is <see cref="DockPanelPinState.Drawer"/>.
/// </summary>
public enum DrawerTabVisibility
{
    /// <summary>
    /// The drawer tab is visible in the sidebar (default).
    /// </summary>
    Visible,

    /// <summary>
    /// The drawer tab is hidden and can be shown programmatically later.
    /// </summary>
    Hidden
}
