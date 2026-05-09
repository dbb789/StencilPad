using System.Text.Json;
using System.Text.Json.Serialization;

namespace StencilPad.Schemas;

public static class SchemaJsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        Converters = { new JsonStringEnumConverter(), new UnitConverter(), new Unit2DConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
