namespace TMS.Libs.Frontend.Web.DockViewWrapper.Enums;

/// <summary>
/// Represents the location type of a dock panel.
/// </summary>
public enum DockPanelLocationType
{
    /// <summary>
    /// The panel is in the main grid layout.
    /// </summary>
    Grid,

    /// <summary>
    /// The panel is floating as a separate window.
    /// </summary>
    Floating,

    /// <summary>
    /// The panel is popped out to a separate browser window.
    /// </summary>
    Popout,

    /// <summary>
    /// The panel location type is unknown.
    /// </summary>
    Unknown
}
