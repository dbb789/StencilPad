using System.Text.Json;
using System.Text.Json.Serialization;
using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class FractionConverter : JsonConverter<Fraction>
{
    public override Fraction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected start of array.");
        }

        reader.Read();
        var numerator = reader.GetInt32();

        reader.Read();
        var denominator = reader.GetInt32();

        reader.Read();

        if (reader.TokenType != JsonTokenType.EndArray)
        {
            throw new JsonException("Expected end of array.");
        }

        return new Fraction(numerator, denominator);
    }

    public override void Write(Utf8JsonWriter writer, Fraction value, JsonSerializerOptions options)
    {
        var unitConverter = (JsonConverter<Unit>)options.GetConverter(typeof(Unit));

        writer.WriteStartArray();
        writer.WriteNumberValue(value.Numerator);
        writer.WriteNumberValue(value.Denominator);
        writer.WriteEndArray();
    }
}
