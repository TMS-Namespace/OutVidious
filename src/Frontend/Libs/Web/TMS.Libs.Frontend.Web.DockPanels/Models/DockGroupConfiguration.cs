using TMS.Libs.Frontend.Web.DockPanels.Components;
using TMS.Libs.Frontend.Web.DockPanels.Enums;

namespace TMS.Libs.Frontend.Web.DockPanels.Models;

/// <summary>
/// Configuration for a dock panel group's initial state.
/// </summary>
public sealed class DockGroupConfiguration
{
    /// <summary>
    /// Gets or sets the anchor panel used to resolve the target group.
    /// </summary>
    public required DockPanelComponent GroupPanel { get; init; }

    /// <summary>
    /// Gets the internal GUID of the anchor panel.
    /// </summary>
    public Guid GroupPanelId => GroupPanel.PanelId;

    /// <summary>
    /// Gets or sets the initial pin state of the group.
    /// </summary>
    public DocksCollectionPinState PinState { get; init; } = DocksCollectionPinState.Pinned;

    /// <summary>
    /// Gets or sets a static title for the group's sidebar button when in drawer mode.
    /// If set, the sidebar button will always display this title instead of the active panel's title.
    /// If null, the default dock panel behavior is used (shows active panel's title).
    /// </summary>
    public string? GroupTitle { get; init; }

    /// <summary>
    /// Gets or sets the panels in this group that need additional configuration.
    /// </summary>
    public List<DockPanelInitConfig> Panels { get; init; } = [];
}
