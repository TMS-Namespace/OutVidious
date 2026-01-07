

using BootstrapBlazor.Components;

namespace TMS.Libs.Frontend.Web.DockPanels.Enums;

/// <summary>
/// Dock panel render mode.
/// </summary>
[JsonEnumConverter(true)]
public enum DockPanelRenderMode
{
    /// <summary>
    /// Render only when visible.
    /// </summary>
    OnlyWhenVisible,

    /// <summary>
    /// Always render.
    /// </summary>
    Always
}
