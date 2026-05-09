using System.Text.Json.Serialization;
using StencilPad.Models;

namespace StencilPad.Schemas;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ShapeSchema), "shape")]
[JsonDerivedType(typeof(MarkerPathSchema), "MarkerPath")]
public abstract class SheetElementSchema
{
    public abstract ISheetElement Unpack();

    public static SheetElementSchema? Pack(ISheetElement element)
    {
        return element switch
        {
            Shape s => ShapeSchema.Pack(s),
            MarkerPath s => MarkerPathSchema.Pack(s),
            _ => null
        };
    }
}
