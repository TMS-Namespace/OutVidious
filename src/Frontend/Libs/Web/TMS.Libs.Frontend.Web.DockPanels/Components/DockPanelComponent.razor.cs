

using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;
using TMS.Libs.Frontend.Web.DockPanels.Converters;
using TMS.Libs.Frontend.Web.DockPanels.Enums;
using TMS.Libs.Frontend.Web.DockPanels.Models;

namespace TMS.Libs.Frontend.Web.DockPanels.Components;

/// <summary>
/// Dock panel component configuration entry.
/// </summary>
public partial class DockPanelComponent
{
    /// <summary>
    /// Gets or sets whether the panel header is visible.
    /// </summary>
    [Parameter]
    public bool ShowHeader { get; set; } = true;

    /// <summary>
    /// Gets or sets the panel title.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the title width in pixels.
    /// </summary>
    [Parameter]
    public int? TitleWidth { get; set; }

    /// <summary>
    /// Gets or sets the title CSS class.
    /// </summary>
    [Parameter]
    public string? TitleClass { get; set; }

    /// <summary>
    /// Gets or sets the title template.
    /// </summary>
    [Parameter]
    [JsonIgnore]
    public RenderFragment? TitleTemplate { get; set; }

    /// <summary>
    /// Gets or sets the panel CSS class.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets whether the panel is visible.
    /// </summary>
    [Parameter]
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to defer rendering the content until the panel is activated.
    /// </summary>
    [Parameter]
    [JsonIgnore]
    public bool DeferContentUntilActive { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the panel should be active when created.
    /// </summary>
    [Parameter]
    [JsonPropertyName("isActive")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets whether the panel can be closed.
    /// </summary>
    [Parameter]
    public bool? ShowClose { get; set; }

    /// <summary>
    /// Gets the internal GUID for this panel.
    /// This value is generated internally and cannot be set by consumers.
    /// </summary>
    [JsonIgnore]
    public Guid PanelId => ComponentId;

    /// <summary>
    /// Gets the internal group GUID for this panel when it belongs to a group.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("groupId")]
    public Guid? GroupId { get; private set; }

    /// <summary>
    /// Gets or sets the float type for the panel.
    /// </summary>
    [Parameter]
    [JsonConverter(typeof(DockPanelFloatTypeJsonConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DockPanelFloatType FloatType { get; set; }

    /// <summary>
    /// Gets or sets the drawer direction.
    /// </summary>
    [Parameter]
    [JsonConverter(typeof(DockPanelDrawerDirectionJsonConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DockPanelDrawerDirection Direction { get; set; }

    /// <summary>
    /// Gets or sets the drawer options.
    /// </summary>
    [Parameter]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DockPanelDrawerOptions? Drawer { get; set; }

    /// <summary>
    /// Gets or sets whether the panel is locked.
    /// </summary>
    [Parameter]
    public bool? IsLock { get; set; }

    /// <summary>
    /// Gets or sets whether the lock button is visible.
    /// </summary>
    [Parameter]
    public bool? ShowLock { get; set; }

    /// <summary>
    /// Gets or sets whether the panel is floating.
    /// </summary>
    [Parameter]
    public bool? IsFloating { get; set; }

    /// <summary>
    /// Gets or sets whether the float button is visible.
    /// </summary>
    [Parameter]
    public bool? ShowFloat { get; set; }

    /// <summary>
    /// Gets or sets whether the maximize button is visible.
    /// </summary>
    [Parameter]
    public bool? ShowMaximize { get; set; }

    /// <summary>
    /// Gets or sets how the panel should behave when closed.
    /// </summary>
    [Parameter]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DockPanelCloseMode CloseMode { get; set; }

    /// <summary>
    /// Gets or sets a static group title for drawer sidebar buttons.
    /// </summary>
    [Parameter]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StaticGroupTitle { get; set; }

    /// <summary>
    /// Gets or sets a static group icon for drawer sidebar buttons.
    /// </summary>
    [Parameter]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("staticGroupIcon")]
    public string? StaticGroupIcon { get; set; }

    /// <summary>
    /// Gets or sets the renderer name.
    /// </summary>
    [Parameter]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Renderer { get; set; }

    /// <summary>
    /// Gets or sets whether the title bar is visible.
    /// </summary>
    [Parameter]
    [JsonIgnore]
    public bool ShowTitleBar { get; set; }

    /// <summary>
    /// Gets or sets the title bar icon name.
    /// </summary>
    [Parameter]
    [JsonIgnore]
    public string? TitleBarIcon { get; set; }

    /// <summary>
    /// Gets or sets the title bar icon URL.
    /// </summary>
    [Parameter]
    [JsonIgnore]
    public string? TitleBarIconUrl { get; set; }

    /// <summary>
    /// Gets or sets the title bar click callback.
    /// </summary>
    [Parameter]
    [JsonIgnore]
    public Func<Task>? OnClickTitleBarCallback { get; set; }

    [CascadingParameter]
    private Guid? ParentGroupId { get; set; }

    [CascadingParameter(Name = "DockGroupIcon")]
    private string? ParentGroupIcon { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        Type = DockCollectionType.Component;
        _isActive = IsActive;
        _hasActivated = IsActive;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        GroupId = ParentGroupId;
        if (string.IsNullOrWhiteSpace(StaticGroupIcon))
        {
            StaticGroupIcon = ParentGroupIcon;
        }
    }

    internal Task SetActiveStateAsync(bool isActive)
    {
        _isActive = isActive;
        if (isActive)
        {
            _hasActivated = true;
        }

        return InvokeAsync(StateHasChanged);
    }

    private async Task OnClickBar()
    {
        if (OnClickTitleBarCallback != null)
        {
            await OnClickTitleBarCallback();
        }
    }

    /// <summary>
    /// Sets the <see cref="Visible"/> property.
    /// </summary>
    /// <param name="visible"></param>
    public void SetVisible(bool visible)
    {
        Visible = visible;
    }

    private bool ShouldRenderContent => !DeferContentUntilActive || _hasActivated || _isActive;

    private bool _isActive;
    private bool _hasActivated;
}
