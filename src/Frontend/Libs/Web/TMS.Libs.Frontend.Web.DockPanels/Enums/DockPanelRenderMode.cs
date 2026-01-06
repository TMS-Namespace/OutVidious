

using BootstrapBlazor.Components;

namespace TMS.Libs.Frontend.Web.DockPanels.Enums;

/// <summary>
/// Dock panel render mode.
/// </summary>
[JsonEnumConverter(true)]
public enum DockPanelRenderMode
{
    /// <summary>
    /// 可见时渲染
    /// </summary>
    OnlyWhenVisible,

    /// <summary>
    /// 始终渲染
    /// </summary>
    Always
}
