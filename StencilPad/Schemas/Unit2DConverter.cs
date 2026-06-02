using System.Text.Json;
using System.Text.Json.Serialization;
using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class Unit2DConverter : JsonConverter<Unit2D>
{
    public override Unit2D Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected start of array.");
        }

        var unitConverter = (JsonConverter<Unit>)options.GetConverter(typeof(Unit));

        reader.Read();
        var x = unitConverter.Read(ref reader, typeof(Unit), options);

        reader.Read();
        var y = unitConverter.Read(ref reader, typeof(Unit), options);

        reader.Read();

        if (reader.TokenType != JsonTokenType.EndArray)
        {
            throw new JsonException("Expected end of array.");
        }

        return new Unit2D(x, y);
    }

    public override void Write(Utf8JsonWriter writer, Unit2D value, JsonSerializerOptions options)
    {
        var unitConverter = (JsonConverter<Unit>)options.GetConverter(typeof(Unit));

        writer.WriteStartArray();
        unitConverter.Write(writer, value.X, options);
        unitConverter.Write(writer, value.Y, options);
        writer.WriteEndArray();
    }
}
