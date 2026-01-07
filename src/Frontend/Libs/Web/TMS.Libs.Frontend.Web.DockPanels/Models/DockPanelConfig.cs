

using System.Text.Json.Serialization;
using TMS.Libs.Frontend.Web.DockPanels.Components;
using TMS.Libs.Frontend.Web.DockPanels.Converters;
using TMS.Libs.Frontend.Web.DockPanels.Enums;

namespace TMS.Libs.Frontend.Web.DockPanels.Models;

internal sealed class DockPanelConfig
{
    /// <summary>
    /// Gets or sets whether local layout persistence is enabled. Default is true.
    /// </summary>
    public bool EnableLocalStorage { get; set; } = true;

    /// <summary>
    /// Gets or sets whether panels are locked. Default is false.
    /// </summary>
    /// <remarks>Locked panels cannot be dragged.</remarks>
    [JsonPropertyName("lock")]
    public bool IsLock { get; set; }

    /// <summary>
    /// Gets or sets whether to show the lock button. Default is true.
    /// </summary>
    public bool ShowLock { get; set; }

    /// <summary>
    /// Gets or sets whether panels are floating. Default is false.
    /// </summary>
    /// <remarks>Locked panels cannot be dragged.</remarks>
    public bool IsFloating { get; set; }

    /// <summary>
    /// Gets or sets whether to show the float button. Default is true.
    /// </summary>
    public bool ShowFloat { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show the close button. Default is true.
    /// </summary>
    public bool ShowClose { get; set; }

    /// <summary>
    /// Gets or sets whether to show the pin button. Default is true.
    /// </summary>
    public bool ShowPin { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show the maximize button. Default is true.
    /// </summary>
    public bool ShowMaximize { get; set; } = true;

    /// <summary>
    /// Gets or sets the client renderer mode. Default is <see cref="DockPanelRenderMode.OnlyWhenVisible"/>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DockPanelRenderMode Renderer { get; set; }

    /// <summary>
    /// Gets or sets the callback for panel visibility changes.
    /// </summary>
    public string? PanelVisibleChangedCallback { get; set; }

    /// <summary>
    /// Gets or sets the callback for panel active state changes.
    /// </summary>
    public string? PanelActiveChangedCallback { get; set; }

    /// <summary>
    /// Gets or sets the callback for panel registration.
    /// </summary>
    public string? PanelAddedCallback { get; set; }

    /// <summary>
    /// Gets or sets the callback when a drawer group is ready.
    /// </summary>
    public string? DrawerReadyCallback { get; set; }

    /// <summary>
    /// Gets or sets the callback when initialization completes.
    /// </summary>
    public string? InitializedCallback { get; set; }

    /// <summary>
    /// Gets or sets the callback when lock state changes.
    /// </summary>
    public string? LockChangedCallback { get; set; }

    /// <summary>
    /// Gets or sets the callback when the splitter is resized.
    /// </summary>
    public string? SplitterCallback { get; set; }

    /// <summary>
    /// Gets or sets the local storage key for layout persistence.
    /// </summary>
    public string? LocalStorageKey { get; set; }

    /// <summary>
    /// Gets or sets the layout content configuration.
    /// </summary>
    [JsonPropertyName("content")]
    [JsonConverter(typeof(DockPanelComponentConverter))]
    public List<DockPanelComponentBase> Contents { get; set; } = [];

    /// <summary>
    /// Gets or sets the component theme.
    /// </summary>
    public string? Theme { get; set; }

    /// <summary>
    /// Gets or sets the sidebar width in pixels.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("asideWidthPx")]
    public int? AsideWidthPx { get; set; }

    /// <summary>
    /// Gets or sets the sidebar button inline padding in pixels.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("asideButtonPaddingInlinePx")]
    public int? AsideButtonPaddingInlinePx { get; set; }

    /// <summary>
    /// Gets or sets the extra right-side inline padding for sidebar buttons in pixels.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("asideButtonPaddingInlineEndExtraPx")]
    public int? AsideButtonPaddingInlineEndExtraPx { get; set; }

    /// <summary>
    /// Gets or sets the sidebar button block padding in pixels.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("asideButtonPaddingBlockPx")]
    public int? AsideButtonPaddingBlockPx { get; set; }

    /// <summary>
    /// Gets or sets the sidebar button gap in pixels.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("asideButtonGapPx")]
    public int? AsideButtonGapPx { get; set; }

    /// <summary>
    /// Gets or sets the gap between dock action buttons in pixels.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("actionButtonsGapPx")]
    public int? ActionButtonsGapPx { get; set; }

    /// <summary>
    /// Gets or sets the layout configuration JSON.
    /// </summary>
    public string? LayoutConfig { get; set; }
}
