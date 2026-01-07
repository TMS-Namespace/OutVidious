using System;
using TMS.Libs.Frontend.Web.DockPanels.Models;
using TMS.Libs.Frontend.Web.DockPanels.Services;

namespace TMS.Libs.Frontend.Web.DockPanels.Components;

public partial class DocksHostComponent
{
    /// <summary>
    /// Gets the underlying dock panel interop service.
    /// </summary>
    public IDockPanelInterop GetInterop() => DockPanelInterop;

    /// <summary>
    /// Shows a drawer tab by panel instance.
    /// </summary>
    public Task<bool> ShowDrawerTabAsync(DockPanelComponent panel, int? widthPx, CancellationToken cancellationToken)
    {
        return ShowDrawerTabAsync(panel.PanelId, widthPx, cancellationToken);
    }

    /// <summary>
    /// Shows a drawer tab by panel ID.
    /// </summary>
    public Task<bool> ShowDrawerTabAsync(Guid panelId, int? widthPx, CancellationToken cancellationToken)
    {
        if (widthPx.HasValue)
        {
            return DockPanelInterop.ShowDrawerTabAsync(DockPanelId, panelId, widthPx.Value, cancellationToken);
        }

        return DockPanelInterop.ShowDrawerTabAsync(DockPanelId, panelId, cancellationToken);
    }

    /// <summary>
    /// Shows a drawer panel by panel instance.
    /// </summary>
    public Task<bool> ShowDrawerAsync(DockPanelComponent panel, CancellationToken cancellationToken)
    {
        return DockPanelInterop.ShowDrawerAsync(DockPanelId, panel.PanelId, cancellationToken);
    }

    /// <summary>
    /// Shows a drawer panel by panel ID.
    /// </summary>
    public Task<bool> ShowDrawerAsync(Guid panelId, CancellationToken cancellationToken)
    {
        return DockPanelInterop.ShowDrawerAsync(DockPanelId, panelId, cancellationToken);
    }

    /// <summary>
    /// Hides a drawer panel by instance.
    /// </summary>
    public Task<bool> HideDrawerAsync(DockPanelComponent panel, CancellationToken cancellationToken)
    {
        return DockPanelInterop.HideDrawerAsync(DockPanelId, panel.PanelId, cancellationToken);
    }

    /// <summary>
    /// Hides a drawer panel by ID.
    /// </summary>
    public Task<bool> HideDrawerAsync(Guid panelId, CancellationToken cancellationToken)
    {
        return DockPanelInterop.HideDrawerAsync(DockPanelId, panelId, cancellationToken);
    }

    /// <summary>
    /// Hides a drawer tab by panel instance.
    /// </summary>
    public Task<bool> HideDrawerTabAsync(DockPanelComponent panel, CancellationToken cancellationToken)
    {
        return DockPanelInterop.HideDrawerTabAsync(DockPanelId, panel.PanelId, cancellationToken);
    }

    /// <summary>
    /// Hides a drawer tab by panel ID.
    /// </summary>
    public Task<bool> HideDrawerTabAsync(Guid panelId, CancellationToken cancellationToken)
    {
        return DockPanelInterop.HideDrawerTabAsync(DockPanelId, panelId, cancellationToken);
    }

    /// <summary>
    /// Activates a panel by instance.
    /// </summary>
    public Task<bool> ActivatePanelAsync(DockPanelComponent panel, CancellationToken cancellationToken)
    {
        return DockPanelInterop.ActivatePanelAsync(DockPanelId, panel.PanelId, cancellationToken);
    }

    /// <summary>
    /// Activates a panel by ID.
    /// </summary>
    public Task<bool> ActivatePanelAsync(Guid panelId, CancellationToken cancellationToken)
    {
        return DockPanelInterop.ActivatePanelAsync(DockPanelId, panelId, cancellationToken);
    }

    /// <summary>
    /// Sets the active panel by instance without toggling drawer visibility.
    /// </summary>
    public Task<bool> SetActivePanelAsync(DockPanelComponent panel, CancellationToken cancellationToken)
    {
        return DockPanelInterop.SetActivePanelAsync(DockPanelId, panel.PanelId, cancellationToken);
    }

    /// <summary>
    /// Sets the active panel by ID without toggling drawer visibility.
    /// </summary>
    public Task<bool> SetActivePanelAsync(Guid panelId, CancellationToken cancellationToken)
    {
        return DockPanelInterop.SetActivePanelAsync(DockPanelId, panelId, cancellationToken);
    }

    /// <summary>
    /// Checks if a panel exists by instance.
    /// </summary>
    public Task<bool> PanelExistsAsync(DockPanelComponent panel, CancellationToken cancellationToken)
    {
        return DockPanelInterop.PanelExistsAsync(DockPanelId, panel.PanelId, cancellationToken);
    }

    /// <summary>
    /// Checks if a panel exists by ID.
    /// </summary>
    public Task<bool> PanelExistsAsync(Guid panelId, CancellationToken cancellationToken)
    {
        return DockPanelInterop.PanelExistsAsync(DockPanelId, panelId, cancellationToken);
    }

    /// <summary>
    /// Gets the current state of a panel by instance.
    /// </summary>
    public Task<DockPanelState?> GetPanelStateAsync(DockPanelComponent panel, CancellationToken cancellationToken)
    {
        return DockPanelInterop.GetPanelStateAsync(DockPanelId, panel.PanelId, cancellationToken);
    }

    /// <summary>
    /// Gets the current state of a panel by ID.
    /// </summary>
    public Task<DockPanelState?> GetPanelStateAsync(Guid panelId, CancellationToken cancellationToken)
    {
        return DockPanelInterop.GetPanelStateAsync(DockPanelId, panelId, cancellationToken);
    }

    /// <summary>
    /// Shows and expands a drawer by panel instance.
    /// </summary>
    public Task<bool> ShowAndExpandDrawerAsync(DockPanelComponent panel, int? widthPx, CancellationToken cancellationToken)
    {
        return ShowAndExpandDrawerAsync(panel.PanelId, widthPx, cancellationToken);
    }

    /// <summary>
    /// Shows and expands a drawer by panel ID.
    /// </summary>
    public Task<bool> ShowAndExpandDrawerAsync(Guid panelId, int? widthPx, CancellationToken cancellationToken)
    {
        if (widthPx.HasValue)
        {
            return DockPanelInterop.ShowAndExpandDrawerAsync(DockPanelId, panelId, widthPx.Value, cancellationToken);
        }

        return DockPanelInterop.ShowAndExpandDrawerAsync(DockPanelId, panelId, cancellationToken);
    }

    /// <summary>
    /// Sets the drawer width by panel instance.
    /// </summary>
    public Task<bool> SetDrawerWidthAsync(DockPanelComponent panel, int widthPx, CancellationToken cancellationToken)
    {
        return DockPanelInterop.SetDrawerWidthAsync(DockPanelId, panel.PanelId, widthPx, cancellationToken);
    }

    /// <summary>
    /// Sets the drawer width by panel ID.
    /// </summary>
    public Task<bool> SetDrawerWidthAsync(Guid panelId, int widthPx, CancellationToken cancellationToken)
    {
        return DockPanelInterop.SetDrawerWidthAsync(DockPanelId, panelId, widthPx, cancellationToken);
    }

    /// <summary>
    /// Checks whether a drawer is fully ready in the DOM.
    /// </summary>
    public Task<bool> IsDrawerReadyAsync(DockPanelComponent panel, CancellationToken cancellationToken)
    {
        return DockPanelInterop.IsDrawerReadyAsync(DockPanelId, panel.PanelId, cancellationToken);
    }

    /// <summary>
    /// Checks whether a drawer is fully ready in the DOM by panel ID.
    /// </summary>
    public Task<bool> IsDrawerReadyAsync(Guid panelId, CancellationToken cancellationToken)
    {
        return DockPanelInterop.IsDrawerReadyAsync(DockPanelId, panelId, cancellationToken);
    }

    /// <summary>
    /// Unpins a group by group component.
    /// </summary>
    public Task<bool> UnpinGroupAsync(DocksCollectionComponent group, CancellationToken cancellationToken)
    {
        if (group.GroupId is null)
        {
            return Task.FromResult(false);
        }

        return DockPanelInterop.UnpinGroupAsync(DockPanelId, group.GroupId.Value, cancellationToken);
    }

    /// <summary>
    /// Unpins a group by group ID.
    /// </summary>
    public Task<bool> UnpinGroupAsync(Guid groupId, CancellationToken cancellationToken)
    {
        return DockPanelInterop.UnpinGroupAsync(DockPanelId, groupId, cancellationToken);
    }

    /// <summary>
    /// Unpins a group using a panel instance.
    /// </summary>
    public Task<bool> UnpinGroupAsync(DockPanelComponent panel, CancellationToken cancellationToken)
    {
        return DockPanelInterop.UnpinPanelAsync(DockPanelId, panel.PanelId, cancellationToken);
    }

    /// <summary>
    /// Pins a panel by instance.
    /// </summary>
    public Task<bool> PinPanelAsync(DockPanelComponent panel, CancellationToken cancellationToken)
    {
        return DockPanelInterop.PinPanelAsync(DockPanelId, panel.PanelId, cancellationToken);
    }

    /// <summary>
    /// Pins a panel by ID.
    /// </summary>
    public Task<bool> PinPanelAsync(Guid panelId, CancellationToken cancellationToken)
    {
        return DockPanelInterop.PinPanelAsync(DockPanelId, panelId, cancellationToken);
    }

    /// <summary>
    /// Floats a panel by instance.
    /// </summary>
    public Task<bool> FloatPanelAsync(DockPanelComponent panel, CancellationToken cancellationToken)
    {
        return DockPanelInterop.FloatPanelAsync(DockPanelId, panel.PanelId, cancellationToken);
    }

    /// <summary>
    /// Floats a panel by ID.
    /// </summary>
    public Task<bool> FloatPanelAsync(Guid panelId, CancellationToken cancellationToken)
    {
        return DockPanelInterop.FloatPanelAsync(DockPanelId, panelId, cancellationToken);
    }

    /// <summary>
    /// Docks a panel by instance.
    /// </summary>
    public Task<bool> DockPanelAsync(DockPanelComponent panel, CancellationToken cancellationToken)
    {
        return DockPanelInterop.DockPanelAsync(DockPanelId, panel.PanelId, cancellationToken);
    }

    /// <summary>
    /// Docks a panel by ID.
    /// </summary>
    public Task<bool> DockPanelAsync(Guid panelId, CancellationToken cancellationToken)
    {
        return DockPanelInterop.DockPanelAsync(DockPanelId, panelId, cancellationToken);
    }

    /// <summary>
    /// Collapses a panel by instance.
    /// </summary>
    public Task<bool> CollapsePanelAsync(DockPanelComponent panel, CancellationToken cancellationToken)
    {
        return DockPanelInterop.CollapsePanelAsync(DockPanelId, panel.PanelId, cancellationToken);
    }

    /// <summary>
    /// Collapses a panel by ID.
    /// </summary>
    public Task<bool> CollapsePanelAsync(Guid panelId, CancellationToken cancellationToken)
    {
        return DockPanelInterop.CollapsePanelAsync(DockPanelId, panelId, cancellationToken);
    }

    /// <summary>
    /// Expands a panel by instance.
    /// </summary>
    public Task<bool> ExpandPanelAsync(DockPanelComponent panel, CancellationToken cancellationToken)
    {
        return DockPanelInterop.ExpandPanelAsync(DockPanelId, panel.PanelId, cancellationToken);
    }

    /// <summary>
    /// Expands a panel by ID.
    /// </summary>
    public Task<bool> ExpandPanelAsync(Guid panelId, CancellationToken cancellationToken)
    {
        return DockPanelInterop.ExpandPanelAsync(DockPanelId, panelId, cancellationToken);
    }

    /// <summary>
    /// Shows a panel group by instance.
    /// </summary>
    public Task<bool> ShowGroupAsync(DockPanelComponent panel, CancellationToken cancellationToken)
    {
        return DockPanelInterop.ShowGroupAsync(DockPanelId, panel.PanelId, cancellationToken);
    }

    /// <summary>
    /// Shows a panel group by ID.
    /// </summary>
    public Task<bool> ShowGroupAsync(Guid panelId, CancellationToken cancellationToken)
    {
        return DockPanelInterop.ShowGroupAsync(DockPanelId, panelId, cancellationToken);
    }

    /// <summary>
    /// Hides a panel group by instance.
    /// </summary>
    public Task<bool> HideGroupAsync(DockPanelComponent panel, CancellationToken cancellationToken)
    {
        return DockPanelInterop.HideGroupAsync(DockPanelId, panel.PanelId, cancellationToken);
    }

    /// <summary>
    /// Hides a panel group by ID.
    /// </summary>
    public Task<bool> HideGroupAsync(Guid panelId, CancellationToken cancellationToken)
    {
        return DockPanelInterop.HideGroupAsync(DockPanelId, panelId, cancellationToken);
    }

    /// <summary>
    /// Checks group visibility by instance.
    /// </summary>
    public Task<bool> IsGroupVisibleAsync(DockPanelComponent panel, CancellationToken cancellationToken)
    {
        return DockPanelInterop.IsGroupVisibleAsync(DockPanelId, panel.PanelId, cancellationToken);
    }

    /// <summary>
    /// Checks group visibility by ID.
    /// </summary>
    public Task<bool> IsGroupVisibleAsync(Guid panelId, CancellationToken cancellationToken)
    {
        return DockPanelInterop.IsGroupVisibleAsync(DockPanelId, panelId, cancellationToken);
    }

    /// <summary>
    /// Removes a panel by instance.
    /// </summary>
    public Task<bool> RemovePanelAsync(DockPanelComponent panel, CancellationToken cancellationToken)
    {
        return DockPanelInterop.RemovePanelAsync(DockPanelId, panel.PanelId, cancellationToken);
    }

    /// <summary>
    /// Removes a panel by ID.
    /// </summary>
    public Task<bool> RemovePanelAsync(Guid panelId, CancellationToken cancellationToken)
    {
        return DockPanelInterop.RemovePanelAsync(DockPanelId, panelId, cancellationToken);
    }

    /// <summary>
    /// Adds a new panel to a new group at the specified position.
    /// </summary>
    public Task<bool> AddPanelToNewGroupAsync(Guid panelId, string panelTitle, string position, Guid? referenceGroupId, CancellationToken cancellationToken)
    {
        return DockPanelInterop.AddPanelToNewGroupAsync(DockPanelId, panelId, panelTitle, position, referenceGroupId, cancellationToken);
    }

    /// <summary>
    /// Sets a static title for a group's sidebar button by panel instance.
    /// </summary>
    public Task<bool> SetGroupStaticTitleAsync(DockPanelComponent panel, string staticTitle, CancellationToken cancellationToken)
    {
        return DockPanelInterop.SetGroupStaticTitleAsync(DockPanelId, panel.PanelId, staticTitle, cancellationToken);
    }

    /// <summary>
    /// Sets a static title for a group's sidebar button by panel ID.
    /// </summary>
    public Task<bool> SetGroupStaticTitleAsync(Guid panelId, string staticTitle, CancellationToken cancellationToken)
    {
        return DockPanelInterop.SetGroupStaticTitleAsync(DockPanelId, panelId, staticTitle, cancellationToken);
    }

    /// <summary>
    /// Clears the static title for a group by panel instance.
    /// </summary>
    public Task<bool> ClearGroupStaticTitleAsync(DockPanelComponent panel, CancellationToken cancellationToken)
    {
        return DockPanelInterop.ClearGroupStaticTitleAsync(DockPanelId, panel.PanelId, cancellationToken);
    }

    /// <summary>
    /// Clears the static title for a group by panel ID.
    /// </summary>
    public Task<bool> ClearGroupStaticTitleAsync(Guid panelId, CancellationToken cancellationToken)
    {
        return DockPanelInterop.ClearGroupStaticTitleAsync(DockPanelId, panelId, cancellationToken);
    }
}
