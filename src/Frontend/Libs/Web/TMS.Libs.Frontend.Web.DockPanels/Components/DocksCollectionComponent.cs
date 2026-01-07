

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Text.Json.Serialization;
using TMS.Libs.Frontend.Web.DockPanels.Converters;
using TMS.Libs.Frontend.Web.DockPanels.Enums;

namespace TMS.Libs.Frontend.Web.DockPanels.Components;

/// <summary>
/// Dock panel content component for layout configuration.
/// </summary>
public class DocksCollectionComponent : DockPanelComponentBase
{
    /// <summary>
    /// Gets or sets the static group icon for drawer sidebar buttons.
    /// </summary>
    [Parameter]
    [JsonIgnore]
    public string? StaticGroupIcon { get; set; }

    /// <summary>
    /// Gets the internal GUID for this collection.
    /// </summary>
    [JsonIgnore]
    public Guid CollectionId => ComponentId;

    /// <summary>
    /// Gets the internal GUID for this group when <see cref="DockPanelComponentBase.Type"/> is <see cref="DockCollectionType.Group"/>.
    /// </summary>
    [JsonIgnore]
    public Guid? GroupId => Type == DockCollectionType.Group ? ComponentId : null;

    /// <summary>
    /// Gets or sets the child items.
    /// </summary>
    [JsonConverter(typeof(DockPanelComponentConverter))]
    [JsonPropertyName("content")]
    public List<DockPanelComponentBase> Items { get; set; } = [];

    [CascadingParameter]
    private Guid? ParentGroupId { get; set; }

    [CascadingParameter(Name = "DockGroupIcon")]
    private string? ParentGroupIcon { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="builder"></param>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var groupId = GroupId ?? ParentGroupId;
        var groupIcon = string.IsNullOrWhiteSpace(StaticGroupIcon)
            ? ParentGroupIcon
            : StaticGroupIcon;

        builder.OpenComponent<CascadingValue<Guid?>>(0);
        builder.AddAttribute(1, nameof(CascadingValue<Guid?>.Value), groupId);
        builder.AddAttribute(2, nameof(CascadingValue<Guid?>.IsFixed), true);
        builder.AddAttribute(3, nameof(CascadingValue<Guid?>.ChildContent), (RenderFragment)(childBuilder =>
        {
            childBuilder.OpenComponent<CascadingValue<string?>>(0);
            childBuilder.AddAttribute(1, nameof(CascadingValue<string?>.Name), "DockGroupIcon");
            childBuilder.AddAttribute(2, nameof(CascadingValue<string?>.Value), groupIcon);
            childBuilder.AddAttribute(3, nameof(CascadingValue<string?>.IsFixed), true);
            childBuilder.AddAttribute(4, nameof(CascadingValue<string?>.ChildContent), (RenderFragment)(iconBuilder =>
            {
                iconBuilder.OpenComponent<CascadingValue<List<DockPanelComponentBase>>>(0);
                iconBuilder.AddAttribute(1, nameof(CascadingValue<List<DockPanelComponentBase>>.Value), Items);
                iconBuilder.AddAttribute(2, nameof(CascadingValue<List<DockPanelComponentBase>>.IsFixed), true);
                iconBuilder.AddAttribute(3, nameof(CascadingValue<List<DockPanelComponentBase>>.ChildContent), ChildContent);
                iconBuilder.CloseComponent();
            }));
            childBuilder.CloseComponent();
        }));
        builder.CloseComponent();
    }
}
