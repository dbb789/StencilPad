using System.Text.Json;
using System.Text.Json.Serialization;
using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class UnitConverter : JsonConverter<Unit>
{
    public override Unit Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var s = reader.GetString() ?? string.Empty;

        if (!Unit.TryParse(s, out var result))
        {
            throw new JsonException($"Could not parse '{s}' as a Unit.");
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, Unit value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(6));
    }
}
