using System.Text.Json;
using System.Text.Json.Serialization;
using TMS.Libs.Frontend.Web.DockPanels.Enums;

namespace TMS.Libs.Frontend.Web.DockPanels.Converters;

internal sealed class DockPanelDrawerDirectionJsonConverter : JsonConverter<DockPanelDrawerDirection>
{
    public override DockPanelDrawerDirection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            return DockPanelDrawerDirection.None;
        }

        var value = reader.GetString();
        return value?.ToLowerInvariant() switch
        {
            "left" => DockPanelDrawerDirection.Left,
            "right" => DockPanelDrawerDirection.Right,
            _ => DockPanelDrawerDirection.None
        };
    }

    public override void Write(Utf8JsonWriter writer, DockPanelDrawerDirection value, JsonSerializerOptions options)
    {
        var stringValue = value switch
        {
            DockPanelDrawerDirection.Left => "left",
            DockPanelDrawerDirection.Right => "right",
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
