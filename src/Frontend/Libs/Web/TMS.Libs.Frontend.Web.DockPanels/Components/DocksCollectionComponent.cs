

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Text.Json.Serialization;
using TMS.Libs.Frontend.Web.DockPanels.Converters;

namespace TMS.Libs.Frontend.Web.DockPanels.Components;

/// <summary>
/// Dock panel content component for layout configuration.
/// </summary>
public class DocksCollectionComponent : DockPanelComponentBase
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
