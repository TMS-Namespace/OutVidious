

namespace TMS.Libs.Frontend.Web.DockPanels.Models;

/// <summary>
/// Dock panel options.
/// </summary>
internal sealed class DockPanelOptions
{
    /// <summary>
    /// Gets or sets the layout version string.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets whether local storage is enabled.
    /// </summary>
    public bool? EnableLocalStorage { get; set; }

    /// <summary>
    /// Gets or sets the local storage prefix.
    /// </summary>
    public string? LocalStoragePrefix { get; set; }
}
