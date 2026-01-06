using System.Text.Json;
using System.Text.Json.Serialization;
using TMS.Libs.Frontend.Web.DockPanels.Enums;

namespace TMS.Libs.Frontend.Web.DockPanels.Converters;

internal sealed class DockPanelFloatTypeJsonConverter : JsonConverter<DockPanelFloatType>
{
    public override DockPanelFloatType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            return DockPanelFloatType.None;
        }

        var value = reader.GetString();
        return value?.ToLowerInvariant() switch
        {
            "drawer" => DockPanelFloatType.Drawer,
            _ => DockPanelFloatType.None
        };
    }

    public override void Write(Utf8JsonWriter writer, DockPanelFloatType value, JsonSerializerOptions options)
    {
        var stringValue = value switch
        {
            DockPanelFloatType.Drawer => "drawer",
            _ => null
        };

        if (stringValue is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(stringValue);
    }
}
