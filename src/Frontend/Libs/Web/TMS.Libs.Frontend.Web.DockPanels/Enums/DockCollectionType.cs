

using BootstrapBlazor.Components;

namespace TMS.Libs.Frontend.Web.DockPanels.Enums;

/// <summary>
/// Dock panel content layout type.
/// </summary>
[JsonEnumConverter(true)]
public enum DockCollectionType
{
    /// <summary>
    /// Row layout (horizontal).
    /// </summary>
    Row,

    /// <summary>
    /// Column layout (vertical).
    /// </summary>
    Column,

    /// <summary>
    /// Group (tabbed) layout.
    /// </summary>
    Group,

    /// <summary>
    /// Component (leaf) item.
    /// </summary>
    Component,
}
