using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace StencilPad.Schemas;

public class ColorConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected start of array.");
        }

        reader.Read();
        var r = reader.GetByte();

        reader.Read();
        var g = reader.GetByte();

        reader.Read();
        var b = reader.GetByte();

        reader.Read();
        var a = reader.GetByte();

        reader.Read();

        if (reader.TokenType != JsonTokenType.EndArray)
        {
            throw new JsonException("Expected end of array.");
        }

        return Color.FromArgb(a, r, g, b);
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.R);
        writer.WriteNumberValue(value.G);
        writer.WriteNumberValue(value.B);
        writer.WriteNumberValue(value.A);
        writer.WriteEndArray();
    }
}
