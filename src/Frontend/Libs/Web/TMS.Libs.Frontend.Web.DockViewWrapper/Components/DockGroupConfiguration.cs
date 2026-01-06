using TMS.Libs.Frontend.Web.DockViewWrapper.Enums;

namespace TMS.Libs.Frontend.Web.DockViewWrapper.Components;

/// <summary>
/// Configuration for a dock panel group's initial state.
/// </summary>
public sealed class DockGroupConfiguration
{
    /// <summary>
    /// Gets or sets the 0-based index of the group.
    /// </summary>
    public required int GroupIndex { get; init; }

    /// <summary>
    /// Gets or sets the initial pin state of the group.
    /// </summary>
    public DockPanelPinState PinState { get; init; } = DockPanelPinState.Pinned;

    /// <summary>
    /// Gets or sets a static title for the group's sidebar button when in drawer mode.
    /// If set, the sidebar button will always display this title instead of the active panel's title.
    /// If null, the default DockView behavior is used (shows active panel's title).
    /// </summary>
    public string? GroupTitle { get; init; }

    /// <summary>
    /// Gets or sets the panels in this group that need additional configuration.
    /// </summary>
    public List<DockPanelInitConfig> Panels { get; init; } = [];
}

/// <summary>
/// Configuration for an individual panel's initial state.
/// </summary>
public sealed class DockPanelInitConfig
{
    /// <summary>
    /// Gets or sets the title of the panel.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets or sets the drawer tab visibility when the panel is a drawer.
    /// </summary>
    public DrawerTabVisibility DrawerTabVisibility { get; init; } = DrawerTabVisibility.Visible;

    /// <summary>
    /// Gets or sets the default width in pixels for the drawer when expanded.
    /// If null, the default CSS width is used.
    /// </summary>
    public int? DrawerWidthPx { get; init; }
}
