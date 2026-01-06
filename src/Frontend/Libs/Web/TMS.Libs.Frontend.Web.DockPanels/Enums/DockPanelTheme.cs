

using System.ComponentModel;

namespace TMS.Libs.Frontend.Web.DockPanels.Enums;

/// <summary>
/// Dock panel theme.
/// </summary>
public enum DockPanelTheme
{
    /// <summary>
    /// dockview-theme-light 主题
    /// </summary>
    [Description("dockview-theme-light")]
    Light,

    /// <summary>
    /// dockview-theme-dark 主题
    /// </summary>
    [Description("dockview-theme-dark")]
    Dark,

    /// <summary>
    /// dockview-theme-vs 主题
    /// </summary>
    [Description("dockview-theme-vs")]
    VS,

    /// <summary>
    /// dockview-theme-abyss 主题
    /// </summary>
    [Description("dockview-theme-abyss")]
    Abyss,

    /// <summary>
    /// dockview-theme-dracula 主题
    /// </summary>
    [Description("dockview-theme-dracula")]
    Dracula,

    /// <summary>
    /// dockview-theme-replit 主题
    /// </summary>
    [Description("dockview-theme-replit")]
    Replit
}
