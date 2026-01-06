

using System.Diagnostics.CodeAnalysis;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace TMS.Libs.Frontend.Web.DockPanels.Components;

/// <summary>
/// Dock panel icon component.
/// </summary>
public partial class DockPanelIconComponent
{
    private const string IconSpritePath = "./_content/TMS.Libs.Frontend.Web.DockPanels/icon/dockview.svg";

    /// <summary>
    /// 获得/设置 资源文件接口实例
    /// </summary>
    [Inject, NotNull]
    protected IStringLocalizer<DockPanelIconComponent>? Localizer { get; set; }

    /// <summary>
    /// 获得/设置 图标名称
    /// </summary>
    [Parameter, NotNull]
    [EditorRequired]
    public string? IconName { get; set; }

    /// <summary>
    /// 获得 样式字符串
    /// </summary>
    private string? ClassString => CssBuilder.Default("bb-dockview-control-icon")
        .AddClass($"bb-dockview-control-icon-{IconName}")
        .Build();

    /// <summary>
    /// 获得 图标地址
    /// </summary>
    protected string Href => $"{IconSpritePath}#{IconName}";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        IconName ??= "close";
    }
}
