using System.Text.Json.Serialization;

namespace TMS.Libs.Frontend.Web.DockPanels.Models;

public sealed record DockPanelDrawerOptions
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Width { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Visible { get; init; }
}
