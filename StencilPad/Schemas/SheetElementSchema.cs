using System.Text.Json.Serialization;
using StencilPad.Models;

namespace StencilPad.Schemas;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ElementGroupSchema), "group")]
[JsonDerivedType(typeof(ShapeSchema), "shape")]
[JsonDerivedType(typeof(MarkerPathSchema), "markerpath")]
[JsonDerivedType(typeof(RulerSchema), "ruler")]
[JsonDerivedType(typeof(TextElementSchema), "text")]
[JsonDerivedType(typeof(ImageElementSchema), "image")]
public abstract class SheetElementSchema
{
    public abstract ISheetElement Unpack();

    public static SheetElementSchema? Pack(ISheetElement element)
    {
        return element switch
        {
            ElementGroup g => ElementGroupSchema.Pack(g),
            Shape s => ShapeSchema.Pack(s),
            MarkerPath s => MarkerPathSchema.Pack(s),
            Ruler s => RulerSchema.Pack(s),
            TextElement t => TextElementSchema.Pack(t),
            ImageElement i => ImageElementSchema.Pack(i),
            _ => null
        };
    }
}
