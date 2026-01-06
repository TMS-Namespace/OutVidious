namespace TMS.Libs.Frontend.Web.DockViewWrapper.Enums;

/// <summary>
/// Represents the initial pin state of a dock panel when the layout initializes.
/// </summary>
public enum DockPanelPinState
{
    /// <summary>
    /// The panel starts pinned to the grid (default dock behavior).
    /// </summary>
    Pinned,

    /// <summary>
    /// The panel starts unpinned as a collapsible drawer that slides in/out from the side.
    /// </summary>
    Drawer
}
