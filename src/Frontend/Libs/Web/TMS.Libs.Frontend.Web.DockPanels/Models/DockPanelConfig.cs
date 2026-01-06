

using System.Text.Json.Serialization;
using TMS.Libs.Frontend.Web.DockPanels.Components;
using TMS.Libs.Frontend.Web.DockPanels.Converters;
using TMS.Libs.Frontend.Web.DockPanels.Enums;

namespace TMS.Libs.Frontend.Web.DockPanels.Models;

internal sealed class DockPanelConfig
{
    /// <summary>
    /// 获得/设置 是否启用本地布局保持 默认 true
    /// </summary>
    public bool EnableLocalStorage { get; set; } = true;

    /// <summary>
    /// 获得/设置 是否锁定 默认 false
    /// </summary>
    /// <remarks>锁定后无法拖动</remarks>
    [JsonPropertyName("lock")]
    public bool IsLock { get; set; }

    /// <summary>
    /// 获得/设置 是否显示锁定按钮 默认 true 显示
    /// </summary>
    public bool ShowLock { get; set; }

    /// <summary>
    /// 获得/设置 是否悬浮 默认 false
    /// </summary>
    /// <remarks>锁定后无法拖动</remarks>
    public bool IsFloating { get; set; }

    /// <summary>
    /// 获得/设置 是否显示可悬浮按钮 默认 true
    /// </summary>
    public bool ShowFloat { get; set; } = true;

    /// <summary>
    /// 获得/设置 是否显示关闭按钮 默认 true 显示
    /// </summary>
    public bool ShowClose { get; set; }

    /// <summary>
    /// 获得/设置 是否显示显示图钉按钮 默认 true
    /// </summary>
    public bool ShowPin { get; set; } = true;

    /// <summary>
    /// 获得/设置 是否显示最大化按钮 默认 true
    /// </summary>
    public bool ShowMaximize { get; set; } = true;

    /// <summary>
    /// 获得/设置 客户端渲染模式 默认 null 客户端默认使用 always onlyWhenVisible 值
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DockPanelRenderMode Renderer { get; set; }

    /// <summary>
    /// 获得/设置 标签页可见状态改变事件回调
    /// </summary>
    public string? PanelVisibleChangedCallback { get; set; }

    /// <summary>
    /// Gets or sets the callback for panel active state changes.
    /// </summary>
    public string? PanelActiveChangedCallback { get; set; }

    /// <summary>
    /// 获得/设置 组件初始化完成事件回调
    /// </summary>
    public string? InitializedCallback { get; set; }

    /// <summary>
    /// 获得/设置 锁定事件回调
    /// </summary>
    public string? LockChangedCallback { get; set; }

    /// <summary>
    /// 获得/设置 分割栏调整事件回调
    /// </summary>
    public string? SplitterCallback { get; set; }

    /// <summary>
    /// 获得/设置 客户端缓存键值
    /// </summary>
    public string? LocalStorageKey { get; set; }

    /// <summary>
    /// 获得/设置 Golden-Layout 配置项集合 默认 空集合
    /// </summary>
    [JsonPropertyName("content")]
    [JsonConverter(typeof(DockPanelComponentConverter))]
    public List<DockPanelComponentBase> Contents { get; set; } = [];

    /// <summary>
    /// 获得/设置 组件主题 默认 null 未设置
    /// </summary>
    public string? Theme { get; set; }

    /// <summary>
    /// 获得/设置 布局配置 默认 null 未设置
    /// </summary>
    public string? LayoutConfig { get; set; }
}
