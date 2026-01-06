

using BootstrapBlazor.Components;

namespace TMS.Libs.Frontend.Web.DockPanels.Components;

/// <summary>
/// Dock panel dropdown icon component.
/// </summary>
public partial class DockPanelDropdownIconComponent
{
    /// <summary>
    /// 获得 样式字符串
    /// </summary>
    private string? ClassString => CssBuilder.Default("dropdown dropdown-center bb-dockview-control-icon")
        .AddClass($"bb-dockview-control-icon-{IconName}")
        .Build();
}
