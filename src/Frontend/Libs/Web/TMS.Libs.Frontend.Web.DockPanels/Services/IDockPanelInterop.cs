using TMS.Libs.Frontend.Web.DockPanels.Models;

namespace TMS.Libs.Frontend.Web.DockPanels.Services;

/// <summary>
/// Provides programmatic control over dock panel states.
/// </summary>
public interface IDockPanelInterop : IAsyncDisposable
{
    /// <summary>
    /// Initializes the interop module. Must be called after the component is rendered.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task InitializeAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Unpins a group by its internal ID (converts it to a drawer).
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="groupId">The internal group ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> UnpinGroupAsync(string dockViewId, Guid groupId, CancellationToken cancellationToken);

    /// <summary>
    /// Unpins a panel (converts its group to a drawer).
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> UnpinPanelAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken);

    /// <summary>
    /// Pins a panel (docks a drawer back to the grid).
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> PinPanelAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken);

    /// <summary>
    /// Floats a panel (converts a grid panel to a floating window).
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> FloatPanelAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken);

    /// <summary>
    /// Docks a floating panel back to the grid.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> DockPanelAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken);

    /// <summary>
    /// Collapses a floating panel (shows only the header).
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> CollapsePanelAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken);

    /// <summary>
    /// Expands a collapsed floating panel.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> ExpandPanelAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the current state of a panel.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The panel state, or null if not found.</returns>
    Task<DockPanelState?> GetPanelStateAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken);

    /// <summary>
    /// Shows a drawer panel (slides it in).
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> ShowDrawerAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken);

    /// <summary>
    /// Hides a drawer panel (slides it out).
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> HideDrawerAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken);

    /// <summary>
    /// Activates (focuses) a panel and expands its drawer if applicable.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> ActivatePanelAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the active panel without toggling drawer visibility.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> SetActivePanelAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if a panel with the given ID exists.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the panel exists.</returns>
    Task<bool> PanelExistsAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken);

    /// <summary>
    /// Hides a drawer's tab button in the sidebar.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> HideDrawerTabAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken);

    /// <summary>
    /// Shows a drawer's tab button that was previously hidden.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> ShowDrawerTabAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken);

    /// <summary>
    /// Shows a drawer's tab button with a specific width.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID.</param>
    /// <param name="widthPx">The width in pixels to set for the drawer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> ShowDrawerTabAsync(string dockViewId, Guid panelId, int widthPx, CancellationToken cancellationToken);

    /// <summary>
    /// Shows a drawer's tab button (if hidden), sets its width, and expands it.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> ShowAndExpandDrawerAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken);

    /// <summary>
    /// Shows a drawer's tab button (if hidden), sets its width, and expands it.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID.</param>
    /// <param name="widthPx">The width in pixels to set for the drawer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> ShowAndExpandDrawerAsync(string dockViewId, Guid panelId, int widthPx, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the width of a drawer panel.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID.</param>
    /// <param name="widthPx">The width in pixels.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> SetDrawerWidthAsync(string dockViewId, Guid panelId, int widthPx, CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether a drawer is fully ready in the DOM (button + container).
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the drawer elements are ready.</returns>
    Task<bool> IsDrawerReadyAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken);

    /// <summary>
    /// Shows a panel's group.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> ShowGroupAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken);

    /// <summary>
    /// Hides a panel's group.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> HideGroupAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if a panel's group is visible.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the group is visible.</returns>
    Task<bool> IsGroupVisibleAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken);

    /// <summary>
    /// Removes a panel by its ID.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> RemovePanelAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new panel to a new group at the specified position.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID for the new panel.</param>
    /// <param name="panelTitle">The title for the new panel.</param>
    /// <param name="position">Where to place the group: "top", "bottom", "left", "right", "within".</param>
    /// <param name="referenceGroupId">The reference group ID for position context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> AddPanelToNewGroupAsync(
        string dockViewId,
        Guid panelId,
        string panelTitle,
        string position,
        Guid? referenceGroupId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Sets a static title for a group's sidebar button that persists across panel changes.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID used to resolve the group.</param>
    /// <param name="staticTitle">The static title to display on the sidebar button.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> SetGroupStaticTitleAsync(
        string dockViewId,
        Guid panelId,
        string staticTitle,
        CancellationToken cancellationToken);

    /// <summary>
    /// Removes the static title from a group's sidebar button.
    /// </summary>
    /// <param name="dockViewId">The DockViewV2 element ID.</param>
    /// <param name="panelId">The internal panel ID used to resolve the group.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    Task<bool> ClearGroupStaticTitleAsync(
        string dockViewId,
        Guid panelId,
        CancellationToken cancellationToken);
}
