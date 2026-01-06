// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://www.blazor.zone or https://argozhang.github.io/

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
