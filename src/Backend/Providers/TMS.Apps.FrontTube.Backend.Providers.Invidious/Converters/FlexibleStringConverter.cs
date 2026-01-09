using System.Text.Json;
using System.Text.Json.Serialization;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.Converters;

/// <summary>
/// JSON converter that handles values that can be either string or number in JSON
/// and converts them to string. This is needed because Invidious API returns
/// inconsistent types for some fields.
/// </summary>
internal sealed class FlexibleStringConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => reader.TryGetInt64(out var longValue) 
                ? longValue.ToString() 
                : reader.GetDouble().ToString(),
            JsonTokenType.Null => null,
            JsonTokenType.True => "true",
            JsonTokenType.False => "false",
            _ => throw new JsonException($"Unexpected token type: {reader.TokenType}")
        };
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value);
        }
    }
}
