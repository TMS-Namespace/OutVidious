

using BootstrapBlazor.Components;

namespace TMS.Libs.Frontend.Web.DockPanels.Enums;

/// <summary>
/// Dock panel content layout type.
/// </summary>
[JsonEnumConverter(true)]
public enum DockPanelContentType
{
    /// <summary>
    /// 行排列 水平排列
    /// </summary>
    Row,

    /// <summary>
    /// 列排列 垂直排列
    /// </summary>
    Column,

    /// <summary>
    /// 标签排列
    /// </summary>
    Group,

    /// <summary>
    /// 组件
    /// </summary>
    Component,
}
