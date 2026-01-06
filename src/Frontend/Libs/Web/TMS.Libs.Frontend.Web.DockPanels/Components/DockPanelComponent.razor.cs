

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
    /// 获得/设置 组件是否显示 Header 默认 true 显示
    /// </summary>
    [Parameter]
    public bool ShowHeader { get; set; } = true;

    /// <summary>
    /// 获得/设置 组件 Title
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// 获得/设置 组件 Title 宽度 默认 null 未设置
    /// </summary>
    [Parameter]
    public int? TitleWidth { get; set; }

    /// <summary>
    /// 获得/设置 组件 Title 样式 默认 null 未设置
    /// </summary>
    [Parameter]
    public string? TitleClass { get; set; }

    /// <summary>
    /// 获得/设置 Title 模板 默认 null 未设置
    /// </summary>
    [Parameter]
    [JsonIgnore]
    public RenderFragment? TitleTemplate { get; set; }

    /// <summary>
    /// 获得/设置 组件 Class 默认 null 未设置
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// 获得/设置 组件是否可见 默认 true 可见
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
    /// 获得/设置 组件是否允许关闭 默认 null 使用 DockView 的配置
    /// </summary>
    [Parameter]
    public bool? ShowClose { get; set; }

    /// <summary>
    /// 获得/设置 组件唯一标识值 默认 null 未设置时取 Title 作为唯一标识
    /// </summary>
    [Parameter]
    public string? Key { get; set; }

    /// <summary>
    /// Gets or sets the group ID used to keep related panels together.
    /// </summary>
    [Parameter]
    public string? GroupId { get; set; }

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
    /// 获得/设置 是否锁定 默认 null 未设置时取 DockView 的配置
    /// </summary>
    /// <remarks>锁定后无法拖动</remarks>
    [Parameter]
    public bool? IsLock { get; set; }

    /// <summary>
    /// 获得/设置 是否显示锁定按钮 默认 null 未设置时取 DockView 的配置
    /// </summary>
    [Parameter]
    public bool? ShowLock { get; set; }

    /// <summary>
    /// 获得/设置 是否悬浮 默认 null 未设置时取 DockView 的配置
    /// </summary>
    [Parameter]
    public bool? IsFloating { get; set; }

    /// <summary>
    /// 获得/设置 是否显示可悬浮按钮 默认 null 未设置时取 DockView 的配置
    /// </summary>
    [Parameter]
    public bool? ShowFloat { get; set; }

    /// <summary>
    /// 获得/设置 是否显示最大化按钮 默认 null 未设置时取 DockView 的配置
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
    /// 获得/设置 是否一直显示 默认 null 未设置时取 DockView 的配置
    /// </summary>
    [Parameter]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Renderer { get; set; }

    /// <summary>
    /// 获得/设置 是否显示标题前置图标 默认 false 不显示
    /// </summary>
    [Parameter]
    [JsonIgnore]
    public bool ShowTitleBar { get; set; }

    /// <summary>
    /// 获得/设置 标题前置图标 默认 null 未设置使用默认图标
    /// </summary>
    [Parameter]
    [JsonIgnore]
    public string? TitleBarIcon { get; set; }

    /// <summary>
    /// 获得/设置 标题前置图标 Url 默认 null 未设置使用默认图标
    /// </summary>
    [Parameter]
    [JsonIgnore]
    public string? TitleBarIconUrl { get; set; }

    /// <summary>
    /// 获得/设置 标题前置图标点击回调方法 默认 null
    /// </summary>
    [Parameter]
    [JsonIgnore]
    public Func<Task>? OnClickTitleBarCallback { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        Type = DockPanelContentType.Component;
        _isActive = IsActive;
        _hasActivated = IsActive;
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
    /// 设置 Visible 参数方法
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
