

using System.ComponentModel;

namespace TMS.Libs.Frontend.Web.DockPanels.Enums;

/// <summary>
/// Dock panel theme.
/// </summary>
public enum DockPanelTheme
{
    /// <summary>
    /// dockview-theme-light theme.
    /// </summary>
    [Description("dockview-theme-light")]
    Light,

    /// <summary>
    /// dockview-theme-dark theme.
    /// </summary>
    [Description("dockview-theme-dark")]
    Dark,

    /// <summary>
    /// dockview-theme-vs theme.
    /// </summary>
    [Description("dockview-theme-vs")]
    VS,

    /// <summary>
    /// dockview-theme-abyss theme.
    /// </summary>
    [Description("dockview-theme-abyss")]
    Abyss,

    /// <summary>
    /// dockview-theme-dracula theme.
    /// </summary>
    [Description("dockview-theme-dracula")]
    Dracula,

    /// <summary>
    /// dockview-theme-replit theme.
    /// </summary>
    [Description("dockview-theme-replit")]
    Replit
}
