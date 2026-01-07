

using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;
using TMS.Libs.Frontend.Web.DockPanels.Enums;

namespace TMS.Libs.Frontend.Web.DockPanels.Components;

/// <summary>
/// Dock panel component base.
/// </summary>
public abstract class DockPanelComponentBase : IdComponentBase, IDisposable
{
    /// <summary>
    /// Gets the internal GUID for this component instance.
    /// This value is generated internally and cannot be set by consumers.
    /// </summary>
    [JsonIgnore]
    public Guid ComponentId { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the render type. Default is <see cref="DockCollectionType.Component"/>.
    /// </summary>
    [Parameter]
    public DockCollectionType Type { get; set; }

    /// <summary>
    /// Gets or sets the component width percentage.
    /// </summary>
    [Parameter]
    public int? Width { get; set; }

    /// <summary>
    /// Gets or sets the component height percentage.
    /// </summary>
    [Parameter]
    public int? Height { get; set; }

    /// <summary>
    /// Gets or sets the child content.
    /// </summary>
    [Parameter]
    [JsonIgnore]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the collection of dock items from the parent container.
    /// </summary>
    [CascadingParameter]
    private List<DockPanelComponentBase>? Parent { get; set; }

    /// <summary>
    /// Gets the string representation used for the DockView internal ID.
    /// </summary>
    internal string ComponentIdString => ComponentId.ToString("D");

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        Id = ComponentIdString;
        Parent?.Add(this);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        Id = ComponentIdString;
    }

    /// <summary>
    /// Disposes managed resources.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Parent?.Remove(this);
        }
    }

    /// <summary>
    /// Disposes the component.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
