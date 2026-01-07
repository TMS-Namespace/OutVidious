

using System.Diagnostics.CodeAnalysis;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace TMS.Libs.Frontend.Web.DockPanels.Components;

/// <summary>
/// Dock panel icon component.
/// </summary>
public partial class DockPanelIconComponent
{
    private const string IconSpritePath = "./_content/TMS.Libs.Frontend.Web.DockPanels/icon/dockview.svg";

    /// <summary>
    /// Gets or sets the localizer instance.
    /// </summary>
    [Inject, NotNull]
    protected IStringLocalizer<DockPanelIconComponent>? Localizer { get; set; }

    /// <summary>
    /// Gets or sets the icon name.
    /// </summary>
    [Parameter, NotNull]
    [EditorRequired]
    public string? IconName { get; set; }

    /// <summary>
    /// Gets the CSS class string.
    /// </summary>
    private string? ClassString => CssBuilder.Default("bb-dockview-icon")
        .AddClass($"bb-dockview-icon-{IconName}")
        .Build();

    /// <summary>
    /// Gets the icon href.
    /// </summary>
    protected string Href => $"{IconSpritePath}#{IconName}";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        IconName ??= "close";
    }
}
