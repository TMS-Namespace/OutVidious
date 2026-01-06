using TMS.Libs.Frontend.Web.DockViewWrapper.Models;

namespace TMS.Libs.Frontend.Web.DockViewWrapper.Services;

/// <summary>
/// Provides programmatic control over DockViewV2 panel states.
/// </summary>
public interface IDockViewInterop : IAsyncDisposable
{
    /// <summary>
    /// Initializes the interop module. Must be called after the component is rendered.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task InitializeAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Unpins a group by its index (converts it to a drawer).
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="groupIndex">The 0-based index of the group to unpin.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> UnpinGroupAsync(string dockViewId, int groupIndex, CancellationToken cancellationToken);

    /// <summary>
    /// Unpins a panel (converts it to a drawer).
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelTitle">The title of the panel to unpin.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> UnpinPanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken);

    /// <summary>
    /// Unpins a panel's group by the panel title.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelTitle">The title of a panel in the group to unpin.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> UnpinGroupByPanelTitleAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken);

    /// <summary>
    /// Pins a panel (docks a drawer back to the grid).
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelTitle">The title of the panel to pin.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> PinPanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken);

    /// <summary>
    /// Floats a panel (converts a grid panel to a floating window).
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelTitle">The title of the panel to float.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> FloatPanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken);

    /// <summary>
    /// Docks a floating panel back to the grid.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelTitle">The title of the panel to dock.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> DockPanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken);

    /// <summary>
    /// Collapses a floating panel (shows only the header).
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelTitle">The title of the panel to collapse.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> CollapsePanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken);

    /// <summary>
    /// Expands a collapsed floating panel.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelTitle">The title of the panel to expand.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> ExpandPanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the current state of a panel.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelTitle">The title of the panel.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The panel state, or null if not found.</returns>
    Task<DockPanelState?> GetPanelStateAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken);

    /// <summary>
    /// Shows a drawer panel (slides it in).
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelTitle">The title of the panel to show.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> ShowDrawerAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken);

    /// <summary>
    /// Hides a drawer panel (slides it out).
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelTitle">The title of the panel to hide.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> HideDrawerAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken);

    /// <summary>
    /// Activates (focuses) a panel and expands its drawer if applicable.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelTitle">The title of the panel to activate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> ActivatePanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if a panel with the given title exists.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelTitle">The title of the panel to check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the panel exists.</returns>
    Task<bool> PanelExistsAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken);

    /// <summary>
    /// Hides a drawer's tab button in the sidebar.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelTitle">The title of the panel whose tab to hide.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> HideDrawerTabAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken);

    /// <summary>
    /// Shows a drawer's tab button that was previously hidden.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelTitle">The title of the panel whose tab to show.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> ShowDrawerTabAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken);

    /// <summary>
    /// Shows a drawer's tab button with a specific width.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelTitle">The title of the panel whose tab to show.</param>
    /// <param name="widthPx">The width in pixels to set for the drawer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> ShowDrawerTabAsync(string dockViewId, string panelTitle, int widthPx, CancellationToken cancellationToken);

    /// <summary>
    /// Shows a drawer's tab button (if hidden), sets its width, and expands it.
    /// This is a combined operation that ensures the drawer is visible and expanded.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelTitle">The title of the panel whose drawer to show and expand.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> ShowAndExpandDrawerAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken);

    /// <summary>
    /// Shows a drawer's tab button (if hidden), sets its width, and expands it.
    /// This is a combined operation that ensures the drawer is visible and expanded.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelTitle">The title of the panel whose drawer to show and expand.</param>
    /// <param name="widthPx">The width in pixels to set for the drawer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> ShowAndExpandDrawerAsync(string dockViewId, string panelTitle, int widthPx, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the width of a drawer panel.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelTitle">The title of the panel.</param>
    /// <param name="widthPx">The width in pixels.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> SetDrawerWidthAsync(string dockViewId, string panelTitle, int widthPx, CancellationToken cancellationToken);

    /// <summary>
    /// Shows a panel's group.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelTitle">The title of the panel whose group to show.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> ShowGroupAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken);

    /// <summary>
    /// Hides a panel's group.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelTitle">The title of the panel whose group to hide.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> HideGroupAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if a panel's group is visible.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelTitle">The title of the panel to check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the group is visible.</returns>
    Task<bool> IsGroupVisibleAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken);

    /// <summary>
    /// Removes a panel by its title.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelTitle">The title of the panel to remove.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> RemovePanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new panel to a new group at the specified position.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The unique ID for the new panel.</param>
    /// <param name="panelTitle">The title for the new panel.</param>
    /// <param name="position">Where to place the group: "top", "bottom", "left", "right".</param>
    /// <param name="referenceGroupIndex">The index of the reference group for position context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> AddPanelToNewGroupAsync(
        string dockViewId,
        string panelId,
        string panelTitle,
        string position,
        int? referenceGroupIndex,
        CancellationToken cancellationToken);

    /// <summary>
    /// Sets a static title for a group's sidebar button that persists across panel changes.
    /// When set, the button text will not change when switching between panels in the group.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelTitle">The title of any panel in the group (used to find the sidebar button).</param>
    /// <param name="staticTitle">The static title to display on the sidebar button.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> SetGroupStaticTitleAsync(
        string dockViewId,
        string panelTitle,
        string staticTitle,
        CancellationToken cancellationToken);

    /// <summary>
    /// Removes the static title from a group's sidebar button, reverting to default DockView behavior.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelTitle">The title of any panel in the group.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> ClearGroupStaticTitleAsync(
        string dockViewId,
        string panelTitle,
        CancellationToken cancellationToken);

    #region Key-based methods (preferred over title-based)

    /// <summary>
    /// Hides a drawer's tab button in the sidebar by panel key.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelKey">The unique key of the panel whose tab to hide.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> HideDrawerTabByKeyAsync(string dockViewId, string panelKey, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the width of a drawer panel by panel key.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelKey">The unique key of the panel.</param>
    /// <param name="widthPx">The width in pixels.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> SetDrawerWidthByKeyAsync(string dockViewId, string panelKey, int widthPx, CancellationToken cancellationToken);

    /// <summary>
    /// Sets a static title for a group's sidebar button by panel key.
    /// When set, the button text will not change when switching between panels in the group.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelKey">The unique key of any panel in the group.</param>
    /// <param name="staticTitle">The static title to display on the sidebar button.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> SetGroupStaticTitleByKeyAsync(
        string dockViewId,
        string panelKey,
        string staticTitle,
        CancellationToken cancellationToken);

    /// <summary>
    /// Shows a drawer's tab button by panel key.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelKey">The unique key of the panel whose tab to show.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> ShowDrawerTabByKeyAsync(string dockViewId, string panelKey, CancellationToken cancellationToken);

    /// <summary>
    /// Shows a drawer's tab button by panel key with a specific width.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelKey">The unique key of the panel whose tab to show.</param>
    /// <param name="widthPx">The width in pixels to set for the drawer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> ShowDrawerTabByKeyAsync(string dockViewId, string panelKey, int widthPx, CancellationToken cancellationToken);

    /// <summary>
    /// Activates (focuses) a panel by key and expands its drawer if applicable.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelKey">The unique key of the panel to activate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> ActivatePanelByKeyAsync(string dockViewId, string panelKey, CancellationToken cancellationToken);

    #endregion
}
