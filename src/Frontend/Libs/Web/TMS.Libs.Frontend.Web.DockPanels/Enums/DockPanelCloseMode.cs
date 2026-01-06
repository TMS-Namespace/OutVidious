namespace TMS.Libs.Frontend.Web.DockPanels.Enums;

/// <summary>
/// Defines how a dock panel behaves when the close action is invoked.
/// </summary>
public enum DockPanelCloseMode
{
    /// <summary>
    /// Use the default DockView close behavior (remove the panel).
    /// </summary>
    Default = 0,

    /// <summary>
    /// Hide the drawer tab instead of removing the panel.
    /// </summary>
    HideDrawerTab = 1
}
