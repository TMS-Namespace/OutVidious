// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://www.blazor.zone or https://argozhang.github.io/

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Text.Json.Serialization;
using TMS.Libs.Frontend.Web.DockPanels.Converters;

namespace TMS.Libs.Frontend.Web.DockPanels.Components;

/// <summary>
/// Dock panel content component for layout configuration.
/// </summary>
public class DockPanelContentComponent : DockPanelComponentBase
{
    /// <summary>
    /// 获得/设置 子项集合
    /// </summary>
    [JsonConverter(typeof(DockPanelComponentConverter))]
    [JsonPropertyName("content")]
    public List<DockPanelComponentBase> Items { get; set; } = [];

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="builder"></param>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<List<DockPanelComponentBase>>>(0);
        builder.AddAttribute(1, nameof(CascadingValue<List<DockPanelComponentBase>>.Value), Items);
        builder.AddAttribute(2, nameof(CascadingValue<List<DockPanelComponentBase>>.IsFixed), true);
        builder.AddAttribute(3, nameof(CascadingValue<List<DockPanelComponentBase>>.ChildContent), ChildContent);
        builder.CloseComponent();
    }
}
