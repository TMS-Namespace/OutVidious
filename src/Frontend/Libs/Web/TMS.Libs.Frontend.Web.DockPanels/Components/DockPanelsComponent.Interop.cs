using TMS.Libs.Frontend.Web.DockPanels.Models;
using TMS.Libs.Frontend.Web.DockPanels.Services;

namespace TMS.Libs.Frontend.Web.DockPanels.Components;

public partial class DockPanelsComponent
{
    /// <summary>
    /// Gets the underlying dock panel interop service.
    /// </summary>
    public IDockPanelInterop GetInterop() => DockPanelInterop;

    /// <summary>
    /// Shows a drawer tab by title.
    /// </summary>
    /// <param name="panelTitle">The panel title.</param>
    /// <param name="widthPx">Optional drawer width.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<bool> ShowDrawerTabAsync(string panelTitle, int? widthPx, CancellationToken cancellationToken)
    {
        if (widthPx.HasValue)
        {
            return DockPanelInterop.ShowDrawerTabAsync(DockPanelId, panelTitle, widthPx.Value, cancellationToken);
        }

        return DockPanelInterop.ShowDrawerTabAsync(DockPanelId, panelTitle, cancellationToken);
    }

    /// <summary>
    /// Shows a drawer panel by title.
    /// </summary>
    /// <param name="panelTitle">The panel title.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<bool> ShowDrawerAsync(string panelTitle, CancellationToken cancellationToken)
    {
        return DockPanelInterop.ShowDrawerAsync(DockPanelId, panelTitle, cancellationToken);
    }

    /// <summary>
    /// Hides a drawer tab by title.
    /// </summary>
    /// <param name="panelTitle">The panel title.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<bool> HideDrawerTabAsync(string panelTitle, CancellationToken cancellationToken)
    {
        return DockPanelInterop.HideDrawerTabAsync(DockPanelId, panelTitle, cancellationToken);
    }

    /// <summary>
    /// Activates a panel by title.
    /// </summary>
    /// <param name="panelTitle">The panel title.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<bool> ActivatePanelAsync(string panelTitle, CancellationToken cancellationToken)
    {
        return DockPanelInterop.ActivatePanelAsync(DockPanelId, panelTitle, cancellationToken);
    }

    /// <summary>
    /// Sets the active panel by title without toggling drawer visibility.
    /// </summary>
    /// <param name="panelTitle">The panel title.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<bool> SetActivePanelAsync(string panelTitle, CancellationToken cancellationToken)
    {
        return DockPanelInterop.SetActivePanelAsync(DockPanelId, panelTitle, cancellationToken);
    }

    /// <summary>
    /// Checks if a panel with the given title exists.
    /// </summary>
    /// <param name="panelTitle">The panel title.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<bool> PanelExistsAsync(string panelTitle, CancellationToken cancellationToken)
    {
        return DockPanelInterop.PanelExistsAsync(DockPanelId, panelTitle, cancellationToken);
    }

    /// <summary>
    /// Gets the current state of a panel by title.
    /// </summary>
    /// <param name="panelTitle">The panel title.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<DockPanelState?> GetPanelStateAsync(string panelTitle, CancellationToken cancellationToken)
    {
        return DockPanelInterop.GetPanelStateAsync(DockPanelId, panelTitle, cancellationToken);
    }

    /// <summary>
    /// Checks if a panel with the given key exists.
    /// </summary>
    /// <param name="panelKey">The panel key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<bool> PanelExistsByKeyAsync(string panelKey, CancellationToken cancellationToken)
    {
        return DockPanelInterop.PanelExistsByKeyAsync(DockPanelId, panelKey, cancellationToken);
    }

    /// <summary>
    /// Shows and expands a drawer by title.
    /// </summary>
    /// <param name="panelTitle">The panel title.</param>
    /// <param name="widthPx">Optional drawer width.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<bool> ShowAndExpandDrawerAsync(string panelTitle, int? widthPx, CancellationToken cancellationToken)
    {
        if (widthPx.HasValue)
        {
            return DockPanelInterop.ShowAndExpandDrawerAsync(DockPanelId, panelTitle, widthPx.Value, cancellationToken);
        }

        return DockPanelInterop.ShowAndExpandDrawerAsync(DockPanelId, panelTitle, cancellationToken);
    }

    /// <summary>
    /// Sets the drawer width by title.
    /// </summary>
    /// <param name="panelTitle">The panel title.</param>
    /// <param name="widthPx">The drawer width.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<bool> SetDrawerWidthAsync(string panelTitle, int widthPx, CancellationToken cancellationToken)
    {
        return DockPanelInterop.SetDrawerWidthAsync(DockPanelId, panelTitle, widthPx, cancellationToken);
    }

    /// <summary>
    /// Checks whether a drawer is fully ready in the DOM (button + container).
    /// </summary>
    /// <param name="panelTitle">The panel title.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<bool> IsDrawerReadyAsync(string panelTitle, CancellationToken cancellationToken)
    {
        return DockPanelInterop.IsDrawerReadyAsync(DockPanelId, panelTitle, cancellationToken);
    }

    /// <summary>
    /// Shows a drawer tab by panel key.
    /// </summary>
    /// <param name="panelKey">The panel key.</param>
    /// <param name="widthPx">Optional drawer width.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<bool> ShowDrawerTabByKeyAsync(string panelKey, int? widthPx, CancellationToken cancellationToken)
    {
        if (widthPx.HasValue)
        {
            return DockPanelInterop.ShowDrawerTabByKeyAsync(DockPanelId, panelKey, widthPx.Value, cancellationToken);
        }

        return DockPanelInterop.ShowDrawerTabByKeyAsync(DockPanelId, panelKey, cancellationToken);
    }

    /// <summary>
    /// Hides a drawer tab by panel key.
    /// </summary>
    /// <param name="panelKey">The panel key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<bool> HideDrawerTabByKeyAsync(string panelKey, CancellationToken cancellationToken)
    {
        return DockPanelInterop.HideDrawerTabByKeyAsync(DockPanelId, panelKey, cancellationToken);
    }

    /// <summary>
    /// Activates a panel by key.
    /// </summary>
    /// <param name="panelKey">The panel key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<bool> ActivatePanelByKeyAsync(string panelKey, CancellationToken cancellationToken)
    {
        return DockPanelInterop.ActivatePanelByKeyAsync(DockPanelId, panelKey, cancellationToken);
    }

    /// <summary>
    /// Sets the active panel by key without toggling drawer visibility.
    /// </summary>
    /// <param name="panelKey">The panel key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<bool> SetActivePanelByKeyAsync(string panelKey, CancellationToken cancellationToken)
    {
        return DockPanelInterop.SetActivePanelByKeyAsync(DockPanelId, panelKey, cancellationToken);
    }

    /// <summary>
    /// Sets the drawer width by panel key.
    /// </summary>
    /// <param name="panelKey">The panel key.</param>
    /// <param name="widthPx">The drawer width.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<bool> SetDrawerWidthByKeyAsync(string panelKey, int widthPx, CancellationToken cancellationToken)
    {
        return DockPanelInterop.SetDrawerWidthByKeyAsync(DockPanelId, panelKey, widthPx, cancellationToken);
    }

    /// <summary>
    /// Unpins a group by index to make it a drawer.
    /// </summary>
    /// <param name="groupIndex">The group index.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<bool> UnpinGroupAsync(int groupIndex, CancellationToken cancellationToken)
    {
        return DockPanelInterop.UnpinGroupAsync(DockPanelId, groupIndex, cancellationToken);
    }

    /// <summary>
    /// Unpins a panel's group by the panel title.
    /// </summary>
    /// <param name="panelTitle">The panel title.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<bool> UnpinGroupByPanelTitleAsync(string panelTitle, CancellationToken cancellationToken)
    {
        return DockPanelInterop.UnpinGroupByPanelTitleAsync(DockPanelId, panelTitle, cancellationToken);
    }

    /// <summary>
    /// Sets a static title for a group by panel key.
    /// </summary>
    /// <param name="panelKey">The panel key.</param>
    /// <param name="staticTitle">The static title to set.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<bool> SetGroupStaticTitleByKeyAsync(string panelKey, string staticTitle, CancellationToken cancellationToken)
    {
        return DockPanelInterop.SetGroupStaticTitleByKeyAsync(DockPanelId, panelKey, staticTitle, cancellationToken);
    }

    /// <summary>
    /// Sets a static title for a group by panel title.
    /// </summary>
    /// <param name="panelTitle">The panel title.</param>
    /// <param name="staticTitle">The static title to set.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<bool> SetGroupStaticTitleAsync(string panelTitle, string staticTitle, CancellationToken cancellationToken)
    {
        return DockPanelInterop.SetGroupStaticTitleAsync(DockPanelId, panelTitle, staticTitle, cancellationToken);
    }

    /// <summary>
    /// Unpins a panel's group by the panel key.
    /// </summary>
    /// <param name="panelKey">The unique key of a panel in the group to unpin.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<bool> UnpinGroupByPanelKeyAsync(string panelKey, CancellationToken cancellationToken)
    {
        return DockPanelInterop.UnpinGroupByPanelKeyAsync(DockPanelId, panelKey, cancellationToken);
    }
}
