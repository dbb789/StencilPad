using System.Text.Json;
using System.Text.Json.Serialization;
using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class Unit2DConverter : JsonConverter<Unit2D>
{
    public override Unit2D Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object.");
        }

        var unitConverter = (JsonConverter<Unit>)options.GetConverter(typeof(Unit));

        Unit x = Unit.Zero;
        Unit y = Unit.Zero;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new Unit2D(x, y);
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var name = reader.GetString();
                reader.Read();

                if (name == "X")
                {
                    x = unitConverter.Read(ref reader, typeof(Unit), options);
                }
                else if (name == "Y")
                {
                    y = unitConverter.Read(ref reader, typeof(Unit), options);
                }
            }
        }

        throw new JsonException("Unexpected end of JSON.");
    }

    public override void Write(Utf8JsonWriter writer, Unit2D value, JsonSerializerOptions options)
    {
        var unitConverter = (JsonConverter<Unit>)options.GetConverter(typeof(Unit));

        writer.WriteStartObject();
        writer.WritePropertyName("X");
        unitConverter.Write(writer, value.X, options);
        writer.WritePropertyName("Y");
        unitConverter.Write(writer, value.Y, options);
        writer.WriteEndObject();
    }
}
